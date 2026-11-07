using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Domain.Enums;

namespace SafeRouteAI.Infrastructure.Services;

public class FakeRiskService : IRiskService
{
    public Task<RiskAssessmentDto> AssessAsync(
        RouteOptionDto route,
        WeatherResult weather,
        CancellationToken cancellationToken = default)
    {
        if (!weather.IsAvailable)
        {
            return Task.FromResult(new RiskAssessmentDto(
                AssessmentStatus.InsufficientData,
                null,
                null,
                []));
        }

        var factors = new List<string>();
        var score = 0.2;

        if (weather.Precipitation > 2)
        {
            score += 0.3;
            factors.Add("Rain");
        }

        if (weather.Visibility < 1000)
        {
            score += 0.3;
            factors.Add("Low visibility");
        }

        if (weather.WindSpeed > 40)
        {
            score += 0.1;
            factors.Add("Strong wind");
        }

        if (route.DistanceKm > 30)
        {
            score += 0.1;
            factors.Add("Long route");
        }

        score = Math.Min(score, 1);
        var level = score < 0.4 ? RiskLevel.Low : score < 0.7 ? RiskLevel.Medium : RiskLevel.High;
        return Task.FromResult(new RiskAssessmentDto(AssessmentStatus.Valid, score, level, factors));
    }
}
