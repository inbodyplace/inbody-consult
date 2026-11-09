using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public sealed class ProductService(ApplicationDbContext dbContext) : IProductService
{
    public async Task<IReadOnlyList<ProductListItemResponse>> GetProductsAsync(
        CancellationToken cancellationToken)
    {
        var products = await dbContext.Products.AsNoTracking()
            .OrderBy(product => product.Id)
            .ToListAsync(cancellationToken);
        if (products.Count == 0)
            return [];

        var productIds = products.Select(product => product.Id).ToArray();
        var memberships = await (
            from link in dbContext.ProductMembers.AsNoTracking()
            join member in dbContext.Members.AsNoTracking() on link.MemberId equals member.Id
            where productIds.Contains(link.ProductId)
            select new ProductMembership(link.ProductId, link.MemberId, member.Gender))
            .ToListAsync(cancellationToken);
        var measurementsByMember = await LoadMeasurementsAsync(
            memberships.Select(link => link.MemberId), cancellationToken);

        return products.Select(product =>
        {
            var memberIds = memberships
                .Where(link => link.ProductId == product.Id)
                .DistinctBy(link => link.MemberId, StringComparer.Ordinal)
                .ToArray();
            var statistics = CalculateStatistics(memberIds, measurementsByMember);
            return new ProductListItemResponse(
                product.Id,
                product.Name,
                product.RegisteredAt,
                memberIds.Length,
                statistics.Overall.CompletedCount,
                statistics.Overall.MonthlyAverageChange);
        }).ToArray();
    }

    public async Task<ProductDetailResponse?> GetProductAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == productId, cancellationToken);
        if (product is null)
            return null;

        var memberships = await (
            from link in dbContext.ProductMembers.AsNoTracking()
            join member in dbContext.Members.AsNoTracking() on link.MemberId equals member.Id
            where link.ProductId == productId
            select new ProductMembership(link.ProductId, link.MemberId, member.Gender))
            .ToListAsync(cancellationToken);
        var measurementsByMember = await LoadMeasurementsAsync(
            memberships.Select(link => link.MemberId), cancellationToken);
        var statistics = CalculateStatistics(memberships, measurementsByMember);

        return new ProductDetailResponse(
            product.Id,
            product.Name,
            product.RegisteredAt,
            memberships.Select(link => link.MemberId).Distinct(StringComparer.Ordinal).Count(),
            statistics);
    }

    public async Task<ProductResponse> CreateProductAsync(
        string name,
        CancellationToken cancellationToken)
    {
        // Normalize the program name and save its creation date
        var product = new Product
        {
            Name = name.Trim(),
            RegisteredAt = DateOnly.FromDateTime(DateTime.Today),
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ProductResponse(product.Id, product.Name, product.RegisteredAt);
    }

    private async Task<Dictionary<string, List<InBodyMeasurement>>> LoadMeasurementsAsync(
        IEnumerable<string> memberIds,
        CancellationToken cancellationToken)
    {
        var ids = memberIds.Distinct(StringComparer.Ordinal).ToArray();
        if (ids.Length == 0)
            return new(StringComparer.Ordinal);

        var measurements = await dbContext.InBodyMeasurements.AsNoTracking()
            .Where(item => ids.Contains(item.MemberId))
            .ToListAsync(cancellationToken);
        return measurements
            .GroupBy(item => item.MemberId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);
    }

    private static ProductStatisticsResponse CalculateStatistics(
        IEnumerable<ProductMembership> memberships,
        IReadOnlyDictionary<string, List<InBodyMeasurement>> measurementsByMember)
    {
        var overall = new List<MeasurementChangeResponse>();
        var male = new List<MeasurementChangeResponse>();
        var female = new List<MeasurementChangeResponse>();

        foreach (var member in memberships.DistinctBy(
            membership => membership.MemberId, StringComparer.Ordinal))
        {
            if (!measurementsByMember.TryGetValue(member.MemberId, out var measurements))
                continue;

            var pair = MeasurementPairSelector.FindCompletedPair(measurements);
            if (pair is null)
                continue;

            var change = CalculateMonthlyChange(pair.Value.Before, pair.Value.After);
            overall.Add(change);
            if (member.Gender == "M")
                male.Add(change);
            else if (member.Gender == "F")
                female.Add(change);
        }

        return new ProductStatisticsResponse(
            Summarize(overall),
            Summarize(male),
            Summarize(female));
    }

    private static MeasurementChangeResponse CalculateMonthlyChange(
        InBodyMeasurement before,
        InBodyMeasurement after)
    {
        var days = (decimal)(after.MeasuredAt - before.MeasuredAt).TotalDays;
        return new MeasurementChangeResponse(
            (after.Smm!.Value - before.Smm!.Value) * 30m / days,
            (after.Bfm!.Value - before.Bfm!.Value) * 30m / days,
            (after.Weight!.Value - before.Weight!.Value) * 30m / days,
            (after.Pbf!.Value - before.Pbf!.Value) * 30m / days);
    }

    private static ProductStatisticsGroupResponse Summarize(
        IReadOnlyCollection<MeasurementChangeResponse> changes)
    {
        if (changes.Count == 0)
            return new ProductStatisticsGroupResponse(0, null);

        var count = changes.Count;
        return new ProductStatisticsGroupResponse(
            count,
            new MeasurementChangeResponse(
                changes.Sum(change => change.Smm) / count,
                changes.Sum(change => change.Bfm) / count,
                changes.Sum(change => change.Weight) / count,
                changes.Sum(change => change.Pbf) / count));
    }

    private sealed record ProductMembership(int ProductId, string MemberId, string Gender);
}
