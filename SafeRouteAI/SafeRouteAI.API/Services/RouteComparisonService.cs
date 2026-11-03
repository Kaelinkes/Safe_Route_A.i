
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.Interfaces;

namespace SafeRouteAI.API.Services;

public class RouteComparisonService : IRouteComparisonService
{
    public Task<RouteComparisonResponse> CompareAsync(
        RouteComparisonRequest request,
        CancellationToken cancellationToken = default)
    {
        // The backend routing service is not yet available
        // on this branch. Do not invent route results.

        return Task.FromResult(new RouteComparisonResponse
        {
            IsMock = false,
            Routes = new List<RouteOptionResponse>()
        });
    }
}
