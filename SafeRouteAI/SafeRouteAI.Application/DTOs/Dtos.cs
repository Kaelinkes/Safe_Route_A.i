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

public record RankedRouteDto(
    int Rank,
    RouteOptionDto Route,
    RiskAssessmentDto Risk,
    bool IsHighRisk);
