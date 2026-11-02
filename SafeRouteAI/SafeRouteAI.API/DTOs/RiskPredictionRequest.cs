
using System.ComponentModel.DataAnnotations;

namespace SafeRouteAI.API.DTOs;

public class RiskPredictionRequest
{
    [Required]
    public string SegmentId { get; set; } = string.Empty;

    [Required]
    public Dictionary<string, double> Features { get; set; } = new();
}
