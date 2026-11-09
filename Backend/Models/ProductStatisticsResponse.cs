using System.Text.Json.Serialization;

namespace Backend.Models;

public sealed record ProductStatisticsGroupResponse(
    int CompletedCount,
    MeasurementChangeResponse? MonthlyAverageChange);

public sealed record ProductStatisticsResponse(
    ProductStatisticsGroupResponse Overall,
    [property: JsonPropertyName("M")]
    ProductStatisticsGroupResponse M,
    [property: JsonPropertyName("F")]
    ProductStatisticsGroupResponse F);
