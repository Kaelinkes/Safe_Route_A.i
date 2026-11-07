
using SafeRouteAI.API.DTOs;

namespace SafeRouteAI.API.Interfaces;

public interface IRiskPredictionService
{
    Task<RiskPredictionResponse> PredictAsync(
        RiskPredictionRequest request,
        CancellationToken cancellationToken = default);
}
