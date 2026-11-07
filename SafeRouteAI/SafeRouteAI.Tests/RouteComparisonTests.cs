using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Services;
using SafeRouteAI.Domain.Enums;

public class RouteComparisonTests
{
    private static readonly WeatherResult Clear = new(true, 0, 10000, 5, 22, 40, "Clear");
    private static readonly WeatherResult Storm = new(true, 8, 400, 50, 15, 90, "Thunderstorm");

    private static (RouteOptionDto, RiskAssessmentDto) Item(string id, double minutes, double? score, RiskLevel? level) =>
        (new RouteOptionDto(id, 10, minutes, "{}"),
         new RiskAssessmentDto(score is null ? AssessmentStatus.InsufficientData : AssessmentStatus.Valid,
                               score, level, new()));

    [Fact]
    public void Safer_route_ranks_first_even_if_slower()
    {
        var ranked = RouteComparisonService.Rank(new()
        {
            Item("fast-risky", 10, 0.8, RiskLevel.High),
            Item("slow-safe", 20, 0.1, RiskLevel.Low)
        }, Clear);

        Assert.Equal("slow-safe", ranked[0].Route.RouteId);
        Assert.Equal(1, ranked[0].Rank);
    }

    [Fact]
    public void Insufficient_data_routes_go_last()
    {
        var ranked = RouteComparisonService.Rank(new()
        {
            Item("unknown", 5, null, null),
            Item("known", 30, 0.9, RiskLevel.High)
        }, Clear);

        Assert.Equal("known", ranked[0].Route.RouteId);
        Assert.Equal("unknown", ranked[1].Route.RouteId);
    }

    [Fact]
    public void High_risk_flag_needs_bad_weather()
    {
        var items = new List<(RouteOptionDto, RiskAssessmentDto)> { Item("a", 10, 0.8, RiskLevel.High) };

        Assert.False(RouteComparisonService.Rank(items, Clear)[0].IsHighRisk);
        Assert.True(RouteComparisonService.Rank(items, Storm)[0].IsHighRisk);
    }
}