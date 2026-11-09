namespace Backend.Models;

public sealed record SimilarMemberResponse(
    string Id,
    string Name,
    string Gender,
    DateOnly StartDate,
    DateOnly EndDate,
    int Months,
    SimilarMemberMeasurementsResponse Before,
    SimilarMemberMeasurementsResponse After);

public sealed record SimilarMemberMeasurementsResponse(
    decimal Smm,
    decimal Bfm,
    decimal Weight,
    decimal Pbf);
