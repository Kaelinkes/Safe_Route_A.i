
namespace SafeRouteAI.Domain.Entities;

public class Route
{
    public int RouteID { get; set; }

    // Nullable because anonymous users can request routes.
    public int? UserID { get; set; }

    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime RequestDateTime { get; set; }

    // Navigation property to the associated user, if registered.
    public User? User { get; set; }
}
