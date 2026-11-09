using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Backend.Services;

public sealed class InBodyMeasurementClient(
    HttpClient httpClient,
    IConfiguration configuration,
    IInBodyApiQuotaService quotaService
)
    : IInBodyMeasurementClient
{
    private const string DefaultDatetimesUrl =
        "https://kr.developers.lookinbody.com/InBody/GetDatetimesByID";
    private const string DefaultDataUrl =
        "https://kr.developers.lookinbody.com/InBody/GetInBodyDataByID";

    public async Task<IReadOnlyList<string>> GetDatetimesByIdAsync(
        string userId,
        CancellationToken cancellationToken
    )
    {
        using var document = await PostAsync(
            configuration["InBody:GetDatetimesByIdUrl"] ?? DefaultDatetimesUrl,
            new { userID = userId },
            InBodyCallPurpose.MemberSync,
            cancellationToken
        );

        if (document is null)
            return [];
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new InBodyApiException("GetDatetimesByID response is not an array.");

        return document
            .RootElement.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()?.Trim() ?? string.Empty)
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    public async Task<InBodyMeasurementData?> GetInBodyDataByIdAsync(
        string userId,
        string datetimes,
        InBodyCallPurpose purpose,
        CancellationToken cancellationToken
    )
    {
        using var document = await PostAsync(
            configuration["InBody:GetInBodyDataByIdUrl"] ?? DefaultDataUrl,
            new { userID = userId, datetimes },
            purpose,
            cancellationToken
        );

        if (document is null)
            return null;

        var result = document.RootElement;
        if (result.ValueKind == JsonValueKind.Array)
        {
            result = result
                .EnumerateArray()
                .FirstOrDefault(item => ReadString(item, "DATETIMES") == datetimes);
        }
        if (result.ValueKind != JsonValueKind.Object)
            throw new InBodyApiException("GetInBodyDataByID response is missing a measurement object.");

        var timestamp = ReadString(result, "DATETIMES");
        var serial = ReadString(result, "EQUIP_SERIAL");
        if (string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(serial))
            throw new InBodyApiException(
                "GetInBodyDataByID response is missing measurement time or device serial number."
            );
        if (
            !DateTime.TryParseExact(
                timestamp,
                "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var measuredAt
            )
        )
            throw new InBodyApiException("GetInBodyDataByID DATETIMES format is invalid.");

        return new InBodyMeasurementData(
            measuredAt,
            serial,
            ReadDecimal(result, "SMM"),
            ReadDecimal(result, "BFM"),
            ReadDecimal(result, "WT"),
            ReadDecimal(result, "PBF")
        );
    }

    private async Task<JsonDocument?> PostAsync(
        string url,
        object payload,
        InBodyCallPurpose purpose,
        CancellationToken cancellationToken
    )
    {
        var apiKey = configuration["InBody:ApiKey"];
        var account = configuration["InBody:Account"];
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(account))
            throw new InBodyApiException("InBody API credentials are not configured.");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload),
            };
            request.Headers.Accept.Add(new("application/json"));
            request.Headers.Add("API-KEY", apiKey);
            request.Headers.Add("Account", account);

            if (purpose == InBodyCallPurpose.MemberSync)
                await quotaService.ReserveAsync(cancellationToken);
            using var response = await httpClient.SendAsync(request, cancellationToken);
            var document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken
            );

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                if (ReadString(document.RootElement, "code") == "40401")
                {
                    document.Dispose();
                    return null;
                }
            }
            if (!response.IsSuccessStatusCode)
            {
                document.Dispose();
                throw new InBodyApiException($"InBody API returned {(int)response.StatusCode}.");
            }
            return document;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (InBodyApiException)
        {
            throw;
        }
        catch (Exception exception)
            when (exception
                    is HttpRequestException
                        or TaskCanceledException
                        or JsonException
                        or InvalidOperationException
                        or UriFormatException
            )
        {
            throw new InBodyApiException(
                "InBody measurement API request or response processing failed.",
                exception
            );
        }
    }

    private static string? ReadString(JsonElement item, string name)
    {
        if (item.ValueKind != JsonValueKind.Object)
            return null;
        foreach (var property in item.EnumerateObject())
        {
            if (
                string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)
                && property.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number
            )
                return property.Value.ToString().Trim();
        }
        return null;
    }

    private static decimal? ReadDecimal(JsonElement item, string name)
    {
        var value = ReadString(item, name);
        return decimal.TryParse(
            value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var parsed
        )
            ? parsed
            : null;
    }
}
