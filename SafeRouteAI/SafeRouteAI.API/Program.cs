
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using SafeRouteAI.API.Security;
using SafeRouteAI.API.Services;
using SafeRouteAI.Infrastructure;
using SafeRouteAI.Infrastructure.Data;
using SafeRouteAI.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// API controllers.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

// API documentation and error handling.
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Weather integration.
builder.Services.AddHttpClient("OpenMeteo", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Infrastructure registration.
// Preserve the existing routing integration behavior.
var routingProvider =
    builder.Configuration["RoutingIntegration:Provider"]
    ?? "Unavailable";

if (routingProvider.Trim().Equals(
    "Unavailable",
    StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddInfrastructure();
}

// ML integration.
builder.Services.AddMlIntegration(builder.Configuration);

// Routing integration.
builder.Services.AddRoutingIntegration(builder.Configuration);

// Session-based Bearer authentication.
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            SessionAuthenticationHandler.SchemeName;

        options.DefaultChallengeScheme =
            SessionAuthenticationHandler.SchemeName;
    })
    .AddScheme<AuthenticationSchemeOptions,
        SessionAuthenticationHandler>(
            SessionAuthenticationHandler.SchemeName,
            options => { });

// Role-based authorization.
builder.Services.AddAuthorization();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    app.Logger.LogInformation("Applying pending database migrations.");
    await db.Database.MigrateAsync();
    app.Logger.LogInformation("Database migrations are up to date.");

    if (app.Environment.IsDevelopment())
    {
        var testUsers = scope.ServiceProvider
            .GetRequiredService<DevelopmentTestUserSeeder>();
        await testUsers.SeedAsync();
        app.Logger.LogInformation("Development test users are available.");
    }
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Authentication must run before authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
