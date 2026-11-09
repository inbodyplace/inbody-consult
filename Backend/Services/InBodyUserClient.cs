using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Backend.Services;

public sealed class InBodyUserClient(
    HttpClient httpClient,
    IConfiguration configuration,
    IInBodyApiQuotaService quotaService
)
    : IInBodyUserClient
{
    public async Task<InBodyUser?> GetUserAsync(
        string userToken,
        CancellationToken cancellationToken
    )
    {
        var url = configuration["InBody:GetUserUrl"];
        var apiKey = configuration["InBody:ApiKey"];
        var account = configuration["InBody:Account"];
        if (
            string.IsNullOrWhiteSpace(url)
            || string.IsNullOrWhiteSpace(apiKey)
            || string.IsNullOrWhiteSpace(account)
        )
            throw new InBodyApiException("GetUser URL or credentials are not configured.");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(new { userToken }),
            };
            request.Headers.Accept.Add(new("application/json"));
            request.Headers.Add("API-KEY", apiKey);
            request.Headers.Add("Account", account);

            await quotaService.ReserveAsync(cancellationToken);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                using var notFound = await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    cancellationToken: cancellationToken
                );
                if (Read(notFound.RootElement, "code") == "40401")
                    return null;
            }
            if (!response.IsSuccessStatusCode)
                throw new InBodyApiException($"GetUser returned {(int)response.StatusCode}.");

            using var document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken
            );
            var users = document.RootElement;
            if (users.ValueKind != JsonValueKind.Array)
                throw new InBodyApiException("GetUser response is not an array.");

            foreach (var candidate in users.EnumerateArray())
            {
                var token = Digits(Read(candidate, "userToken") ?? Read(candidate, "phone"));
                if (token != userToken)
                    continue;

                var id = Read(candidate, "iD");
                var name = Read(candidate, "name");
                var gender = Read(candidate, "gender")?.ToUpperInvariant();
                if (
                    string.IsNullOrWhiteSpace(id)
                    || string.IsNullOrWhiteSpace(name)
                    || gender is not ("M" or "F")
                )
                    throw new InBodyApiException(
                        "GetUser response is missing member ID, name, or gender."
                    );

                return new InBodyUser(id, name, gender);
            }

            return null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
            when (exception
                    is HttpRequestException
                        or TaskCanceledException
                        or JsonException
                        or InvalidOperationException
            )
        {
            throw new InBodyApiException("GetUser request or response processing failed.", exception);
        }
    }

    private static string? Read(JsonElement item, string name)
    {
        if (item.ValueKind != JsonValueKind.Object)
            return null;
        foreach (var property in item.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                return property.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number
                    ? property.Value.ToString().Trim()
                    : null;
        }
        return null;
    }

    private static string Digits(string? value) =>
        new((value ?? string.Empty).Where(char.IsAsciiDigit).ToArray());
}
