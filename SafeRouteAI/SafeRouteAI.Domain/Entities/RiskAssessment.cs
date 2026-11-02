using SafeRouteAI.Domain.Enums;
namespace SafeRouteAI.Domain.Entities;

public class RiskAssessment
{
    public int RiskAssessmentId { get; set; }
    public int RoadSegmentId { get; set; }
    public decimal? RiskScore { get; set; }
    public RiskLevel? RiskLevel { get; set; }
    public AssessmentStatus AssessmentStatus { get; set; }
    public int? WeatherLogId { get; set; }
}