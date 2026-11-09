namespace Backend.Services;

public interface IInBodyWebhookService
{
    Task<InBodyWebhookResult> HandleMeasurementAsync(
        string userToken,
        string equipSerial,
        string datetimes,
        CancellationToken cancellationToken
    );
}

public enum InBodyWebhookStatus
{
    Processed,
    Duplicate,
    MemberNotFound,
    InBodyApiError,
}

public sealed record InBodyWebhookResult(InBodyWebhookStatus Status);
