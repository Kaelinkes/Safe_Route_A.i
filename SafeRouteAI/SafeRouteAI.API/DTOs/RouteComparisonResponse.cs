
using SafeRouteAI.API.DTOs.Enums;

namespace SafeRouteAI.API.DTOs;

public class RouteComparisonResponse
{
    public List<RouteOptionResponse> Routes { get; set; } = new();

    public bool IsMock { get; set; }
}

public class RouteOptionResponse
{
    public string RouteId { get; set; } = string.Empty;

    public double DistanceKm { get; set; }

    public double DurationMinutes { get; set; }

    public RiskLevel RiskLevel { get; set; } =
        RiskLevel.InsufficientData;

    public double? RiskScore { get; set; }

    public string? Geometry { get; set; }
}
