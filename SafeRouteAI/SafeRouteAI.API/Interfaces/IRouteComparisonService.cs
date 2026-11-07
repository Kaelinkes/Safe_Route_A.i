
using SafeRouteAI.API.DTOs;

namespace SafeRouteAI.API.Interfaces;

public interface IRouteComparisonService
{
    Task<RouteComparisonResponse> CompareAsync(
        RouteComparisonRequest request,
        CancellationToken cancellationToken = default);
}
