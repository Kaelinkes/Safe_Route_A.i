namespace SafeRouteAI.Application.DTOs;

public record Coordinate(double Lat, double Lon);

public record RouteOptionDto(
    string RouteId,
    double DistanceKm,
    double DurationMinutes,
    string Geometry);

public record WeatherResult(bool IsAvailable, double Precipitation, double Visibility, double WindSpeed);