namespace SafeRouteAI.Domain.Entities;

public class RouteOption
{
    public int RouteOptionID { get; set; }

    public int RouteID { get; set; }

    public string RouteInfo { get; set; } = string.Empty;

    public string Geometry { get; set; } = string.Empty;

    public double Distance { get; set; }

    public double EstTimeMin { get; set; }

    public double? RiskScore { get; set; }

    public int Rank { get; set; }

    public Route? Route { get; set; }

    public ICollection<RouteOptionRoadSegment> RouteOptionRoadSegments { get; set; }
        = new List<RouteOptionRoadSegment>();
}