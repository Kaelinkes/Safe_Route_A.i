
using System.Net;
using System.Text;
using System.Text.Json;
using SafeRouteAI.API.Services;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Domain.Enums;
using Xunit;

namespace SafeRouteAI.API.Tests;

public class ApiRiskServiceBridgeTests
{
    [Fact]
    public async Task ValidPrediction_MapsScoreAndLevel()
    {
        var handler = new StubHttpHandler("""
        {
            "results": [
                {
                    "status": "Valid",
                    "riskScore": 0.85,
                    "riskLevel": "High",
                    "missingFields": [],
                    "invalidFields": []
                }
            ],
            "weatherProvider": "Open-Meteo",
            "routePointsProcessed": 1
        }
        """);

        using var client = CreateClient(handler);

        var bridge = new ApiRiskServiceBridge(
            new PythonRoutePredictionClient(client));

        var result = await bridge.AssessAsync(
            CreateRoute(),
            CreateWeather());

        Assert.Equal(
            AssessmentStatus.Valid,
            result.Status);

        Assert.Equal(0.85, result.Score);

        Assert.Equal(
            RiskLevel.High,
            result.Level);

        // Verify that the request uses the Python
        // /predict-route segments contract.
        using var document = JsonDocument.Parse(
            handler.RequestBody);

        var segments = document.RootElement
            .GetProperty("segments");

        Assert.Equal(2, segments.GetArrayLength());
    }

    [Fact]
    public async Task InsufficientPrediction_ReturnsNoScore()
    {
        var handler = new StubHttpHandler("""
        {
            "results": [
                {
                    "status": "InsufficientData",
                    "riskScore": null,
                    "riskLevel": null,
                    "missingFields": ["num_lanes"],
                    "invalidFields": []
                }
            ],
            "weatherProvider": "Open-Meteo",
            "routePointsProcessed": 1
        }
        """);

        using var client = CreateClient(handler);

        var bridge = new ApiRiskServiceBridge(
            new PythonRoutePredictionClient(client));

        var result = await bridge.AssessAsync(
            CreateRoute(),
            CreateWeather());

        Assert.Equal(
            AssessmentStatus.InsufficientData,
            result.Status);

        Assert.Null(result.Score);
        Assert.Null(result.Level);

        Assert.Contains(
            "num_lanes",
            result.Factors);
    }

    [Fact]
    public async Task RouteGeometry_IsSentToPython()
    {
        var handler = new StubHttpHandler("""
        {
            "results": [],
            "weatherProvider": "Open-Meteo",
            "routePointsProcessed": 0
        }
        """);

        using var client = CreateClient(handler);

        var bridge = new ApiRiskServiceBridge(
            new PythonRoutePredictionClient(client));

        await bridge.AssessAsync(
            CreateRoute(),
            CreateWeather());

        // Parse JSON so numeric formatting
        // does not affect the test.
        using var document = JsonDocument.Parse(
            handler.RequestBody);

        var segments = document.RootElement
            .GetProperty("segments");

        Assert.Equal(2, segments.GetArrayLength());

        // First GeoJSON coordinate: [longitude, latitude]
        Assert.Equal(
            -26.0,
            segments[0]
                .GetProperty("latitude")
                .GetDouble());

        Assert.Equal(
            28.0,
            segments[0]
                .GetProperty("longitude")
                .GetDouble());

        // Second GeoJSON coordinate.
        Assert.Equal(
            -26.1,
            segments[1]
                .GetProperty("latitude")
                .GetDouble());

        Assert.Equal(
            28.1,
            segments[1]
                .GetProperty("longitude")
                .GetDouble());
    }

    [Fact]
    public async Task PythonHttpError_IsPropagated()
    {
        var handler = new StubHttpHandler(
            """{"detail":"Python service unavailable"}""",
            HttpStatusCode.BadGateway);

        using var client = CreateClient(handler);

        var bridge = new ApiRiskServiceBridge(
            new PythonRoutePredictionClient(client));

        await Assert.ThrowsAsync<HttpRequestException>(
            () => bridge.AssessAsync(
                CreateRoute(),
                CreateWeather()));
    }

    private static HttpClient CreateClient(
        StubHttpHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri(
                "http://127.0.0.1:8006/")
        };
    }

    private static RouteOptionDto CreateRoute() =>
        new(
            "route-001",
            25.0,
            30.0,
            """
            {
                "type": "LineString",
                "coordinates": [
                    [28.0, -26.0],
                    [28.1, -26.1]
                ]
            }
            """);

    private static WeatherResult CreateWeather() =>
        new(
            true,
            1.0,
            10000.0,
            12.0,
            22.0,
            65.0,
            "Clear");

    private sealed class StubHttpHandler : HttpMessageHandler
    {
        private readonly string _response;
        private readonly HttpStatusCode _statusCode;

        public string RequestBody { get; private set; } = "";

        public StubHttpHandler(
            string response,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _response = response;
            _statusCode = statusCode;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestBody = await request.Content!
                .ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(
                    _response,
                    Encoding.UTF8,
                    "application/json")
            };
        }
    }
}
