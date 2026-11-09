using Backend.Data;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Local MSSQL connection settings excluded from the repository
builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("MSSQL connection string is missing. Check Backend/secrets.json.");
}

// Register API, Swagger, and EF Core services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IInBodyApiQuotaService, InBodyApiQuotaService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductMemberService, ProductMemberService>();
builder.Services.AddHttpClient<IInBodyUserClient, InBodyUserClient>();
builder.Services.AddHttpClient<IInBodyMeasurementClient, InBodyMeasurementClient>();
builder.Services.AddScoped<IInBodyWebhookService, InBodyWebhookService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// Check server and MSSQL connection status
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/health/database", async (ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
    await dbContext.Database.CanConnectAsync(cancellationToken)
        ? Results.Ok(new { status = "ok", database = "connected" })
        : Results.Json(new { status = "unavailable", database = "disconnected" }, statusCode: StatusCodes.Status503ServiceUnavailable));

app.Run();
