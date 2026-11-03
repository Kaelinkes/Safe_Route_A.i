namespace SafeRouteAI.Infrastructure.Persistence.Entities;

public sealed class UserRoleRecord { public string RoleCode { get; set; } = string.Empty; }
public sealed class AccountStatusRecord { public string StatusCode { get; set; } = string.Empty; }
public sealed class RequestStatusRecord { public string StatusCode { get; set; } = string.Empty; }
public sealed class RiskCategoryRecord { public string CategoryCode { get; set; } = string.Empty; public short SortOrder { get; set; } }
public sealed class ReportStatusRecord { public string StatusCode { get; set; } = string.Empty; }
public sealed class WeatherSeverityRecord { public string SeverityCode { get; set; } = string.Empty; public short SeverityLevel { get; set; } }
public sealed class ExternalServiceRecord { public string ServiceCode { get; set; } = string.Empty; }

public sealed class UserRecord
{
    public long UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EmailNormalized { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string RoleCode { get; set; } = "user";
    public string StatusCode { get; set; } = "active";
    public short FailedLogins { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
}

public sealed class LoginAttemptRecord
{
    public long AttemptId { get; set; }
    public long? UserId { get; set; }
    public string EmailTried { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset AttemptedAt { get; set; }
}

public sealed class UserSessionRecord
{
    public string SessionId { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}

public sealed class RoadSegmentRecord
{
    public long SegmentId { get; set; }
    public string? ExternalRef { get; set; }
    public string? RoadName { get; set; }
    public string? RoadType { get; set; }
    public short? SpeedLimitKmh { get; set; }
    public short? Lanes { get; set; }
    public decimal LengthM { get; set; }
    public decimal StartLat { get; set; }
    public decimal StartLng { get; set; }
    public decimal EndLat { get; set; }
    public decimal EndLng { get; set; }
    public decimal MinLat { get; set; }
    public decimal MaxLat { get; set; }
    public decimal MinLng { get; set; }
    public decimal MaxLng { get; set; }
    public string? Polyline { get; set; }
}

public sealed class HistoricalAccidentRecord
{
    public long AccidentId { get; set; }
    public long? SegmentId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public short? Severity { get; set; }
    public short? VehiclesInvolved { get; set; }
    public short? Casualties { get; set; }
    public string? WeatherCondition { get; set; }
    public string? LightCondition { get; set; }
    public string Source { get; set; } = string.Empty;
}

public sealed class RouteRequestRecord
{
    public long RequestId { get; set; }
    public long UserId { get; set; }
    public string OriginLabel { get; set; } = string.Empty;
    public decimal OriginLat { get; set; }
    public decimal OriginLng { get; set; }
    public string DestinationLabel { get; set; } = string.Empty;
    public decimal DestinationLat { get; set; }
    public decimal DestinationLng { get; set; }
    public DateTimeOffset PlannedDeparture { get; set; }
    public string StatusCode { get; set; } = "pending";
    public string? ErrorMessage { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int? ProcessingMs { get; set; }
}

public sealed class ExternalApiCallRecord
{
    public long CallId { get; set; }
    public long? RequestId { get; set; }
    public string ServiceCode { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public short? HttpStatus { get; set; }
    public bool Success { get; set; }
    public int? LatencyMs { get; set; }
    public string? ErrorDetail { get; set; }
    public DateTimeOffset CalledAt { get; set; }
}

public sealed class CandidateRouteRecord
{
    public long RouteId { get; set; }
    public long RequestId { get; set; }
    public string? ProviderRouteRef { get; set; }
    public string? Summary { get; set; }
    public decimal DistanceM { get; set; }
    public int DurationS { get; set; }
    public string Polyline { get; set; } = string.Empty;
    public decimal? OverallRiskScore { get; set; }
    public string? OverallRiskCategory { get; set; }
    public decimal? MaxSegmentRiskScore { get; set; }
    public bool IsHighRisk { get; set; }
    public string? HighRiskReason { get; set; }
    public short? RankByTime { get; set; }
    public short? RankByRisk { get; set; }
    public short? OverallRank { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class RouteSegmentRecord
{
    public long RouteId { get; set; }
    public int SeqNo { get; set; }
    public long SegmentId { get; set; }
    public int? EstimatedTravelTimeS { get; set; }
}

public sealed class WeatherObservationRecord
{
    public long WeatherId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset ValidTo { get; set; }
    public bool IsForecast { get; set; }
    public string ConditionText { get; set; } = string.Empty;
    public decimal? TemperatureC { get; set; }
    public decimal? PrecipitationMm { get; set; }
    public int? VisibilityM { get; set; }
    public decimal? WindSpeedKmh { get; set; }
    public string SeverityCode { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DateTimeOffset FetchedAt { get; set; }
    public string? RawPayload { get; set; }
}

public sealed class IncidentTypeRecord
{
    public short IncidentTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int DefaultTtlMinutes { get; set; }
    public decimal RiskWeight { get; set; }
}

public sealed class CommunityReportRecord
{
    public long ReportId { get; set; }
    public long UserId { get; set; }
    public short IncidentTypeId { get; set; }
    public long? SegmentId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Description { get; set; } = string.Empty;
    public string StatusCode { get; set; } = "pending";
    public DateTimeOffset ReportedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public long? ModeratedBy { get; set; }
    public DateTimeOffset? ModeratedAt { get; set; }
    public string? ModerationNote { get; set; }
}

public sealed class ReportStatusHistoryRecord
{
    public long HistoryId { get; set; }
    public long ReportId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public long? ChangedBy { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
}

public sealed class RiskModelVersionRecord
{
    public int ModelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTimeOffset TrainedAt { get; set; }
    public DateOnly? TrainingDataFrom { get; set; }
    public DateOnly? TrainingDataTo { get; set; }
    public string? Metrics { get; set; }
    public string ModelArtifactUri { get; set; } = string.Empty;
    public string? ArtifactChecksum { get; set; }
    public int MinDataPoints { get; set; }
    public string? ActiveFlag { get; set; }
}

public sealed class SegmentRiskAssessmentRecord
{
    public long AssessmentId { get; set; }
    public long RouteId { get; set; }
    public long SegmentId { get; set; }
    public int ModelId { get; set; }
    public long? WeatherId { get; set; }
    public decimal? RiskScore { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public bool DataSufficient { get; set; }
    public bool HasAccidentData { get; set; }
    public bool HasWeatherData { get; set; }
    public bool HasCommunityData { get; set; }
    public string ExplanationSummary { get; set; } = string.Empty;
    public DateTimeOffset AssessedAt { get; set; }
}

public sealed class AssessmentCommunityReportRecord
{
    public long AssessmentId { get; set; }
    public long ReportId { get; set; }
}

public sealed class RiskFactorRecord
{
    public short FactorId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ExplanationTemplate { get; set; } = string.Empty;
}

public sealed class AssessmentFactorRecord
{
    public long AssessmentId { get; set; }
    public short FactorId { get; set; }
    public decimal Contribution { get; set; }
    public short RankOrder { get; set; }
    public string ExplanationText { get; set; } = string.Empty;
}

public sealed class HighRiskCriteriaRecord
{
    public int CriteriaId { get; set; }
    public decimal MinRiskScore { get; set; }
    public string MinWeatherSeverity { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class RouteWarningRecord
{
    public long WarningId { get; set; }
    public long RouteId { get; set; }
    public long? SegmentId { get; set; }
    public int CriteriaId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class RouteRiskLogRecord
{
    public long LogId { get; set; }
    public long RequestId { get; set; }
    public long? UserId { get; set; }
    public int? ModelId { get; set; }
    public string Snapshot { get; set; } = string.Empty;
    public DateTimeOffset LoggedAt { get; set; }
}