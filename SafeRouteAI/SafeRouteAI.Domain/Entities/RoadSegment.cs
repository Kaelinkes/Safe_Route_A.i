namespace SafeRouteAI.Domain.Entities;

public class RoadSegment
{
    public int RoadSegmentId { get; set; }
    public int RouteOptionId { get; set; }
    public double StartLat { get; set; }
    public double StartLon { get; set; }
    public double EndLat { get; set; }
    public double EndLon { get; set; }
    public string? RoadName { get; set; }
}