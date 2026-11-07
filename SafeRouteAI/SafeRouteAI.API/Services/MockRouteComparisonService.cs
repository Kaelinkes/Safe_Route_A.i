
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.DTOs.Enums;
using SafeRouteAI.API.Interfaces;

namespace SafeRouteAI.API.Services;

public class MockRouteComparisonService : IRouteComparisonService
{
    public Task<RouteComparisonResponse> CompareAsync(
        RouteComparisonRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = new RouteComparisonResponse
        {
            IsRoutingMock = true,
            IsRiskMock = true,
            Routes = new List<RouteOptionResponse>
            {
                new()
                {
                    RouteId = "mock-route-001",
                    DistanceKm = 12.5,
                    DurationMinutes = 20,
                    RiskLevel = RiskLevel.InsufficientData,
                    RiskScore = null,
                    Geometry = null
                },
                new()
                {
                    RouteId = "mock-route-002",
                    DistanceKm = 15.2,
                    DurationMinutes = 25,
                    RiskLevel = RiskLevel.InsufficientData,
                    RiskScore = null,
                    Geometry = null
                }
            }
        };

        return Task.FromResult(response);
    }
}
