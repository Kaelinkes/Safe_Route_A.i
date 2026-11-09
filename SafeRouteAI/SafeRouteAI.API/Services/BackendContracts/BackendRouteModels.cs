
namespace SafeRouteAI.API.Services.BackendContracts;

public sealed record BackendRouteOption(
    string RouteId,
    double DistanceKm,
    double DurationMinutes,
    string Geometry,
    List<BackendRoadSegmentAttribute>? RoadSegments = null);

public sealed record BackendRoadSegmentAttribute(
    int FromIndex,
    int ToIndex,
    string Attribute,
    string Value);

public sealed record BackendRiskAssessment(
    string Status,
    double? Score,
    string? Level,
    string PredictionSource = "Unknown");

public sealed record BackendRankedRoute(
    int Rank,
    BackendRouteOption Route,
    BackendRiskAssessment Risk,
    bool IsHighRisk);
