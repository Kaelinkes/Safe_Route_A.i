
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.DTOs.Enums;
using SafeRouteAI.API.Services.BackendContracts;

namespace SafeRouteAI.API.Services;

public static class BackendRouteResponseMapper
{
    public static RouteComparisonResponse Map(
        IReadOnlyList<BackendRankedRoute> routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        return new RouteComparisonResponse
        {
            IsRoutingMock = false,
            IsRiskMock = true,

            Routes = routes
                .OrderBy(r => r.Rank)
                .Select(r => new RouteOptionResponse
                {
                    RouteId = r.Route.RouteId,
                    DistanceKm = r.Route.DistanceKm,
                    DurationMinutes = r.Route.DurationMinutes,
                    Geometry = r.Route.Geometry,
                    RoadSegments = r.Route.RoadSegments ?? new(),

                    // Do not expose simulated risk predictions.
                    RiskLevel = RiskLevel.InsufficientData,
                    RiskScore = null
                })
                .ToList()
        };
    }
}
