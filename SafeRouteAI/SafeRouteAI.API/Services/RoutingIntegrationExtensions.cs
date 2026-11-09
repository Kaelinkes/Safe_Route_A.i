
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
                // Provide configuration to backend services.
                services.AddSingleton<IConfiguration>(configuration);

                // Ensure standalone routing registration can
                // resolve the Python route prediction client.
                //
                // Program.cs already calls AddMlIntegration(),
                // so avoid registering the client twice.
                if (!services.Any(descriptor =>
                        descriptor.ServiceType ==
                        typeof(PythonRoutePredictionClient)))
                {
                    var mlSection = configuration.GetSection(
                        MlIntegrationOptions.SectionName);

                    var baseUrl = mlSection["BaseUrl"];

                    if (string.IsNullOrWhiteSpace(baseUrl))
                    {
                        baseUrl = "http://127.0.0.1:8006/";
                    }

                    // Validate the Python service address.
                    if (!Uri.TryCreate(
                            baseUrl,
                            UriKind.Absolute,
                            out var uri) ||
                        (uri.Scheme != Uri.UriSchemeHttp &&
                         uri.Scheme != Uri.UriSchemeHttps))
                    {
                        throw new InvalidOperationException(
                            "MlIntegration:BaseUrl must be " +
                            "a valid HTTP URL.");
                    }

                    var normalizedBaseUrl = new Uri(
                        uri.AbsoluteUri.TrimEnd('/') + "/");

                    var timeoutSeconds = int.TryParse(
                        mlSection["TimeoutSeconds"],
                        out var seconds) && seconds > 0
                            ? seconds
                            : 90;

                    services.AddHttpClient<
                        PythonRoutePredictionClient>(client =>
                    {
                        client.BaseAddress = normalizedBaseUrl;

                        client.Timeout = TimeSpan.FromSeconds(
                            timeoutSeconds);
                    });
                }

                // Register existing infrastructure services.
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
                            $"Unsupported route risk provider " +
                            $"'{riskProvider}'.");
                }

                // Register the real backend route client.
                services.AddScoped<
                    IBackendRouteClient,
                    RealBackendRouteClient>();

                // Register the backend route comparison adapter.
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
