using System.ComponentModel.DataAnnotations;

namespace SafeRouteAI.Web.Models.Api;

public sealed class RiskPredictionRequest : IValidatableObject
{
    [Required]
    [MinLength(1)]
    public required string SegmentId { get; init; }

    [Required]
    public required Dictionary<string, double> Features { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (Features is null || Features.Count == 0)
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

            if (!double.IsFinite(feature.Value))
            {
                yield return new ValidationResult(
                    "Feature values must be finite numbers.",
                    new[] { nameof(Features) });
            }
        }
    }
}