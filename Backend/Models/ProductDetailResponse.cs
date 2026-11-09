namespace Backend.Models;

public sealed record ProductDetailResponse(
    int Id,
    string Name,
    DateOnly RegisteredAt,
    int MemberCount,
    ProductStatisticsResponse Stats);
