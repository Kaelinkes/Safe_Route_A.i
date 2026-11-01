
namespace SafeRouteAI.Domain.Entities;

public class Route
{
    public int RouteID { get; set; }

    public int UserID { get; set; }

    public string Origin { get; set; } = string.Empty;

    public string Destination { get; set; } = string.Empty;

    public DateTime RequestDateTime { get; set; }

    // Navigation property
    public User? User { get; set; }
}
