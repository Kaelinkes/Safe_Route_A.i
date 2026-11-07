
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.Interfaces;
using SafeRouteAI.API.Services.BackendContracts;

namespace SafeRouteAI.API.Services;

public sealed class BackendRouteComparisonService
    : IRouteComparisonService
{
    private readonly IBackendRouteClient _backend;

    public BackendRouteComparisonService(
        IBackendRouteClient backend)
    {
        _backend = backend;
    }

    public async Task<RouteComparisonResponse> CompareAsync(
        RouteComparisonRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Origin?.Latitude is not double originLat ||
            request.Origin.Longitude is not double originLon ||
            request.Destination?.Latitude is not double destinationLat ||
            request.Destination.Longitude is not double destinationLon)
        {
            throw new ArgumentException(
                "Valid origin and destination coordinates are required.",
                nameof(request));
        }

        var routes = await _backend.CompareAsync(
            originLat,
            originLon,
            destinationLat,
            destinationLon,
            cancellationToken);

        return BackendRouteResponseMapper.Map(routes);
    }
}
