using System.Text.Json.Serialization;
using SafeRouteAI.API.Interfaces;
using SafeRouteAI.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Register API controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

// Register OpenAPI documentation
builder.Services.AddOpenApi();

// Register mock services
builder.Services.AddScoped<
    IRiskPredictionService,
    MockRiskPredictionService>();

builder.Services.AddScoped<
    IRouteComparisonService,
    MockRouteComparisonService>();

var app = builder.Build();

// OpenAPI documentation
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Register API endpoints
app.MapControllers();

app.Run();
public partial class Program { }