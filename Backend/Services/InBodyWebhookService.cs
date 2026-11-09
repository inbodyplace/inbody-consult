using System.Data;
using System.Globalization;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public sealed class InBodyWebhookService(
    ApplicationDbContext dbContext,
    IInBodyMeasurementClient inBodyMeasurementClient,
    ILogger<InBodyWebhookService> logger
) : IInBodyWebhookService
{
    public async Task<InBodyWebhookResult> HandleMeasurementAsync(
        string userToken,
        string equipSerial,
        string datetimes,
        CancellationToken cancellationToken
    )
    {
        var member = await dbContext
            .Members.AsNoTracking()
            .SingleOrDefaultAsync(item => item.UserToken == userToken, cancellationToken);
        if (member is null)
        {
            logger.LogWarning(
                "Webhook member not found for token ending {UserTokenSuffix}",
                TokenSuffix(userToken)
            );
            return new(InBodyWebhookStatus.MemberNotFound);
        }

        var measuredAt = DateTime.ParseExact(
            datetimes,
            "yyyyMMddHHmmss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None
        );
        if (await HasMeasurementAsync(member.Id, measuredAt, equipSerial, cancellationToken))
        {
            logger.LogInformation(
                "Webhook duplicate ignored for measurement at {MeasuredAt} from device {EquipSerial}",
                measuredAt,
                equipSerial
            );
            return new(InBodyWebhookStatus.Duplicate);
        }

        InBodyMeasurementData? data;
        try
        {
            data = await inBodyMeasurementClient.GetInBodyDataByIdAsync(
                member.Id,
                datetimes,
                InBodyCallPurpose.Webhook,
                cancellationToken
            );
        }
        catch (InBodyApiException exception)
        {
            logger.LogWarning(
                exception,
                "Webhook measurement lookup failed for member {MemberId}",
                member.Id
            );
            return new(InBodyWebhookStatus.InBodyApiError);
        }

        if (data is null)
        {
            logger.LogWarning(
                "Webhook measurement not found in InBody for measurement at {MeasuredAt} from device {EquipSerial}",
                measuredAt,
                equipSerial
            );
            return new(InBodyWebhookStatus.InBodyApiError);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken
        );

        if (
            await HasMeasurementAsync(
                member.Id,
                data.MeasuredAt,
                data.EquipSerial,
                cancellationToken
            )
        )
        {
            logger.LogInformation(
                "Webhook duplicate ignored after InBody lookup for measurement at {MeasuredAt} from device {EquipSerial}",
                data.MeasuredAt,
                data.EquipSerial
            );
            return new(InBodyWebhookStatus.Duplicate);
        }

        var measurement = new InBodyMeasurement
        {
            MemberId = member.Id,
            MeasuredAt = data.MeasuredAt,
            EquipSerial = data.EquipSerial,
            Smm = data.Smm,
            Bfm = data.Bfm,
            Weight = data.Weight,
            Pbf = data.Pbf,
        };
        dbContext.InBodyMeasurements.Add(measurement);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Update each program member’s latest measurement pair
        var links = await dbContext
            .ProductMembers.Where(link => link.MemberId == member.Id)
            .ToListAsync(cancellationToken);
        if (links.Count > 0)
        {
            var measurements = await dbContext
                .InBodyMeasurements.Where(item => item.MemberId == member.Id)
                .ToListAsync(cancellationToken);
            var pair = SelectLatestPair(measurements);
            foreach (var link in links)
            {
                link.BeforeMeasurementId = pair.Before?.Id;
                link.AfterMeasurementId = pair.After?.Id;
            }
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation(
            "Webhook measurement synchronized at {MeasuredAt} from device {EquipSerial}; refreshed {ProductMemberCount} product links",
            data.MeasuredAt,
            data.EquipSerial,
            links.Count
        );
        return new(InBodyWebhookStatus.Processed);
    }

    private static string TokenSuffix(string userToken) =>
        userToken.Length <= 4 ? "****" : userToken[^4..];

    private Task<bool> HasMeasurementAsync(
        string memberId,
        DateTime measuredAt,
        string equipSerial,
        CancellationToken cancellationToken
    ) =>
        dbContext.InBodyMeasurements.AnyAsync(
            item =>
                item.MemberId == memberId
                && item.MeasuredAt == measuredAt
                && item.EquipSerial == equipSerial,
            cancellationToken
        );

    private static (InBodyMeasurement? Before, InBodyMeasurement? After) SelectLatestPair(
        IReadOnlyCollection<InBodyMeasurement> measurements
    )
    {
        var ordered = measurements.OrderBy(item => item.MeasuredAt).ToArray();
        if (ordered.Length == 0)
            return (null, null);

        var after = ordered[^1];
        var before = ordered
            .Where(item => item.MeasuredAt < after.MeasuredAt)
            .OrderByDescending(item => item.MeasuredAt)
            .FirstOrDefault();
        return (before ?? after, after);
    }
}
