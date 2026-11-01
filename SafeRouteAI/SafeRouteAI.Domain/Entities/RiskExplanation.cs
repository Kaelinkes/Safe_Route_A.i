
namespace SafeRouteAI.Domain.Entities;

public class RiskExplanation
{
    public int RiskExplanationID { get; set; }

    public int RiskAssessmentID { get; set; }

    public string ExplanationText { get; set; } = string.Empty;

    // Navigation property
    public RiskAssessment? RiskAssessment { get; set; }
}
