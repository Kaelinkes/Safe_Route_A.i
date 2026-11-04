using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Domain.Enums;

namespace SafeRouteAI.Application.Services;

public static class ExplanationBuilder
{
    public static string Build(RiskAssessmentDto risk)
    {
        if (risk.Status == AssessmentStatus.InsufficientData)
            return "A full risk assessment could not be completed because required data, such as weather, was unavailable.";

        if (risk.Factors.Count == 0)
            return $"{risk.Level} risk. No major risk factors were detected.";

        return $"{risk.Level} risk, mainly because of: {string.Join(", ", risk.Factors.Select(f => f.ToLowerInvariant()))}.";
    }
}