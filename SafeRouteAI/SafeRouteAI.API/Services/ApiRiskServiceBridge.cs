
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.Interfaces;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Domain.Enums;

namespace SafeRouteAI.API.Services;

public sealed class ApiRiskServiceBridge : IRiskService
{
    private readonly IRiskPredictionService _predictions;

    public ApiRiskServiceBridge(
        IRiskPredictionService predictions)
    {
        _predictions = predictions;
    }

    public async Task<RiskAssessmentDto> AssessAsync(
        RouteOptionDto route,
        WeatherResult weather,
        CancellationToken cancellationToken = default)
    {
        if (!weather.IsAvailable)
        {
            return new RiskAssessmentDto(
                AssessmentStatus.InsufficientData,
                null,
                null,
                new List<string>());
        }

        var request = new RiskPredictionRequest
        {
            SegmentId = route.RouteId,
            Features = new Dictionary<string, double>
            {
                ["distanceKm"] = route.DistanceKm,
                ["durationMinutes"] = route.DurationMinutes,
                ["precipitation"] = weather.Precipitation,
                ["visibility"] = weather.Visibility,
                ["windSpeed"] = weather.WindSpeed,
                ["temperature"] = weather.Temperature,
                ["humidity"] = weather.Humidity
            }
        };

        var prediction = await _predictions.PredictAsync(
            request,
            cancellationToken);

        if (prediction.RiskScore is null ||
            prediction.RiskLevel ==
                SafeRouteAI.API.DTOs.Enums.RiskLevel.InsufficientData)
        {
            return new RiskAssessmentDto(
                AssessmentStatus.InsufficientData,
                null,
                null,
                new List<string>());
        }

        var level = prediction.RiskLevel switch
        {
            SafeRouteAI.API.DTOs.Enums.RiskLevel.Low =>
                SafeRouteAI.Domain.Enums.RiskLevel.Low,

            SafeRouteAI.API.DTOs.Enums.RiskLevel.Medium =>
                SafeRouteAI.Domain.Enums.RiskLevel.Medium,

            SafeRouteAI.API.DTOs.Enums.RiskLevel.High =>
                SafeRouteAI.Domain.Enums.RiskLevel.High,

            _ => throw new InvalidOperationException(
                "Unsupported ML risk level.")
        };

        return new RiskAssessmentDto(
            AssessmentStatus.Valid,
            prediction.RiskScore,
            level,
            new List<string>());
    }
}
