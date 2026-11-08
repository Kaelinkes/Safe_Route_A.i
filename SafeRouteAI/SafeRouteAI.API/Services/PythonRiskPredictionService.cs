
using System.Net.Http.Json;
using System.Text.Json;
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.Interfaces;

namespace SafeRouteAI.API.Services;

public sealed class PythonRiskPredictionService : IRiskPredictionService
{
    private readonly HttpClient _http;

    public PythonRiskPredictionService(HttpClient http)
    {
        _http = http;
    }

    public async Task<RiskPredictionResponse> PredictAsync(
        RiskPredictionRequest request,
        CancellationToken cancellationToken = default)
    {
        // The current C# request is not yet compatible with
        // the Python model's required route-segment contract.
        // Do not send incomplete or misleading model inputs.
        await Task.CompletedTask;

        return new RiskPredictionResponse
        {
            SegmentId = request.SegmentId,
            RiskLevel = DTOs.Enums.RiskLevel.InsufficientData,
            RiskScore = null,
            ModelVersion = "LightGBM - awaiting feature mapping",
            IsMock = false
        };
    }
}
