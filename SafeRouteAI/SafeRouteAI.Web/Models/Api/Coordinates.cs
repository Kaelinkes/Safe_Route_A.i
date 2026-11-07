using System.ComponentModel.DataAnnotations;

namespace SafeRouteAI.Web.Models.Api;

public sealed class Coordinates
{
    [Required]
    [Range(-90, 90)]
    public required double Latitude { get; init; }

    [Required]
    [Range(-180, 180)]
    public required double Longitude { get; init; }
}