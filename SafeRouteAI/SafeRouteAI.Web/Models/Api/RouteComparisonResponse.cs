namespace SafeRouteAI.Web.Models.Api;

public sealed class RouteComparisonResponse
{
    public required List<RouteOptionResponse> Routes { get; init; }

    public required bool IsMock { get; init; }
}

public sealed class RouteOptionResponse
{
    public required string RouteId { get; init; }

    public required double DistanceKm { get; init; }

    public required double DurationMinutes { get; init; }

    public required ApiRiskLevel RiskLevel { get; init; }

    public required double? RiskScore { get; init; }

    public required string? Geometry { get; init; }
}