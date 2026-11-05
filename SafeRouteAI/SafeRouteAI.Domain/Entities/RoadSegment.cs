namespace SafeRouteAI.Domain.Entities;

public class RoadSegment
{
    public int RoadSegmentID { get; set; }

    public decimal StartLatitude { get; set; }

    public decimal StartLongitude { get; set; }

    public decimal EndLatitude { get; set; }

    public decimal EndLongitude { get; set; }

    public string? RoadType { get; set; }

    public string? RoadName { get; set; }

    public ICollection<RouteOptionRoadSegment> RouteOptionRoadSegments { get; set; }
        = new List<RouteOptionRoadSegment>();

    public ICollection<RiskAssessment> RiskAssessments { get; set; }
        = new List<RiskAssessment>();
}
