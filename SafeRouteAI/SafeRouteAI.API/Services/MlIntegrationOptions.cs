
namespace SafeRouteAI.API.Services;

public sealed class MlIntegrationOptions
{
    public const string SectionName = "MlIntegration";

    // Mock remains the default until the real model is ready.
    public string Provider { get; set; } = "Mock";

    // Reserved for a future Python HTTP service.
    public string? BaseUrl { get; set; }

    public int TimeoutSeconds { get; set; } = 10;
}
