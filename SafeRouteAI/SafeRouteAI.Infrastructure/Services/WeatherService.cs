using System.Globalization;
using System.Text.Json;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Application.Validation;

namespace SafeRouteAI.Infrastructure.Services;

public class WeatherService(HttpClient http) : IWeatherService
{
    private static readonly WeatherResult Unavailable =
        new(false, 0, 0, 0, 0, 0, "Unknown");

    public async Task<WeatherResult> GetWeatherAsync(
        Coordinate location,
        DateTime time)
    {
        if (!Validators.IsValid(location))
            return Unavailable;

        var hour = time.ToUniversalTime()
            .ToString("yyyy-MM-dd'T'HH':00'", CultureInfo.InvariantCulture);

        var url = string.Create(
            CultureInfo.InvariantCulture,
            $"https://api.open-meteo.com/v1/forecast?latitude={location.Lat}&longitude={location.Lon}&hourly=precipitation,visibility,wind_speed_10m,temperature_2m,relative_humidity_2m,weather_code&start_hour={hour}&end_hour={hour}");

        try
        {
            using var document =
                await JsonDocument.ParseAsync(await http.GetStreamAsync(url));

            var hourly = document.RootElement.GetProperty("hourly");

            var precipitation = First(hourly, "precipitation");
            var visibility = First(hourly, "visibility");
            var windSpeed = First(hourly, "wind_speed_10m");
            var temperature = First(hourly, "temperature_2m");
            var humidity = First(hourly, "relative_humidity_2m");
            var weatherCode = First(hourly, "weather_code");

            if (precipitation is null ||
                visibility is null ||
                windSpeed is null ||
                temperature is null ||
                humidity is null ||
                weatherCode is null)
            {
                return Unavailable;
            }

            return new WeatherResult(
                true,
                precipitation.Value,
                visibility.Value,
                windSpeed.Value,
                temperature.Value,
                humidity.Value,
                ToCondition((int)weatherCode.Value));
        }
        catch (HttpRequestException)
        {
            return Unavailable;
        }
        catch (JsonException)
        {
            return Unavailable;
        }
        catch (KeyNotFoundException)
        {
            return Unavailable;
        }
        catch (IndexOutOfRangeException)
        {
            return Unavailable;
        }
    }

    private static double? First(JsonElement hourly, string name)
    {
        var values = hourly.GetProperty(name);

        if (values.GetArrayLength() == 0)
            return null;

        var value = values[0];

        return value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : null;
    }

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