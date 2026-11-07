using System.Net;
using Microsoft.Extensions.Configuration;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Exceptions;
using SafeRouteAI.Application.Services;
using SafeRouteAI.Domain.Enums;
using SafeRouteAI.Infrastructure.Services;

namespace SafeRouteAI.Tests;

public sealed class BackendServicesTests
{
    private const string RouteResponse = """
        {"features":[{"properties":{"summary":{"distance":12400.0,"duration":1080.0}},
        "geometry":{"type":"LineString","coordinates":[[28.04,-26.2],[28.05,-26.21]]}}]}
        """;

    [Fact]
    public async Task RouteService_converts_units_and_preserves_geometry()
    {
        var routes = await CreateRouteService(new SequenceHandler(
            (HttpStatusCode.OK, RouteResponse)))
            .GetRoutesAsync(new Coordinate(-26.2, 28.04), new Coordinate(-26.21, 28.05));

        var route = Assert.Single(routes);
        Assert.Equal("route-1", route.RouteId);
        Assert.Equal(12.4, route.DistanceKm);
        Assert.Equal(18, route.DurationMinutes);
        Assert.Contains("LineString", route.Geometry);
    }

    [Fact]
    public async Task RouteService_retries_only_when_alternative_routes_are_rejected()
    {
        var handler = new SequenceHandler(
            (HttpStatusCode.BadRequest, "alternative routes exceed the distance limit"),
            (HttpStatusCode.OK, RouteResponse));

        var routes = await CreateRouteService(handler)
            .GetRoutesAsync(new Coordinate(-26.2, 28.04), new Coordinate(-26.21, 28.05));

        Assert.Single(routes);
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task RouteService_translates_no_route_errors()
    {
        var handler = new SequenceHandler(
            (HttpStatusCode.NotFound, """{"error":{"code":2010,"message":"No routable point"}}"""));

        await Assert.ThrowsAsync<RouteNotFoundException>(() =>
            CreateRouteService(handler)
                .GetRoutesAsync(new Coordinate(-26.2, 28.04), new Coordinate(-26.21, 28.05)));

        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task RouteService_rejects_invalid_coordinates()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateRouteService(new SequenceHandler((HttpStatusCode.OK, RouteResponse)))
                .GetRoutesAsync(new Coordinate(999, 0), new Coordinate(0, 0)));
    }

    [Fact]
    public async Task WeatherService_returns_unavailable_for_invalid_coordinates_without_http()
    {
        var handler = new SequenceHandler((HttpStatusCode.OK, "{}"));

        var weather = await new WeatherService(new HttpClient(handler))
            .GetWeatherAsync(new Coordinate(999, 999), DateTime.UtcNow);

        Assert.False(weather.IsAvailable);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task FakeRiskService_returns_insufficient_data_without_weather()
    {
        var risk = await new FakeRiskService().AssessAsync(
            new RouteOptionDto("route-1", 10, 15, "{}"),
            new WeatherResult(false, 0, 0, 0, 0, 0, "Unknown"));

        Assert.Equal(AssessmentStatus.InsufficientData, risk.Status);
        Assert.Null(risk.Score);
        Assert.Null(risk.Level);
    }

    [Fact]
    public void RouteComparison_ranks_safer_route_first_and_marks_weather_risk()
    {
        var clear = new WeatherResult(true, 0, 10_000, 5, 22, 40, "Clear");
        var storm = new WeatherResult(true, 8, 400, 50, 15, 90, "Thunderstorm");
        var safer = new RouteOptionDto("safer", 10, 20, "{}");
        var riskier = new RouteOptionDto("riskier", 10, 10, "{}");
        var items = new List<(RouteOptionDto Route, RiskAssessmentDto Risk)>
        {
            (riskier, new(AssessmentStatus.Valid, 0.8, RiskLevel.High, [])),
            (safer, new(AssessmentStatus.Valid, 0.1, RiskLevel.Low, []))
        };

        var clearRanking = RouteComparisonService.Rank(items, clear);
        var stormRanking = RouteComparisonService.Rank(items, storm);

        Assert.Equal("safer", clearRanking[0].Route.RouteId);
        Assert.False(clearRanking.Single(item => item.Route.RouteId == "riskier").IsHighRisk);
        Assert.True(stormRanking.Single(item => item.Route.RouteId == "riskier").IsHighRisk);
    }

    private static RouteService CreateRouteService(HttpMessageHandler handler)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenRouteService:ApiKey"] = "test"
            })
            .Build();
        return new RouteService(new HttpClient(handler), configuration);
    }

    private sealed class SequenceHandler(params (HttpStatusCode Status, string Body)[] responses)
        : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var (status, body) = responses[Math.Min(Calls++, responses.Length - 1)];
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body)
            });
        }
    }
}
