
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.Interfaces;

namespace SafeRouteAI.API.Services;

/// <summary>
/// Integration point for the backend routing service.
/// The actual IRouteService implementation is maintained
/// by the backend teammate and is not yet available
/// on this feature branch.
/// </summary>
public sealed class BackendRouteComparisonService
    : IRouteComparisonService
{
    public Task<RouteComparisonResponse> CompareAsync(
        RouteComparisonRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        throw new RouteServiceUnavailableException(
            "Backend routing is not connected yet.");
    }
}
