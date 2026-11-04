
using SafeRouteAI.API.DTOs.Enums;
using SafeRouteAI.API.Services;
using SafeRouteAI.API.Services.BackendContracts;
using Xunit;

namespace SafeRouteAI.API.Tests;

public class BackendRouteResponseMapperTests
{
    [Theory]
    [InlineData("Low", RiskLevel.Low)]
    [InlineData("Medium", RiskLevel.Medium)]
    [InlineData("High", RiskLevel.High)]
    public void Map_ValidRisk_MapsLevelCorrectly(
        string level, RiskLevel expected)
    {
        var route = CreateRoute(
            1, "Valid", 0.5, level);

        var result = BackendRouteResponseMapper.Map(
            new[] { route });

        Assert.Equal(
            expected,
            result.Routes[0].RiskLevel);

        Assert.Equal(
            0.5,
            result.Routes[0].RiskScore);
    }

    [Fact]
    public void Map_InsufficientData_ReturnsNullScore()
    {
        var route = CreateRoute(
            1, "InsufficientData", null, null);

        var result = BackendRouteResponseMapper.Map(
            new[] { route });

        Assert.Equal(
            RiskLevel.InsufficientData,
            result.Routes[0].RiskLevel);

        Assert.Null(result.Routes[0].RiskScore);
    }

    [Fact]
    public void Map_PreservesRouteRanking()
    {
        var second = CreateRoute(
            2, "Valid", 0.4, "Medium");

        var first = CreateRoute(
            1, "Valid", 0.2, "Low");

        var result = BackendRouteResponseMapper.Map(
            new[] { second, first });

        Assert.Equal(
            "route-1",
            result.Routes[0].RouteId);

        Assert.Equal(
            "route-2",
            result.Routes[1].RouteId);
    }

    [Fact]
    public void Map_PreservesRouteDetails()
    {
        var route = CreateRoute(
            1, "Valid", 0.2, "Low");

        var result = BackendRouteResponseMapper.Map(
            new[] { route });

        var mapped = Assert.Single(result.Routes);

        Assert.Equal(12.5, mapped.DistanceKm);
        Assert.Equal(20.0, mapped.DurationMinutes);
        Assert.Equal(
            "{\"type\":\"LineString\"}",
            mapped.Geometry);
    }

    [Fact]
    public void Map_EmptyRoutes_ReturnsEmptyList()
    {
        var result = BackendRouteResponseMapper.Map(
            Array.Empty<BackendRankedRoute>());

        Assert.Empty(result.Routes);
    }

    private static BackendRankedRoute CreateRoute(
        int rank,
        string status,
        double? score,
        string? level)
    {
        return new BackendRankedRoute(
            rank,
            new BackendRouteOption(
                $"route-{rank}",
                12.5,
                20.0,
                "{\"type\":\"LineString\"}"),
            new BackendRiskAssessment(
                status, score, level),
            false);
    }
}
