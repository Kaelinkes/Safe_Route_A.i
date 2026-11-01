
namespace SafeRouteAI.Domain.Entities;

public class RiskAssessment
{
    public int RiskAssessmentID { get; set; }

    public int RoadSegmentID { get; set; }

    public int? WeatherLogID { get; set; }

    public double? RiskScore { get; set; }

    public string? RiskLevel { get; set; }

    public string AssessmentStatus { get; set; } = string.Empty;

    // Navigation properties
    public RoadSegment? RoadSegment { get; set; }

    public WeatherLog? WeatherLog { get; set; }

    public RiskExplanation? RiskExplanation { get; set; }
}
