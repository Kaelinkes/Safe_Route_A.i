using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Infrastructure.Services;

public class WeatherServiceTests
{
    [Fact]
    public async Task Returns_weather_for_Johannesburg()
    {
        var result = await new WeatherService(new HttpClient())
            .GetWeatherAsync(new Coordinate(-26.2041, 28.0473), DateTime.UtcNow);

        Assert.True(result.IsAvailable);
    }

    [Fact]
    public async Task Bad_coordinates_are_unavailable()
    {
        var result = await new WeatherService(new HttpClient())
            .GetWeatherAsync(new Coordinate(999, 999), DateTime.UtcNow);

        Assert.False(result.IsAvailable);
    }
}