using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Domain.Enums;

namespace SafeRouteAI.Application.Services;

public class RouteComparisonService(
    IRouteService routes,
    IWeatherService weather,
    IRiskService risk)
{
    public async Task<List<RankedRouteDto>> CompareAsync(
        Coordinate start,
        Coordinate destination,
        CancellationToken cancellationToken = default)
    {
        var routeOptions = await routes.GetRoutesAsync(start, destination, cancellationToken);
        if (routeOptions.Count == 0)
        {
            return [];
        }

        var weatherResult = await weather.GetWeatherAsync(start, DateTime.UtcNow);
        var assessments = new List<(RouteOptionDto Route, RiskAssessmentDto Risk)>();
        foreach (var route in routeOptions)
        {
            assessments.Add((
                route,
                await risk.AssessAsync(route, weatherResult, cancellationToken)));
        }

        return Rank(assessments, weatherResult);
    }

    public static List<RankedRouteDto> Rank(
        List<(RouteOptionDto Route, RiskAssessmentDto Risk)> items,
        WeatherResult weather)
    {
        if (items.Count == 0)
        {
            return [];
        }

        var maxTime = Math.Max(items.Max(item => item.Route.DurationMinutes), 1);
        var adverseWeather = IsAdverse(weather);

        return items
            .OrderBy(item => item.Risk.Score is null ? 1 : 0)
            .ThenBy(item => item.Risk.Score is { } score
                ? 0.5 * score + 0.5 * (item.Route.DurationMinutes / maxTime)
                : item.Route.DurationMinutes)
            .Select((item, index) => new RankedRouteDto(
                index + 1,
                item.Route,
                item.Risk,
                IsHighRisk(item.Risk, adverseWeather)))
            .ToList();
    }

    private static bool IsHighRisk(RiskAssessmentDto risk, bool adverseWeather) =>
        adverseWeather && risk.Level is RiskLevel.Medium or RiskLevel.High;

    private static bool IsAdverse(WeatherResult weather) =>
        weather.IsAvailable &&
        (weather.Precipitation > 2 || weather.Visibility < 1000 || weather.WindSpeed > 40);
}
