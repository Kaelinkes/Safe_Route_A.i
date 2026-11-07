
namespace SafeRouteAI.API.Services;

public sealed class RoutingIntegrationOptions
{
    public const string SectionName = "RoutingIntegration";

    // Disabled until the backend routing service is integrated.
    public string Provider { get; set; } = "Unavailable";
}
