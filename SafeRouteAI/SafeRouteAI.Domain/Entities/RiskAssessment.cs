
namespace SafeRouteAI.Domain.Entities;
using SafeRouteAI.Domain.Enums;

public class RiskAssessment
{
    public int RiskAssessmentID { get; set; }

    public int RoadSegmentID { get; set; }

    public int? WeatherLogID { get; set; }

    public double? RiskScore { get; set; }

    public RiskLevel? RiskLevel { get; set; }

    public AssessmentStatus AssessmentStatus { get; set; }

    // Navigation properties
    public RoadSegment? RoadSegment { get; set; }

    public WeatherLog? WeatherLog { get; set; }

    public RiskExplanation? RiskExplanation { get; set; }
}
