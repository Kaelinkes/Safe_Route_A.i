
using System.ComponentModel.DataAnnotations;

namespace SafeRouteAI.API.DTOs;

public class RouteComparisonRequest
{
    [Required]
    public Coordinates Origin { get; set; } = null!;

    [Required]
    public Coordinates Destination { get; set; } = null!;
}

public class Coordinates
{
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }
}
