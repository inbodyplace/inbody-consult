namespace Backend.Models;

public sealed record ProductMemberResponse(
    string Id,
    string Name,
    string Gender,
    DateOnly? StartDate,
    DateOnly? EndDate,
    MemberMeasurementResponse? Before,
    MemberMeasurementResponse? After,
    bool IncludedInAverage = false);
