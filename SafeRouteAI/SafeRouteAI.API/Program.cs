
using System.Text.Json.Serialization;
using SafeRouteAI.API.Interfaces;
using SafeRouteAI.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Weather API HTTP client
builder.Services.AddHttpClient("OpenMeteo", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Risk prediction: ML integration pending
builder.Services.AddScoped<
    IRiskPredictionService,
    MockRiskPredictionService>();

// Routing: backend integration pending
builder.Services.AddScoped<
    IRouteComparisonService,
    RouteComparisonService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();

public partial class Program { }
