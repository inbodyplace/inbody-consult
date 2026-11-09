namespace Backend.Services;

public interface IInBodyMeasurementClient
{
    Task<IReadOnlyList<string>> GetDatetimesByIdAsync(
        string userId,
        CancellationToken cancellationToken
    );
    Task<InBodyMeasurementData?> GetInBodyDataByIdAsync(
        string userId,
        string datetimes,
        InBodyCallPurpose purpose,
        CancellationToken cancellationToken
    );
}

public sealed record InBodyMeasurementData(
    DateTime MeasuredAt,
    string EquipSerial,
    decimal? Smm,
    decimal? Bfm,
    decimal? Weight,
    decimal? Pbf
);
