
using System.ComponentModel.DataAnnotations;

namespace SafeRouteAI.API.DTOs;

public class RiskPredictionRequest : IValidatableObject
{
    [Required]
    [MinLength(1)]
    public string SegmentId { get; set; } = string.Empty;

    [Required]
    public Dictionary<string, double> Features { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (Features == null || Features.Count == 0)
        {
            yield return new ValidationResult(
                "At least one feature is required.",
                new[] { nameof(Features) });

            yield break;
        }

        foreach (var feature in Features)
        {
            if (string.IsNullOrWhiteSpace(feature.Key))
            {
                yield return new ValidationResult(
                    "Feature names cannot be empty.",
                    new[] { nameof(Features) });
            }

            if (double.IsNaN(feature.Value) ||
                double.IsInfinity(feature.Value))
            {
                yield return new ValidationResult(
                    "Feature values must be finite numbers.",
                    new[] { nameof(Features) });
            }
        }
    }
}
