namespace SafeRouteAI.API.DTOs.Routes;

public sealed record RouteHistoryResponse(
    int RouteId,
    string Origin,
    string Destination,
    DateTime RequestedAt,
    IReadOnlyList<RouteHistoryOptionResponse> Options,
    IReadOnlyList<RouteRiskLogResponse> RiskLogs);

public sealed record RouteHistoryOptionResponse(
    int Rank,
    double DistanceKm,
    double DurationMinutes,
    double? RiskScore,
    IReadOnlyList<string> RoadSegments);

public sealed record RouteRiskLogResponse(DateTime LoggedAt, string Snapshot);
