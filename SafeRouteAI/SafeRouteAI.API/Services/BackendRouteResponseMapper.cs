
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

        var mappedRoutes = routes
            .OrderBy(r => r.Rank)
            .Select(r =>
            {
                var hasRealPrediction =
                    r.Risk is not null &&
                    r.Risk.PredictionSource == "PythonLightGBM" &&
                    r.Risk.Status == "Valid" &&
                    r.Risk.Score is double score &&
                    double.IsFinite(score) &&
                    score >= 0 &&
                    score <= 1 &&
                    Enum.TryParse<RiskLevel>(
                        r.Risk.Level,
                        true,
                        out var parsedLevel) &&
                    parsedLevel != RiskLevel.InsufficientData;

                var riskLevel = RiskLevel.InsufficientData;
                double? riskScore = null;

                if (hasRealPrediction)
                {
                    Enum.TryParse<RiskLevel>(
                        r.Risk.Level,
                        true,
                        out riskLevel);

                    riskScore = r.Risk.Score;
                }

                return new RouteOptionResponse
                {
                    RouteId = r.Route.RouteId,
                    DistanceKm = r.Route.DistanceKm,
                    DurationMinutes = r.Route.DurationMinutes,
                    Geometry = r.Route.Geometry,
                    RoadSegments = r.Route.RoadSegments ?? new(),

                    RiskLevel = riskLevel,
                    RiskScore = riskScore
                };
            })
            .ToList();

        return new RouteComparisonResponse
        {
            IsRoutingMock = false,

            IsRiskMock = mappedRoutes.Count == 0 ||
                         mappedRoutes.Any(r =>
                             r.RiskScore is null),

            Routes = mappedRoutes
        };
    }
}
