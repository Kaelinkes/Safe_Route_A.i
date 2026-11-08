
using SafeRouteAI.API.Services.BackendContracts;
using SafeRouteAI.Application.DTOs;

namespace SafeRouteAI.API.Services;

public sealed class RealBackendRouteClient : IBackendRouteClient
{
    private readonly SafeRouteAI.Application.Services.RouteComparisonService
        _backend;

    private readonly PythonRoutePredictionClient _python;

    public RealBackendRouteClient(
        SafeRouteAI.Application.Services.RouteComparisonService backend,
        PythonRoutePredictionClient python)
    {
        _backend = backend;
        _python = python;
    }

    public async Task<IReadOnlyList<BackendRankedRoute>> CompareAsync(
        double originLatitude,
        double originLongitude,
        double destinationLatitude,
        double destinationLongitude,
        CancellationToken cancellationToken = default)
    {
        var start = new Coordinate(
            originLatitude,
            originLongitude);

        var destination = new Coordinate(
            destinationLatitude,
            destinationLongitude);

        var results = await _backend.CompareAsync(
            start,
            destination,
            cancellationToken);

        var mappedRoutes = new List<BackendRankedRoute>();

        foreach (var item in results)
        {
            // Send this route's geometry to the Python service.
            var pythonResult = await _python.PredictRouteAsync(
                item.Route.Geometry,
                cancellationToken);

            var risk = CreateRiskAssessment(pythonResult);

            mappedRoutes.Add(new BackendRankedRoute(
                item.Rank,
                new BackendRouteOption(
                    item.Route.RouteId,
                    item.Route.DistanceKm,
                    item.Route.DurationMinutes,
                    item.Route.Geometry,
                    item.Route.RoadSegments.Select(segment =>
                        new BackendRoadSegmentAttribute(
                            segment.FromIndex,
                            segment.ToIndex,
                            segment.Attribute,
                            segment.Value
                        )).ToList()),
                risk,
                false));
        }

        return mappedRoutes;
    }

    private static BackendRiskAssessment CreateRiskAssessment(
        PythonRoutePredictionResponse response)
    {
        if (response.Results.Count == 0)
        {
            return new BackendRiskAssessment(
                "InsufficientData", null, null);
        }

        if (response.Results.Any(r => r.Status == "InvalidInput"))
        {
            return new BackendRiskAssessment(
                "InvalidInput", null, null);
        }

        // Every route point must have a valid prediction.
        if (response.Results.Any(r =>
                r.Status != "Valid" || r.RiskScore is null))
        {
            return new BackendRiskAssessment(
                "InsufficientData", null, null);
        }

        var score = response.Results.Average(
            r => r.RiskScore!.Value);

        var levels = response.Results
            .Select(r => r.RiskLevel)
            .ToList();

        var level = levels.Contains("High")
            ? "High"
            : levels.Contains("Medium")
                ? "Medium"
                : "Low";

        return new BackendRiskAssessment(
            "Valid",
            Math.Round(score, 4),
            level);
    }
}
