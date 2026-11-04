namespace SafeRouteAI.Web.Models.Api;

public sealed class HealthResponse
{
    public required string Status { get; init; }

    public required string Service { get; init; }

    public required string Version { get; init; }
}