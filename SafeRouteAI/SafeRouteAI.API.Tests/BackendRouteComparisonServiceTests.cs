
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.DTOs.Enums;
using SafeRouteAI.API.Services;
using SafeRouteAI.API.Services.BackendContracts;
using Xunit;

namespace SafeRouteAI.API.Tests;

public class BackendRouteComparisonServiceTests
{
    [Fact]
    public async Task CompareAsync_ForwardsCoordinatesAndMapsResult()
    {
        var fakeClient = new FakeBackendRouteClient();
        var service = new BackendRouteComparisonService(fakeClient);

        var request = new RouteComparisonRequest
        {
            Origin = new Coordinates
            {
                Latitude = -26.2041,
                Longitude = 28.0473
            },
            Destination = new Coordinates
            {
                Latitude = -26.1367,
                Longitude = 28.1976
            }
        };

        var response = await service.CompareAsync(request);

        // Verify coordinates are forwarded correctly.
        Assert.Equal(-26.2041, fakeClient.OriginLatitude);
        Assert.Equal(28.0473, fakeClient.OriginLongitude);
        Assert.Equal(-26.1367, fakeClient.DestinationLatitude);
        Assert.Equal(28.1976, fakeClient.DestinationLongitude);

        // Verify real routing and simulated risk flags.
        Assert.False(response.IsRoutingMock);
        Assert.True(response.IsRiskMock);

        // Verify the Backend route is preserved.
        var route = Assert.Single(response.Routes);

        Assert.Equal("route-1", route.RouteId);
        Assert.Equal(12.5, route.DistanceKm);
        Assert.Equal(20.0, route.DurationMinutes);

        // Simulated risk values must not be exposed
        // as verified safety predictions.
        Assert.Equal(
            RiskLevel.InsufficientData,
            route.RiskLevel);

        Assert.Null(route.RiskScore);

        // Preserve the Backend's route geometry.
        Assert.Equal(
            "{\"type\":\"LineString\"}",
            route.Geometry);
    }

    [Fact]
    public async Task CompareAsync_WhenBackendUnavailable_ThrowsUnavailable()
    {
        var service = new BackendRouteComparisonService(
            new UnavailableBackendRouteClient());

        var request = new RouteComparisonRequest
        {
            Origin = new Coordinates
            {
                Latitude = -26.2041,
                Longitude = 28.0473
            },
            Destination = new Coordinates
            {
                Latitude = -26.1367,
                Longitude = 28.1976
            }
        };

        await Assert.ThrowsAsync<RouteServiceUnavailableException>(
            () => service.CompareAsync(request));
    }

    private sealed class FakeBackendRouteClient : IBackendRouteClient
    {
        public double OriginLatitude { get; private set; }
        public double OriginLongitude { get; private set; }
        public double DestinationLatitude { get; private set; }
        public double DestinationLongitude { get; private set; }

        public Task<IReadOnlyList<BackendRankedRoute>> CompareAsync(
            double originLatitude,
            double originLongitude,
            double destinationLatitude,
            double destinationLongitude,
            CancellationToken cancellationToken = default)
        {
            OriginLatitude = originLatitude;
            OriginLongitude = originLongitude;
            DestinationLatitude = destinationLatitude;
            DestinationLongitude = destinationLongitude;

            IReadOnlyList<BackendRankedRoute> routes =
            [
                new BackendRankedRoute(
                    1,
                    new BackendRouteOption(
                        "route-1",
                        12.5,
                        20.0,
                        "{\"type\":\"LineString\"}"),
                    new BackendRiskAssessment(
                        "Valid",
                        0.2,
                        "Low"),
                    false)
            ];

            return Task.FromResult(routes);
        }
    }
}
