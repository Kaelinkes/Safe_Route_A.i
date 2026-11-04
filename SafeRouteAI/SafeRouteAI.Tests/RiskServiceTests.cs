using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Domain.Enums;
using SafeRouteAI.Infrastructure.Services;

public class RiskServiceTests
{
    private static readonly RouteOptionDto Route = new("route-1", 10, 15, "{}");

    [Fact]
    public async Task Missing_weather_gives_InsufficientData()
    {
        var r = await new FakeRiskService().AssessAsync(Route, new(false, 0, 0, 0, 0, 0, "Unknown"));

        Assert.Equal(AssessmentStatus.InsufficientData, r.Status);
        Assert.Null(r.Score);
        Assert.Null(r.Level);
    }

    [Fact]
    public async Task Heavy_rain_and_fog_gives_High_risk()
    {
        var r = await new FakeRiskService().AssessAsync(Route, new(true, 8, 400, 50, 15, 90, "Fog"));

        Assert.Equal(RiskLevel.High, r.Level);
    }
}