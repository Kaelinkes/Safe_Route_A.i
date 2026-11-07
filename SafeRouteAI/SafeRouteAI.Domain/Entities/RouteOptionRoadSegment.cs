
namespace SafeRouteAI.Domain.Entities;

public class RouteOptionRoadSegment
{
    public int RouteOptionID { get; set; }

    public int RoadSegmentID { get; set; }

    public int SegmentOrder { get; set; }

    // Navigation properties
    public RouteOption? RouteOption { get; set; }

    public RoadSegment? RoadSegment { get; set; }
}
