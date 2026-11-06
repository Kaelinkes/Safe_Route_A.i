using SafeRouteAI.Domain.Enums;

namespace SafeRouteAI.Application.DTOs;

public record Coordinate(double Lat, double Lon);

public record RouteOptionDto(
    string RouteId,
    double DistanceKm,
    double DurationMinutes,
    string Geometry);

public record WeatherResult(
    bool IsAvailable,
    double Precipitation,
    double Visibility,
    double WindSpeed,
    double Temperature,
    double Humidity,
    string Condition);

public record RiskAssessmentDto(
    AssessmentStatus Status,
    double? Score,
    RiskLevel? Level,
    List<string> Factors);

public record RankedRouteDto(int Rank, RouteOptionDto Route, RiskAssessmentDto Risk, bool IsHighRisk);

public class CommunityReportDto
{
    public int ReportId { get; set; }
    public int UserId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string IncidentType { get; set; } = "";
    public string? Description { get; set; }
    public DateTime ReportedAt { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public int? ReviewedByAdminId { get; set; }
    public DateTime? ReviewedAt { get; set; }
}