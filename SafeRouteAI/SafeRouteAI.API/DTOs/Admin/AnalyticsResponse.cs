namespace SafeRouteAI.API.DTOs.Admin;

public sealed record AnalyticsResponse(
    DateOnly From,
    DateOnly To,
    int RouteRequests,
    int CommunityReports,
    int RiskAssessments,
    int InsufficientRiskAssessments,
    int AccidentRecords,
    int ModelVersions,
    IReadOnlyList<DailyCount> RoutesByDay,
    IReadOnlyList<NamedCount> ReportsByStatus,
    IReadOnlyList<NamedCount> ReportsByType,
    IReadOnlyList<NamedCount> RiskByLevel,
    IReadOnlyList<RiskAssessmentDetail> RiskDetails,
    IReadOnlyList<AccidentRecordDetail> Accidents,
    IReadOnlyList<ModelVersionDetail> ModelVersionDetails);

public sealed record DailyCount(DateOnly Date, int Count);

public sealed record NamedCount(string Name, int Count);

public sealed record RiskAssessmentDetail(
    string RoadName,
    double? Score,
    string? Level,
    string Status,
    string? Explanation,
    DateTime? WeatherObservedAt,
    double? Precipitation,
    double? Humidity,
    double? Visibility,
    double? WindSpeed);

public sealed record AccidentRecordDetail(
    DateTime OccurredAt,
    string? Type,
    string? Severity,
    double Latitude,
    double Longitude);

public sealed record ModelVersionDetail(
    string Version,
    DateTime TrainedAt,
    string Dataset,
    string ValidationSummary);
