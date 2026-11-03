
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.Interfaces;

namespace SafeRouteAI.API.Services;

public class RouteComparisonService : IRouteComparisonService
{
    public Task<RouteComparisonResponse> CompareAsync(
        RouteComparisonRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new RouteServiceUnavailableException(
            "Route calculation is not available yet. " +
            "The backend routing service has not been connected.");
    }
}

public sealed class RouteServiceUnavailableException : Exception
{
    public RouteServiceUnavailableException(string message)
        : base(message)
    {
    }
}
