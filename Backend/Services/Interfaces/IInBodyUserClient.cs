namespace Backend.Services;

public interface IInBodyUserClient
{
    Task<InBodyUser?> GetUserAsync(string userToken, CancellationToken cancellationToken);
}

public sealed record InBodyUser(string Id, string Name, string Gender);

public sealed class InBodyApiException(string message, Exception? innerException = null)
    : Exception(message, innerException);
