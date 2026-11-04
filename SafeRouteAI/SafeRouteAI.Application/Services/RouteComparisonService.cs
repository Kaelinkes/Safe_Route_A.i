using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Domain.Enums;

namespace SafeRouteAI.Application.Services;

public class RouteComparisonService
{
    private readonly IRouteService _routes;
    private readonly IWeatherService _weather;
    private readonly IRiskService _risk;

    public RouteComparisonService(IRouteService routes, IWeatherService weather, IRiskService risk)
    {
        _routes = routes;
        _weather = weather;
        _risk = risk;
    }

    public async Task<List<RankedRouteDto>> CompareAsync(
        Coordinate start, Coordinate destination, CancellationToken ct = default)
    {
        var routes = await _routes.GetRoutesAsync(start, destination, ct);
        if (routes.Count == 0) return new();

        // One weather lookup at the start point, for all routes
        var weather = await _weather.GetWeatherAsync(start, DateTime.UtcNow);

        var items = new List<(RouteOptionDto Route, RiskAssessmentDto Risk)>();
        foreach (var route in routes)
            items.Add((route, await _risk.AssessAsync(route, weather, ct)));

        return Rank(items, weather);
    }

    public static List<RankedRouteDto> Rank(
        List<(RouteOptionDto Route, RiskAssessmentDto Risk)> items, WeatherResult weather)
    {
        var maxTime = Math.Max(items.Max(i => i.Route.DurationMinutes), 1);
        var adverse = IsAdverse(weather);

        return items
            .OrderBy(i => i.Risk.Score is null ? 1 : 0)   // InsufficientData routes go last
            .ThenBy(i => i.Risk.Score is { } s
                ? 0.5 * s + 0.5 * (i.Route.DurationMinutes / maxTime)
                : i.Route.DurationMinutes)
            .Select((i, index) => new RankedRouteDto(index + 1, i.Route, i.Risk, IsHighRisk(i.Risk, adverse)))
            .ToList();
    }

    // FR9: elevated risk together with poor weather
    private static bool IsHighRisk(RiskAssessmentDto risk, bool adverse) =>
        adverse && risk.Level is RiskLevel.Medium or RiskLevel.High;

    private static bool IsAdverse(WeatherResult w) =>
        w.IsAvailable && (w.Precipitation > 2 || w.Visibility < 1000 || w.WindSpeed > 40);
}