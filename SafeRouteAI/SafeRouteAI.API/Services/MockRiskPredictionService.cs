
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.DTOs.Enums;
using SafeRouteAI.API.Interfaces;

namespace SafeRouteAI.API.Services;

public class MockRiskPredictionService : IRiskPredictionService
{
    public Task<RiskPredictionResponse> PredictAsync(
        RiskPredictionRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = new RiskPredictionResponse
        {
            SegmentId = request.SegmentId,
            RiskLevel = RiskLevel.InsufficientData,
            RiskScore = null,
            ModelVersion = null,
            IsMock = true
        };

        return Task.FromResult(response);
    }
}
