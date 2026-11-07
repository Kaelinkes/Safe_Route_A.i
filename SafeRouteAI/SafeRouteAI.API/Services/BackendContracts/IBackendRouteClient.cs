
namespace SafeRouteAI.API.Services.BackendContracts;

public interface IBackendRouteClient
{
    Task<IReadOnlyList<BackendRankedRoute>> CompareAsync(
        double originLatitude,
        double originLongitude,
        double destinationLatitude,
        double destinationLongitude,
        CancellationToken cancellationToken = default);
}
