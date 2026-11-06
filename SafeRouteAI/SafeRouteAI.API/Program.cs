
using System.Text.Json.Serialization;
using SafeRouteAI.API.Services;
using SafeRouteAI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Weather integration.
builder.Services.AddHttpClient("OpenMeteo", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Register infrastructure separately only when
// routing is not using the Backend provider.
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
// Backend mode registers its own infrastructure.
builder.Services.AddRoutingIntegration(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();

public partial class Program { }
