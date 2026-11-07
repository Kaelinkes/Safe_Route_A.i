
using SafeRouteAI.API.Services.BackendContracts;

namespace SafeRouteAI.API.Services;

public sealed class UnavailableBackendRouteClient
    : IBackendRouteClient
{
    public Task<IReadOnlyList<BackendRankedRoute>> CompareAsync(
        double originLatitude,
        double originLongitude,
        double destinationLatitude,
        double destinationLongitude,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        throw new RouteServiceUnavailableException(
            "Backend routing dependencies are not connected yet.");
    }
}
