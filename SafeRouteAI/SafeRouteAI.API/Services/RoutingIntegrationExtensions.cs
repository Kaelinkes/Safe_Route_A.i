
using SafeRouteAI.API.Interfaces;
using SafeRouteAI.API.Services.BackendContracts;

namespace SafeRouteAI.API.Services;

public static class RoutingIntegrationExtensions
{
    public static IServiceCollection AddRoutingIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(
            RoutingIntegrationOptions.SectionName);

        services.Configure<RoutingIntegrationOptions>(section);

        var provider = section["Provider"] ?? "Unavailable";

        switch (provider.Trim().ToLowerInvariant())
        {
            case "unavailable":
                services.AddScoped<
                    IRouteComparisonService,
                    RouteComparisonService>();
                break;

            case "backend":
                services.AddScoped<
                    IBackendRouteClient,
                    UnavailableBackendRouteClient>();

                services.AddScoped<
                    IRouteComparisonService,
                    BackendRouteComparisonService>();
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported routing provider '{provider}'.");
        }

        return services;
    }
}
