
using System.Text.Json.Serialization;
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

// Weather integration
builder.Services.AddHttpClient("OpenMeteo", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// ML integration
builder.Services.AddMlIntegration(builder.Configuration);

// Routing integration
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
