
using System.ComponentModel.DataAnnotations;

namespace SafeRouteAI.API.DTOs;

public class RouteComparisonRequest : IValidatableObject
{
    [Required]
    public Coordinates Origin { get; set; } = null!;

    [Required]
    public Coordinates Destination { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (Origin == null || Destination == null)
            yield break;

        if (Origin.Latitude == Destination.Latitude &&
            Origin.Longitude == Destination.Longitude)
        {
            yield return new ValidationResult(
                "Origin and destination cannot be identical.",
                new[] { nameof(Destination) });
        }
    }
}

public class Coordinates
{
    [Required]
    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Required]
    [Range(-180, 180)]
    public double? Longitude { get; set; }
}
