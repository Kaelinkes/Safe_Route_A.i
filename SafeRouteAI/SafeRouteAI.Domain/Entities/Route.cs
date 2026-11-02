namespace SafeRouteAI.Domain.Entities;

public class Route
{
    public int RouteId { get; set; }
    public string UserId { get; set; } = "";
    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";
    public DateTime RequestDateTime { get; set; }
    public List<RouteOption> Options { get; set; } = new();
}