using System.Globalization;
using System.Text.Json;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Application.Validation;

namespace SafeRouteAI.Infrastructure.Services;

public class WeatherService : IWeatherService
{
    private static readonly WeatherResult Unavailable = new(false, 0, 0, 0, 0, 0, "Unknown");
    private readonly HttpClient _http;

    public WeatherService(HttpClient http) => _http = http;

    public async Task<WeatherResult> GetWeatherAsync(Coordinate location, DateTime time)
    {
        if (!Validators.IsValid(location)) return Unavailable;

        try
        {
            var hour = time.ToUniversalTime().ToString("yyyy-MM-dd'T'HH':00'");
            var url = string.Create(CultureInfo.InvariantCulture,
                $"https://api.open-meteo.com/v1/forecast?latitude={location.Lat}&longitude={location.Lon}&hourly=precipitation,visibility,wind_speed_10m,temperature_2m,relative_humidity_2m,weather_code&start_hour={hour}&end_hour={hour}");

            using var doc = await JsonDocument.ParseAsync(await _http.GetStreamAsync(url));
            var hourly = doc.RootElement.GetProperty("hourly");

            var rain = First(hourly, "precipitation");
            var vis = First(hourly, "visibility");
            var wind = First(hourly, "wind_speed_10m");
            var temp = First(hourly, "temperature_2m");
            var humidity = First(hourly, "relative_humidity_2m");
            var code = First(hourly, "weather_code");

            if (rain is null || vis is null || wind is null || temp is null || humidity is null || code is null)
                return Unavailable;

            return new WeatherResult(true, rain.Value, vis.Value, wind.Value,
                temp.Value, humidity.Value, ToCondition((int)code.Value));
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

    // WMO weather codes used by Open-Meteo
    private static string ToCondition(int code) => code switch
    {
        0 => "Clear",
        1 or 2 => "Partly cloudy",
        3 => "Overcast",
        45 or 48 => "Fog",
        >= 51 and <= 57 => "Drizzle",
        >= 61 and <= 67 => "Rain",
        >= 71 and <= 77 => "Snow",
        >= 80 and <= 82 => "Rain showers",
        85 or 86 => "Snow showers",
        >= 95 and <= 99 => "Thunderstorm",
        _ => "Unknown"
    };
}