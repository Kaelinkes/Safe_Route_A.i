
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
                // Provide configuration to Backend services.
                services.AddSingleton<IConfiguration>(configuration);

                // Keep existing routing registration behavior
                // for API tests and standalone consumers.
                services.AddInfrastructure();

                var riskProvider =
                    configuration["MlIntegration:RouteRiskProvider"]
                    ?? "Fake";

                switch (riskProvider.Trim().ToLowerInvariant())
                {
                    case "fake":
                        break;

                    case "apibridge":
                        services.AddScoped<
                            IRiskService,
                            ApiRiskServiceBridge>();
                        break;

                    default:
                        throw new InvalidOperationException(
                            $"Unsupported route risk provider '{riskProvider}'.");
                }

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
