using System.ComponentModel.DataAnnotations;

namespace SafeRouteAI.Web.Models.Api;

public sealed class RouteComparisonRequest : IValidatableObject
{
    [Required]
    public required Coordinates Origin { get; init; }

    [Required]
    public required Coordinates Destination { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (Origin is not null && Destination is not null &&
            Origin.Latitude == Destination.Latitude &&
            Origin.Longitude == Destination.Longitude)
        {
            yield return new ValidationResult(
                "Origin and destination cannot be identical.",
                new[] { nameof(Destination) });
        }
    }
}