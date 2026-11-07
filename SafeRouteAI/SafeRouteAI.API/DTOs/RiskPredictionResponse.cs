
using SafeRouteAI.API.DTOs.Enums;

namespace SafeRouteAI.API.DTOs;

public class RiskPredictionResponse
{
    public string SegmentId { get; set; } = string.Empty;

    public RiskLevel RiskLevel { get; set; } =
        RiskLevel.InsufficientData;

    public double? RiskScore { get; set; }

    public string? ModelVersion { get; set; }

    public bool IsMock { get; set; }
}
