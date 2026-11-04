
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
            // Routing will be real, but the current
            // backend risk calculation is simulated.
            IsMock = true,

            Routes = routes
                .OrderBy(r => r.Rank)
                .Select(r => new RouteOptionResponse
                {
                    RouteId = r.Route.RouteId,
                    DistanceKm = r.Route.DistanceKm,
                    DurationMinutes = r.Route.DurationMinutes,
                    Geometry = r.Route.Geometry,
                    RiskLevel = MapRiskLevel(r.Risk),
                    RiskScore = IsValidAssessment(r.Risk)
                        ? r.Risk.Score
                        : null
                })
                .ToList()
        };
    }

    private static bool IsValidAssessment(
        BackendRiskAssessment risk)
    {
        return string.Equals(
            risk.Status,
            "Valid",
            StringComparison.OrdinalIgnoreCase);
    }

    private static RiskLevel MapRiskLevel(
        BackendRiskAssessment risk)
    {
        if (!IsValidAssessment(risk))
        {
            return RiskLevel.InsufficientData;
        }

        return risk.Level?.Trim().ToLowerInvariant() switch
        {
            "low" => RiskLevel.Low,
            "medium" => RiskLevel.Medium,
            "high" => RiskLevel.High,
            _ => RiskLevel.InsufficientData
        };
    }
}
