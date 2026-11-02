namespace SafeRouteAI.Application.DTOs;

public record Coordinate(double Lat, double Lon);

public record SegmentDto(Coordinate Start, Coordinate End, string? RoadName);

public record RouteOptionDto(int TravelTimeMinutes, double DistanceKm, List<SegmentDto> Segments);

public record WeatherResult(bool IsAvailable, double Precipitation, double Visibility, double WindSpeed);