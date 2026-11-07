
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace SafeRouteAI.API.Controllers;

[ApiController]
[Route("api/v1/weather")]
public class WeatherController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(
        IHttpClientFactory httpClientFactory,
        ILogger<WeatherController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetWeather(
        [FromQuery] double? latitude,
        [FromQuery] double? longitude,
        CancellationToken cancellationToken)
    {
        if (latitude is null || longitude is null ||
            !double.IsFinite(latitude.Value) ||
            !double.IsFinite(longitude.Value) ||
            latitude < -90 || latitude > 90 ||
            longitude < -180 || longitude > 180)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid coordinates",
                Detail = "Valid latitude and longitude are required.",
                Status = 400
            });
        }

        var url = string.Create(
            CultureInfo.InvariantCulture,
            $"https://api.open-meteo.com/v1/forecast?latitude={latitude.Value}&longitude={longitude.Value}&current=temperature_2m,relative_humidity_2m,precipitation,visibility,wind_speed_10m,weather_code");

        try
        {
            var client = _httpClientFactory.CreateClient("OpenMeteo");

            using var response = await client.GetAsync(
                url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Open-Meteo returned HTTP {StatusCode}",
                    (int)response.StatusCode);

                return StatusCode(503, new ProblemDetails
                {
                    Title = "Weather service unavailable",
                    Status = 503
                });
            }

            using var stream = await response.Content.ReadAsStreamAsync(
                cancellationToken);

            using var json = await JsonDocument.ParseAsync(
                stream,
                cancellationToken: cancellationToken);

            var current = json.RootElement.GetProperty("current");

            return Ok(new
            {
                latitude,
                longitude,
                observationTimeUtc =
                    current.GetProperty("time").GetString(),
                temperatureCelsius =
                    current.GetProperty("temperature_2m").GetDouble(),
                humidityPercent =
                    current.GetProperty("relative_humidity_2m").GetDouble(),
                precipitationMm =
                    current.GetProperty("precipitation").GetDouble(),
                visibilityMeters =
                    current.GetProperty("visibility").GetDouble(),
                windSpeedKmh =
                    current.GetProperty("wind_speed_10m").GetDouble(),
                weatherCode =
                    current.GetProperty("weather_code").GetInt32(),
                isAvailable = true
            });
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve weather data from Open-Meteo");

            return StatusCode(503, new ProblemDetails
            {
                Title = "Weather service unavailable",
                Status = 503
            });
        }
    }
}
