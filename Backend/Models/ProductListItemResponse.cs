namespace Backend.Models;

public sealed record ProductListItemResponse(
    int Id,
    string Name,
    DateOnly RegisteredAt,
    int MemberCount,
    int CompletedCount,
    MeasurementChangeResponse? MonthlyAverageChange);
