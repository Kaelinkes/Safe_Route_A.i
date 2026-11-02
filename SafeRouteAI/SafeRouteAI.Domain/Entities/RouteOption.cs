namespace SafeRouteAI.Domain.Entities;

public class RouteOption
{
    public int RouteOptionId { get; set; }
    public int RouteId { get; set; }
    public string RouteInformation { get; set; } = "";
    public int TravelTime { get; set; }
    public decimal Distance { get; set; }
    public decimal? RiskScore { get; set; }
    public List<RoadSegment> Segments { get; set; } = new();
}