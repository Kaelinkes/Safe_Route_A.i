
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.Interfaces;
using SafeRouteAI.API.Services;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Domain.Enums;
using Xunit;

using ApiRiskLevel = SafeRouteAI.API.DTOs.Enums.RiskLevel;

namespace SafeRouteAI.API.Tests;

public class ApiRiskServiceBridgeTests
{
    [Fact]
    public async Task ValidPrediction_MapsScoreAndLevel()
    {
        var predictor = new StubPredictionService(
            new RiskPredictionResponse
            {
                RiskLevel = ApiRiskLevel.High,
                RiskScore = 0.85,
                IsMock = false
            });

        var bridge = new ApiRiskServiceBridge(predictor);

        var result = await bridge.AssessAsync(
            CreateRoute(),
            CreateWeather());

        Assert.Equal(AssessmentStatus.Valid, result.Status);
        Assert.Equal(0.85, result.Score);
        Assert.Equal(RiskLevel.High, result.Level);

        Assert.NotNull(predictor.LastRequest);
        Assert.Equal(
            "route-001",
            predictor.LastRequest!.SegmentId);

        Assert.Equal(
            25.0,
            predictor.LastRequest.Features["distanceKm"]);

        Assert.Equal(
            12.0,
            predictor.LastRequest.Features["windSpeed"]);
    }

    [Fact]
    public async Task InsufficientPrediction_ReturnsNoScore()
    {
        var predictor = new StubPredictionService(
            new RiskPredictionResponse
            {
                RiskLevel = ApiRiskLevel.InsufficientData,
                RiskScore = null,
                IsMock = true
            });

        var bridge = new ApiRiskServiceBridge(predictor);

        var result = await bridge.AssessAsync(
            CreateRoute(),
            CreateWeather());

        Assert.Equal(
            AssessmentStatus.InsufficientData,
            result.Status);

        Assert.Null(result.Score);
        Assert.Null(result.Level);
    }

    [Fact]
    public async Task MissingWeather_DoesNotCallPredictionService()
    {
        var predictor = new StubPredictionService(
            new RiskPredictionResponse
            {
                RiskLevel = ApiRiskLevel.High,
                RiskScore = 0.9
            });

        var bridge = new ApiRiskServiceBridge(predictor);

        var weather = CreateWeather() with
        {
            IsAvailable = false
        };

        var result = await bridge.AssessAsync(
            CreateRoute(),
            weather);

        Assert.Equal(
            AssessmentStatus.InsufficientData,
            result.Status);

        Assert.Null(result.Score);
        Assert.Null(predictor.LastRequest);
    }

    [Fact]
    public async Task CancellationToken_IsForwarded()
    {
        var predictor = new StubPredictionService(
            new RiskPredictionResponse
            {
                RiskLevel = ApiRiskLevel.Low,
                RiskScore = 0.2
            });

        var bridge = new ApiRiskServiceBridge(predictor);

        using var source = new CancellationTokenSource();

        await bridge.AssessAsync(
            CreateRoute(),
            CreateWeather(),
            source.Token);

        Assert.Equal(
            source.Token,
            predictor.LastCancellationToken);
    }

    private static RouteOptionDto CreateRoute() =>
        new(
            "route-001",
            25.0,
            30.0,
            "{}");

    private static WeatherResult CreateWeather() =>
        new(
            true,
            1.0,
            10000.0,
            12.0,
            22.0,
            65.0,
            "Clear");

    private sealed class StubPredictionService
        : IRiskPredictionService
    {
        private readonly RiskPredictionResponse _response;

        public RiskPredictionRequest? LastRequest { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public StubPredictionService(
            RiskPredictionResponse response)
        {
            _response = response;
        }

        public Task<RiskPredictionResponse> PredictAsync(
            RiskPredictionRequest request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            LastCancellationToken = cancellationToken;

            return Task.FromResult(_response);
        }
    }
}
