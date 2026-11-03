using System.Net;
using Microsoft.Extensions.Configuration;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Infrastructure.Services;

public class RouteServiceTests
{
    private const string Json = """
    {"features":[{"properties":{"summary":{"distance":12400.0,"duration":1080.0}},
    "geometry":{"type":"LineString","coordinates":[[28.04,-26.2],[28.05,-26.21]]}}]}
    """;

    private class StubHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken c) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Json) });
    }

    private static RouteService Create() => new(
        new HttpClient(new StubHandler()),
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["OpenRouteService:ApiKey"] = "test" })
            .Build());

    [Fact]
    public async Task Converts_units_and_keeps_geometry()
    {
        var routes = await Create().GetRoutesAsync(new Coordinate(-26.2, 28.04), new Coordinate(-26.21, 28.05));

        var r = Assert.Single(routes);
        Assert.Equal("route-1", r.RouteId);
        Assert.Equal(12.4, r.DistanceKm);
        Assert.Equal(18, r.DurationMinutes);
        Assert.Contains("LineString", r.Geometry);
    }

    [Fact]
    public async Task Invalid_coordinates_throw()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            Create().GetRoutesAsync(new Coordinate(999, 0), new Coordinate(0, 0)));
    }
}