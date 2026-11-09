using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Backend.Services;

public enum InBodyCallPurpose
{
    MemberSync,
    Webhook,
}

public sealed class InBodyQuotaExceededException(DateTimeOffset resetAtUtc)
    : Exception("The daily InBody API call limit has been reached.")
{
    public DateTimeOffset ResetAtUtc { get; } = resetAtUtc;
}

public interface IInBodyApiQuotaService
{
    Task ReserveAsync(CancellationToken cancellationToken);
}

public sealed class InBodyApiQuotaService : IInBodyApiQuotaService
{
    private const string ReserveSql = """
        SET NOCOUNT ON;
        DECLARE @lockResult int;
        EXEC @lockResult = sp_getapplock
            @Resource = @resource,
            @LockMode = 'Exclusive',
            @LockOwner = 'Transaction',
            @LockTimeout = 10000;
        IF @lockResult < 0 THROW 50001, 'Could not acquire the InBody API quota lock.', 1;

        DECLARE @used int;
        SELECT @used = [CallsUsed]
        FROM [dbo].[InBodyApiUsage]
        WHERE [AccountKey] = @accountKey AND [QuotaDate] = @quotaDate;

        IF @used IS NULL
        BEGIN
            INSERT INTO [dbo].[InBodyApiUsage] ([AccountKey], [QuotaDate], [CallsUsed])
            VALUES (@accountKey, @quotaDate, 1);
            SELECT CAST(1 AS bit);
        END
        ELSE IF @used >= @limit
            SELECT CAST(0 AS bit);
        ELSE
        BEGIN
            UPDATE [dbo].[InBodyApiUsage]
            SET [CallsUsed] = [CallsUsed] + 1
            WHERE [AccountKey] = @accountKey AND [QuotaDate] = @quotaDate;
            SELECT CAST(1 AS bit);
        END
        """;

    private readonly string connectionString;
    private readonly string accountKey;
    private readonly TimeZoneInfo quotaTimeZone;
    private readonly int dailyLimit;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<InBodyApiQuotaService> logger;

    public InBodyApiQuotaService(
        IConfiguration configuration,
        TimeProvider timeProvider,
        ILogger<InBodyApiQuotaService> logger
    )
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("MSSQL connection string is missing.");
        var account = configuration["InBody:Account"] ?? string.Empty;
        accountKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(account)));
        quotaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            configuration["InBody:QuotaTimeZone"] ?? "UTC"
        );
        dailyLimit = configuration.GetValue<int?>("InBody:DailyCallLimit") ?? 500;
        if (dailyLimit is < 1 or > 500)
            throw new InvalidOperationException("The InBody daily call limit must be between 1 and 500.");
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public async Task ReserveAsync(CancellationToken cancellationToken)
    {
        var localNow = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), quotaTimeZone);
        var quotaDate = DateOnly.FromDateTime(localNow.Date);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken
        );
        await using var command = new SqlCommand(ReserveSql, connection, transaction);
        command.Parameters.Add("@resource", SqlDbType.NVarChar, 255).Value =
            $"InBodyApiQuota:{accountKey}:{quotaDate:yyyyMMdd}";
        command.Parameters.Add("@accountKey", SqlDbType.Char, 64).Value = accountKey;
        command.Parameters.Add("@quotaDate", SqlDbType.Date).Value =
            quotaDate.ToDateTime(TimeOnly.MinValue);
        command.Parameters.Add("@limit", SqlDbType.Int).Value = dailyLimit;

        var reserved = (bool)(await command.ExecuteScalarAsync(cancellationToken) ?? false);
        await transaction.CommitAsync(cancellationToken);
        if (reserved)
            return;

        logger.LogWarning(
            "InBody API quota exhausted for {QuotaDate}",
            quotaDate
        );
        var nextLocalMidnight = quotaDate.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var resetAtUtc = TimeZoneInfo.ConvertTimeToUtc(nextLocalMidnight, quotaTimeZone);
        throw new InBodyQuotaExceededException(new DateTimeOffset(resetAtUtc));
    }
}
