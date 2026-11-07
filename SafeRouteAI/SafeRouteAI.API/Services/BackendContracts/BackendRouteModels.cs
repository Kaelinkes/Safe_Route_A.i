
namespace SafeRouteAI.API.Services.BackendContracts;

public sealed record BackendRouteOption(
    string RouteId,
    double DistanceKm,
    double DurationMinutes,
    string Geometry);

public sealed record BackendRiskAssessment(
    string Status,
    double? Score,
    string? Level);

public sealed record BackendRankedRoute(
    int Rank,
    BackendRouteOption Route,
    BackendRiskAssessment Risk,
    bool IsHighRisk);
