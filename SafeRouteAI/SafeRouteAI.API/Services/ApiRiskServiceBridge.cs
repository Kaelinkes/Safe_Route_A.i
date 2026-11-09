
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Domain.Enums;

namespace SafeRouteAI.API.Services;

public sealed class ApiRiskServiceBridge : IRiskService
{
    private readonly PythonRoutePredictionClient _python;

    public ApiRiskServiceBridge(PythonRoutePredictionClient python)
    {
        _python = python;
    }

    public async Task<RiskAssessmentDto> AssessAsync(
        RouteOptionDto route,
        WeatherResult weather,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(route.Geometry))
        {
            return new RiskAssessmentDto(
                AssessmentStatus.InsufficientData,
                null,
                null,
                ["Route geometry is missing."]);
        }

        // Python retrieves weather and road information
        // and evaluates the route segments.
        var response = await _python.PredictRouteAsync(
            route.Geometry,
            cancellationToken);

        var predictions = response.Results
            .Where(p =>
                p.RiskScore.HasValue &&
                double.IsFinite(p.RiskScore.Value))
            .ToList();

        if (predictions.Count == 0)
        {
            var missing = response.Results
                .SelectMany(p => p.MissingFields)
                .Distinct()
                .ToList();

            if (missing.Count == 0)
                missing.Add("Python could not assess any route segments.");

            return new RiskAssessmentDto(
                AssessmentStatus.InsufficientData,
                null,
                null,
                missing);
        }

        // Use the highest-risk assessed segment as the
        // overall route risk.
        var worst = predictions
            .OrderByDescending(p => p.RiskScore!.Value)
            .First();

        var score = worst.RiskScore!.Value;

        var level = Enum.TryParse<RiskLevel>(
            worst.RiskLevel,
            true,
            out var parsedLevel)
                ? parsedLevel
                : score <= 0.27
                    ? RiskLevel.Low
                    : score <= 0.41
                        ? RiskLevel.Medium
                        : RiskLevel.High;

        var factors = new List<string>
        {
            $"Python assessed {predictions.Count} route segments."
        };

        return new RiskAssessmentDto(
            AssessmentStatus.Valid,
            score,
            level,
            factors);
    }
}
