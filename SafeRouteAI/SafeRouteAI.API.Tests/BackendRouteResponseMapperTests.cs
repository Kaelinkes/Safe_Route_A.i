
using SafeRouteAI.API.DTOs.Enums;
using SafeRouteAI.API.Services;
using SafeRouteAI.API.Services.BackendContracts;
using Xunit;

namespace SafeRouteAI.API.Tests;

public class BackendRouteResponseMapperTests
{
    [Theory]
    [InlineData("Low")]
    [InlineData("Medium")]
    [InlineData("High")]
    public void Map_SimulatedRisk_ReturnsInsufficientData(
        string level)
    {
        var route = CreateRoute(
            1, "Valid", 0.5, level);

        var result = BackendRouteResponseMapper.Map(
            new[] { route });

        var mapped = Assert.Single(result.Routes);

        // Routing is real, but risk is simulated.
        Assert.False(result.IsRoutingMock);
        Assert.True(result.IsRiskMock);

        // Simulated risk must not appear as
        // a verified ML prediction.
        Assert.Equal(
            RiskLevel.InsufficientData,
            mapped.RiskLevel);

        Assert.Null(mapped.RiskScore);
    }

    [Fact]
    public void Map_InsufficientData_ReturnsNullScore()
    {
        var route = CreateRoute(
            1, "InsufficientData", null, null);

        var result = BackendRouteResponseMapper.Map(
            new[] { route });

        var mapped = Assert.Single(result.Routes);

        Assert.Equal(
            RiskLevel.InsufficientData,
            mapped.RiskLevel);

        Assert.Null(mapped.RiskScore);

        Assert.False(result.IsRoutingMock);
        Assert.True(result.IsRiskMock);
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

        Assert.Equal(2, result.Routes.Count);

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

        Assert.Equal("route-1", mapped.RouteId);
        Assert.Equal(12.5, mapped.DistanceKm);
        Assert.Equal(20.0, mapped.DurationMinutes);

        // Keep the Backend geometry unchanged.
        Assert.Equal(
            "{\"type\":\"LineString\"}",
            mapped.Geometry);

        Assert.Equal(
            RiskLevel.InsufficientData,
            mapped.RiskLevel);

        Assert.Null(mapped.RiskScore);
    }

    [Fact]
    public void Map_EmptyRoutes_ReturnsEmptyList()
    {
        var result = BackendRouteResponseMapper.Map(
            Array.Empty<BackendRankedRoute>());

        Assert.Empty(result.Routes);

        Assert.False(result.IsRoutingMock);
        Assert.True(result.IsRiskMock);
    }

    [Fact]
    public void Map_NullRoutes_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => BackendRouteResponseMapper.Map(null!));
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
