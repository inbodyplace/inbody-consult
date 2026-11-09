namespace Backend.Models;

public sealed record MemberMeasurementResponse(
    decimal? Smm,
    decimal? Bfm,
    decimal? Weight,
    decimal? Pbf);
