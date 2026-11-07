
using SafeRouteAI.API.Services.BackendContracts;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Services;

namespace SafeRouteAI.API.Services;

public sealed class RealBackendRouteClient : IBackendRouteClient
{
    private readonly SafeRouteAI.Application.Services.RouteComparisonService
        _backend;

    public RealBackendRouteClient(
        SafeRouteAI.Application.Services.RouteComparisonService backend)
    {
        _backend = backend;
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

        return results.Select(item => new BackendRankedRoute(
            item.Rank,
            new BackendRouteOption(
                item.Route.RouteId,
                item.Route.DistanceKm,
                item.Route.DurationMinutes,
                item.Route.Geometry),
            new BackendRiskAssessment(
                item.Risk.Status.ToString(),
                item.Risk.Score,
                item.Risk.Level?.ToString()),
            item.IsHighRisk
        )).ToList();
    }
}
