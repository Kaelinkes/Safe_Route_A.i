namespace SafeRouteAI.Domain.Entities;

public class RoadSegment
{
    public int RoadSegmentID { get; set; }

    // Start and end coordinates
    public decimal StartLatitude { get; set; }
    public decimal StartLongitude { get; set; }

    public decimal EndLatitude { get; set; }
    public decimal EndLongitude { get; set; }

    // Proposed road-related attributes
    public string? RoadType { get; set; }
    public string? RoadName { get; set; }

    // Navigation properties
    public ICollection<RouteOptionRoadSegment> RouteOptionRoadSegments { get; set; }
        = new List<RouteOptionRoadSegment>();

    public ICollection<RiskAssessment> RiskAssessments { get; set; }
        = new List<RiskAssessment>();
}

/*A note for checking with the database team: Phase 4 uses the names StartPoint and EndPoint, 
while this implementation represents those points using four decimal coordinate fields. 
RoadType and RoadName are proposed examples of the road-related attributes, not confirmed exact database column names. - Kaelin Kesavjee*/