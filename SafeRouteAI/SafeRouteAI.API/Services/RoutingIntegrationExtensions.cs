
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafeRouteAI.API.Interfaces;
using SafeRouteAI.API.Services.BackendContracts;
using SafeRouteAI.Infrastructure;

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
                // Make configuration available to backend services,
                // including RouteService.
                services.AddSingleton<IConfiguration>(configuration);

                // Register backend routing, weather, fake risk,
                // and route comparison services.
                services.AddInfrastructure();

                // Connect the API adapter to the real backend.
                services.AddScoped<
                    IBackendRouteClient,
                    RealBackendRouteClient>();

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
