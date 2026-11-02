
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

// Standardized API error handling
builder.Services.AddProblemDetails();

// Register API services
builder.Services.AddScoped<
    IRiskPredictionService,
    MockRiskPredictionService>();

builder.Services.AddScoped<
    IRouteComparisonService,
    MockRouteComparisonService>();

var app = builder.Build();

// Handle unexpected exceptions
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();

public partial class Program { }
