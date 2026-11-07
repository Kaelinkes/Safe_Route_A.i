namespace SafeRouteAI.Web.Models.Api;

public sealed class RiskPredictionResponse
{
    public required string SegmentId { get; init; }

    public required ApiRiskLevel RiskLevel { get; init; }

    public required double? RiskScore { get; init; }

    public required string? ModelVersion { get; init; }

    public required bool IsMock { get; init; }
}