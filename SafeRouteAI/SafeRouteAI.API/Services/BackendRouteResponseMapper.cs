
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
            // The Backend calculates actual routes.
            IsRoutingMock = false,

            // Risk predictions are not yet produced
            // by the verified Python ML model.
            IsRiskMock = true,

            Routes = routes
                .OrderBy(r => r.Rank)
                .Select(r => new RouteOptionResponse
                {
                    RouteId = r.Route.RouteId,

                    DistanceKm = r.Route.DistanceKm,

                    DurationMinutes = r.Route.DurationMinutes,

                    // Preserve the Backend's GeoJSON
                    // LineString geometry as a string.
                    Geometry = r.Route.Geometry,

                    // Do not expose simulated risk
                    // classifications as verified.
                    RiskLevel = RiskLevel.InsufficientData,

                    RiskScore = null
                })
                .ToList()
        };
    }
}
