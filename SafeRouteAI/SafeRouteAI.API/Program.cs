
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

// Risk prediction remains unavailable until
// the ML model is integrated.
builder.Services.AddScoped<
    IRiskPredictionService,
    MockRiskPredictionService>();

// API-side routing service.

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
