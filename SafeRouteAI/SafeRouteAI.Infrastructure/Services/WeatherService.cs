using System.Globalization;
using System.Text.Json;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Application.Validation;

namespace SafeRouteAI.Infrastructure.Services;

public class WeatherService : IWeatherService
{
    private static readonly WeatherResult Unavailable = new(false, 0, 0, 0);
    private readonly HttpClient _http;

    public WeatherService(HttpClient http) => _http = http;

    public async Task<WeatherResult> GetWeatherAsync(Coordinate location, DateTime time)
    {
        if (!Validators.IsValid(location)) return Unavailable;

        try
        {
            var hour = time.ToUniversalTime().ToString("yyyy-MM-dd'T'HH':00'");
            var url = string.Create(CultureInfo.InvariantCulture,
                $"https://api.open-meteo.com/v1/forecast?latitude={location.Lat}&longitude={location.Lon}&hourly=precipitation,visibility,wind_speed_10m&start_hour={hour}&end_hour={hour}");

            using var doc = await JsonDocument.ParseAsync(await _http.GetStreamAsync(url));
            var hourly = doc.RootElement.GetProperty("hourly");

            var rain = First(hourly, "precipitation");
            var vis = First(hourly, "visibility");
            var wind = First(hourly, "wind_speed_10m");

            return rain is null || vis is null || wind is null
                ? Unavailable
                : new WeatherResult(true, rain.Value, vis.Value, wind.Value);
        }
        catch
        {
            return Unavailable;
        }
    }

    private static double? First(JsonElement hourly, string name)
    {
        var value = hourly.GetProperty(name)[0];
        return value.ValueKind == JsonValueKind.Number ? value.GetDouble() : null;
    }
}