
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafeRouteAI.API.Interfaces;
using SafeRouteAI.API.Services.BackendContracts;
using SafeRouteAI.Application.Interfaces;
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
                // Make configuration available to Backend services,
                // including RouteService.
                services.AddSingleton<IConfiguration>(configuration);

                // Register Backend routing, weather, risk,
                // and route comparison services.
                services.AddInfrastructure();

                // Select the risk assessment implementation.
                // Fake remains the default until real ML is ready.
                var riskProvider =
                    configuration["MlIntegration:RouteRiskProvider"]
                    ?? "Fake";

                switch (riskProvider.Trim().ToLowerInvariant())
                {
                    case "fake":
                        // Preserve the Backend's existing FakeRiskService.
                        break;

                    case "apibridge":
                        // Override the Backend's risk service with
                        // our API-owned ML bridge.
                        //
                        // Routing and weather services remain unchanged.
                        services.AddScoped<
                            IRiskService,
                            ApiRiskServiceBridge>();
                        break;

                    default:
                        throw new InvalidOperationException(
                            $"Unsupported route risk provider '{riskProvider}'.");
                }

                // Connect the API adapter to Backend routing.
                services.AddScoped<
                    IBackendRouteClient,
                    RealBackendRouteClient>();

                // Expose Backend route comparison through
                // the existing API contract.
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
