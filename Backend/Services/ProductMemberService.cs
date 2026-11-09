using System.Data;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public sealed class ProductMemberService(
    ApplicationDbContext dbContext,
    IInBodyUserClient inBodyUserClient,
    IInBodyMeasurementClient inBodyMeasurementClient,
    ILogger<ProductMemberService> logger
) : IProductMemberService
{
    public async Task<SimilarMembersResult> SearchSimilarMembersAsync(
        int productId,
        string gender,
        SimilarMemberCurrentRequest current,
        CancellationToken cancellationToken
    )
    {
        // Retrieve initial and follow-up measurements for program members
        if (
            !await dbContext.Products.AnyAsync(
                product => product.Id == productId,
                cancellationToken
            )
        )
            return new(SimilarMembersStatus.ProductNotFound);

        var members = await (
            from link in dbContext.ProductMembers.AsNoTracking()
            join member in dbContext.Members.AsNoTracking() on link.MemberId equals member.Id
            where link.ProductId == productId && member.Gender == gender
            select new { member.Id, member.Gender })
            .ToListAsync(cancellationToken);
        var memberIds = members.Select(member => member.Id).Distinct().ToArray();
        var measurements = memberIds.Length == 0
            ? []
            : await dbContext.InBodyMeasurements.AsNoTracking()
                .Where(measurement => memberIds.Contains(measurement.MemberId))
                .ToListAsync(cancellationToken);
        var measurementsByMember = measurements
            .GroupBy(measurement => measurement.MemberId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        // Calculate similarity from the initial values of qualifying measurement pairs
        var candidates = members
            .Select(member => new
            {
                Member = member,
                Pair = measurementsByMember.TryGetValue(member.Id, out var history)
                    ? MeasurementPairSelector.FindCompletedPair(history)
                    : null,
            })
            .Where(candidate => candidate.Pair is not null)
            .Select(candidate => new
            {
                candidate.Member,
                Pair = candidate.Pair!.Value,
                Distance = SimilarityDistance(candidate.Pair.Value.Before, current),
            });

        var results = candidates
            .OrderBy(result => result.Distance)
            .ThenBy(result => result.Member.Id)
            .Take(3)
            .Select((result, index) =>
            {
                var before = result.Pair.Before;
                var after = result.Pair.After;
                var elapsedDays = (after.MeasuredAt - before.MeasuredAt).TotalDays;
                var months = Math.Max(1, (int)Math.Round(elapsedDays / 30, MidpointRounding.AwayFromZero));
                return new SimilarMemberResponse(
                    $"similar-{index + 1}",
                    $"Member {index + 1}",
                    result.Member.Gender,
                    DateOnly.FromDateTime(before.MeasuredAt),
                    DateOnly.FromDateTime(after.MeasuredAt),
                    months,
                    ToSimilarMemberMeasurements(before),
                    ToSimilarMemberMeasurements(after)
                );
            })
            .ToArray();

        return new(SimilarMembersStatus.Success, results);
    }

    public async Task<RemoveMemberStatus> RemoveMemberAsync(
        int productId,
        string memberId,
        CancellationToken cancellationToken
    )
    {
        // Check member associations by program
        if (
            !await dbContext.Products.AnyAsync(
                product => product.Id == productId,
                cancellationToken
            )
        )
            return RemoveMemberStatus.ProductNotFound;

        var link = await dbContext.ProductMembers.SingleOrDefaultAsync(
            item => item.ProductId == productId && item.MemberId == memberId,
            cancellationToken
        );
        if (link is null)
            return RemoveMemberStatus.ProductMemberNotFound;

        // Remove only the association; retain member and measurement records
        dbContext.ProductMembers.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);
        return RemoveMemberStatus.Removed;
    }

    public async Task<GetMembersResult> GetMembersAsync(
        int productId,
        int page,
        int size,
        CancellationToken cancellationToken
    )
    {
        // Page program members by most recent addition
        if (
            !await dbContext.Products.AnyAsync(
                product => product.Id == productId,
                cancellationToken
            )
        )
            return new(GetMembersStatus.ProductNotFound);

        var membersQuery = dbContext
            .ProductMembers.AsNoTracking()
            .Where(link => link.ProductId == productId)
            .Join(
                dbContext.Members.AsNoTracking(),
                link => link.MemberId,
                member => member.Id,
                (link, member) =>
                    new
                    {
                        link.MemberId,
                        member.Name,
                        member.Gender,
                        link.AddedAt,
                        link.BeforeMeasurementId,
                        link.AfterMeasurementId,
                    }
            );

        var totalItems = await membersQuery.CountAsync(cancellationToken);
        var totalPages = totalItems == 0 ? 0 : (int)(((long)totalItems + size - 1) / size);
        var skip = ((long)page - 1) * size;

        var rows =
            skip > int.MaxValue
                ? []
                : await membersQuery
                    .OrderByDescending(row => row.AddedAt)
                    .ThenBy(row => row.MemberId)
                    .Skip((int)skip)
                    .Take(size)
                    .ToListAsync(cancellationToken);

        // Retrieve measurement history and qualifying pairs for paged members
        var memberIds = rows.Select(row => row.MemberId).Distinct().ToArray();
        var pageMeasurements = memberIds.Length == 0
            ? []
            : await dbContext.InBodyMeasurements.AsNoTracking()
                .Where(measurement => memberIds.Contains(measurement.MemberId))
                .ToListAsync(cancellationToken);
        var measurementsByMember = pageMeasurements
            .GroupBy(measurement => measurement.MemberId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        var measurementsById = pageMeasurements.ToDictionary(measurement => measurement.Id);

        // Build the response from linked measurements
        var items = rows.Select(row =>
            {
                measurementsById.TryGetValue(row.BeforeMeasurementId ?? 0, out var linkedBefore);
                measurementsById.TryGetValue(row.AfterMeasurementId ?? 0, out var linkedAfter);
                measurementsByMember.TryGetValue(row.MemberId, out var memberMeasurements);
                var completedPair = MeasurementPairSelector.FindCompletedPair(memberMeasurements ?? []);
                var before = completedPair?.Before ?? linkedBefore;
                var after = completedPair?.After ?? linkedAfter;
                return new ProductMemberResponse(
                    row.MemberId,
                    row.Name,
                    row.Gender,
                    before is null ? null : DateOnly.FromDateTime(before.MeasuredAt),
                    after is null ? null : DateOnly.FromDateTime(after.MeasuredAt),
                    before is null
                        ? null
                        : new MemberMeasurementResponse(
                            before.Smm,
                            before.Bfm,
                            before.Weight,
                            before.Pbf
                        ),
                    after is null
                        ? null
                        : new MemberMeasurementResponse(
                            after.Smm,
                            after.Bfm,
                            after.Weight,
                            after.Pbf
                        ),
                    completedPair is not null
                );
            })
            .ToArray();

        return new(
            GetMembersStatus.Success,
            new PagedProductMembersResponse(items, page, size, totalItems, totalPages)
        );
    }

    public async Task<AddMemberResult> AddMemberAsync(
        int productId,
        string userToken,
        CancellationToken cancellationToken
    )
    {
        if (
            !await dbContext.Products.AnyAsync(
                product => product.Id == productId,
                cancellationToken
            )
        )
            return new(AddMemberStatus.ProductNotFound);

        if (await IsAlreadyRegisteredAsync(productId, userToken, cancellationToken))
            return new(AddMemberStatus.AlreadyRegistered);

        InBodyUser? inBodyUser;
        try
        {
            inBodyUser = await inBodyUserClient.GetUserAsync(userToken, cancellationToken);
        }
        catch (InBodyQuotaExceededException)
        {
            return new(AddMemberStatus.QuotaExceeded);
        }
        catch (InBodyApiException exception)
        {
            logger.LogWarning(exception, "GetUser failed for product {ProductId}", productId);
            return new(AddMemberStatus.InBodyApiError);
        }

        if (inBodyUser is null)
            return new(AddMemberStatus.InBodyMemberNotFound);

        // Reuse measurements already stored for this member across programs.
        var storedMeasurements = await dbContext
            .InBodyMeasurements.AsNoTracking()
            .Where(measurement => measurement.MemberId == inBodyUser.Id)
            .ToListAsync(cancellationToken);
        var initialMeasurements = new List<InBodyMeasurementData>();
        if (storedMeasurements.Count == 0)
        {
            try
            {
                initialMeasurements = await FetchInitialPairCandidatesAsync(
                    inBodyUser.Id,
                    cancellationToken
                );
            }
            catch (InBodyQuotaExceededException)
            {
                return new(AddMemberStatus.QuotaExceeded);
            }
            catch (InBodyApiException exception)
            {
                logger.LogWarning(
                    exception,
                    "Initial measurement sync failed for product {ProductId}",
                    productId
                );
                return new(AddMemberStatus.InBodyApiError);
            }
        }

        // Check again before saving in case another request registered the member during GetUser
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken
        );

        if (
            !await dbContext.Products.AnyAsync(
                product => product.Id == productId,
                cancellationToken
            )
        )
            return new(AddMemberStatus.ProductNotFound);

        if (await IsAlreadyRegisteredAsync(productId, userToken, cancellationToken))
            return new(AddMemberStatus.AlreadyRegistered);

        if (
            await dbContext.ProductMembers.AnyAsync(
                link => link.ProductId == productId && link.MemberId == inBodyUser.Id,
                cancellationToken
            )
        )
            return new(AddMemberStatus.AlreadyRegistered);

        var memberByToken = await dbContext.Members.SingleOrDefaultAsync(
            item => item.UserToken == userToken,
            cancellationToken
        );
        if (memberByToken is not null && memberByToken.Id != inBodyUser.Id)
            return new(AddMemberStatus.MemberIdentityConflict);

        var member =
            await dbContext.Members.FindAsync([inBodyUser.Id], cancellationToken) ?? memberByToken;

        if (member is null)
        {
            member = new Member { Id = inBodyUser.Id, UserToken = userToken };
            dbContext.Members.Add(member);
        }

        member.UserToken = userToken;
        member.Name = inBodyUser.Name;
        member.Gender = inBodyUser.Gender;

        var measurements = await dbContext
            .InBodyMeasurements.Where(measurement => measurement.MemberId == member.Id)
            .ToListAsync(cancellationToken);
        var measurementsByKey = measurements.ToDictionary(measurement =>
            (measurement.MeasuredAt, measurement.EquipSerial)
        );
        foreach (var incoming in initialMeasurements)
        {
            if (
                measurementsByKey.TryGetValue(
                    (incoming.MeasuredAt, incoming.EquipSerial),
                    out var existing
                )
            )
            {
                existing.Smm = incoming.Smm;
                existing.Bfm = incoming.Bfm;
                existing.Weight = incoming.Weight;
                existing.Pbf = incoming.Pbf;
                continue;
            }

            var created = new InBodyMeasurement
            {
                MemberId = member.Id,
                MeasuredAt = incoming.MeasuredAt,
                EquipSerial = incoming.EquipSerial,
                Smm = incoming.Smm,
                Bfm = incoming.Bfm,
                Weight = incoming.Weight,
                Pbf = incoming.Pbf,
            };
            dbContext.InBodyMeasurements.Add(created);
            measurements.Add(created);
            measurementsByKey.Add((created.MeasuredAt, created.EquipSerial), created);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Link the two latest measurements; apply the 30-day completion rule in statistics
        var pair = SelectMeasurementPair(measurements);

        dbContext.ProductMembers.Add(
            new ProductMember
            {
                ProductId = productId,
                MemberId = member.Id,
                AddedAt = DateTime.UtcNow,
                BeforeMeasurementId = pair.Before?.Id,
                AfterMeasurementId = pair.After?.Id,
            }
        );
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // Prefer the pair that qualifies for average calculations
        var completedPair = MeasurementPairSelector.FindCompletedPair(measurements);
        var responseBefore = completedPair?.Before ?? pair.Before;
        var responseAfter = completedPair?.After ?? pair.After;
        return new(
            AddMemberStatus.Created,
            new ProductMemberResponse(
                member.Id,
                member.Name,
                member.Gender,
                responseBefore is null ? null : DateOnly.FromDateTime(responseBefore.MeasuredAt),
                responseAfter is null ? null : DateOnly.FromDateTime(responseAfter.MeasuredAt),
                responseBefore is null ? null : ToMemberMeasurementResponse(responseBefore),
                responseAfter is null ? null : ToMemberMeasurementResponse(responseAfter),
                completedPair is not null
            )
        );
    }

    private async Task<List<InBodyMeasurementData>> FetchInitialPairCandidatesAsync(
        string memberId,
        CancellationToken cancellationToken
    )
    {
        var datetimes = await inBodyMeasurementClient.GetDatetimesByIdAsync(
            memberId,
            cancellationToken
        );
        var dates = datetimes.Select(value =>
        {
            if (!DateTime.TryParseExact(
                    value,
                    "yyyyMMddHHmmss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var date
                ))
                throw new InBodyApiException("GetDatetimesByID contains an invalid measurement time.");
            return date;
        }).Distinct().OrderByDescending(date => date).ToArray();

        var fetched = new Dictionary<DateTime, InBodyMeasurementData?>();
        var selected = new List<InBodyMeasurementData>();

        async Task<InBodyMeasurementData?> GetAtAsync(DateTime date)
        {
            if (fetched.TryGetValue(date, out var cached))
                return cached;

            var measurement = await inBodyMeasurementClient.GetInBodyDataByIdAsync(
                memberId,
                date.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture),
                InBodyCallPurpose.MemberSync,
                cancellationToken
            );
            fetched.Add(date, measurement);
            if (measurement is not null)
                selected.Add(measurement);
            return measurement;
        }

        if (dates.Length == 0)
            return selected;

        // Retain the latest measurement even when no qualifying 30-day pair exists yet.
        await GetAtAsync(dates[0]);

        foreach (var afterDate in dates)
        {
            var beforeDates = dates.Where(date => (afterDate - date).TotalDays >= 30);
            if (!beforeDates.Any())
                continue;

            var after = await GetAtAsync(afterDate);
            if (after is null || !IsComplete(after))
                continue;

            foreach (var beforeDate in beforeDates)
            {
                var before = await GetAtAsync(beforeDate);
                if (before is not null && IsComplete(before))
                    return selected;
            }
        }

        return selected;
    }

    private static bool IsComplete(InBodyMeasurementData measurement) =>
        measurement.Smm.HasValue
        && measurement.Bfm.HasValue
        && measurement.Weight.HasValue
        && measurement.Pbf.HasValue;

    private Task<bool> IsAlreadyRegisteredAsync(
        int productId,
        string userToken,
        CancellationToken cancellationToken
    ) =>
        dbContext.ProductMembers.AnyAsync(
            link =>
                link.ProductId == productId
                && dbContext.Members.Any(member =>
                    member.Id == link.MemberId && member.UserToken == userToken
                ),
            cancellationToken
        );

    private static double SimilarityDistance(
        InBodyMeasurement measurement,
        SimilarMemberCurrentRequest current
    )
    {
        var smmDelta = ((double)(measurement.Smm!.Value - current.Smm!.Value)) / 5;
        var bfmDelta = ((double)(measurement.Bfm!.Value - current.Bfm!.Value)) / 5;
        var weightDelta = ((double)(measurement.Weight!.Value - current.Weight!.Value)) / 10;
        var pbfDelta = ((double)(measurement.Pbf!.Value - current.Pbf!.Value)) / 5;
        return Math.Pow(smmDelta, 2)
            + Math.Pow(bfmDelta, 2)
            + Math.Pow(weightDelta, 2)
            + Math.Pow(pbfDelta, 2);
    }

    private static SimilarMemberMeasurementsResponse ToSimilarMemberMeasurements(
        InBodyMeasurement measurement
    ) =>
        new(
            measurement.Smm!.Value,
            measurement.Bfm!.Value,
            measurement.Weight!.Value,
            measurement.Pbf!.Value
        );

    private static bool IsComplete(InBodyMeasurement measurement) =>
        measurement.Smm.HasValue
        && measurement.Bfm.HasValue
        && measurement.Weight.HasValue
        && measurement.Pbf.HasValue;

    private static (InBodyMeasurement? Before, InBodyMeasurement? After) SelectMeasurementPair(
        IReadOnlyCollection<InBodyMeasurement> measurements
    )
    {
        var ordered = measurements.OrderBy(measurement => measurement.MeasuredAt).ToArray();
        if (ordered.Length == 0)
            return (null, null);

        var after = ordered[^1];
        var before = ordered
            .Where(measurement => measurement.MeasuredAt < after.MeasuredAt)
            .OrderByDescending(measurement => measurement.MeasuredAt)
            .FirstOrDefault();
        return (before ?? after, after);
    }

    private static MemberMeasurementResponse ToMemberMeasurementResponse(
        InBodyMeasurement measurement
    ) => new(measurement.Smm, measurement.Bfm, measurement.Weight, measurement.Pbf);
}
