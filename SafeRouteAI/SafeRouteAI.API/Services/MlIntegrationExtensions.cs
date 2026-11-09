
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafeRouteAI.API.Interfaces;

namespace SafeRouteAI.API.Services;

public static class MlIntegrationExtensions
{
    public static IServiceCollection AddMlIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(
            MlIntegrationOptions.SectionName);

        services.Configure<MlIntegrationOptions>(section);

        // New Python route-risk service.
        // Configured through MlIntegration:BaseUrl.
        var routeBaseUrl = GetValidatedUrl(
            section["BaseUrl"],
            "http://127.0.0.1:8006/",
            "MlIntegration:BaseUrl");

        // Existing Python ML service used by your adapter.
        // Configured independently to preserve the working endpoint.
        var legacyBaseUrl = GetValidatedUrl(
            section["LegacyBaseUrl"],
            "http://127.0.0.1:8001/",
            "MlIntegration:LegacyBaseUrl");

        var timeoutSeconds = int.TryParse(
            section["TimeoutSeconds"],
            out var seconds) && seconds > 0
                ? seconds
                : 90;

        var timeout = TimeSpan.FromSeconds(timeoutSeconds);

        // Existing ML health client.
        services.AddHttpClient<PythonMlHealthClient>(client =>
        {
            client.BaseAddress = legacyBaseUrl;
            client.Timeout = timeout;
        });

        // Existing ML prediction client.
        services.AddHttpClient<PythonMlPredictionClient>(client =>
        {
            client.BaseAddress = legacyBaseUrl;
            client.Timeout = timeout;
        });

        // Preserve your working route-risk adapter.
        services.AddScoped<RouteRiskPredictionAdapter>();

        // New automatic route-risk prediction client.
        services.AddHttpClient<PythonRoutePredictionClient>(
            client =>
            {
                client.BaseAddress = routeBaseUrl;
                client.Timeout = timeout;
            });

        // Provider for the existing risk prediction contract.
        var provider = section["Provider"] ?? "Mock";

        switch (provider.Trim().ToLowerInvariant())
        {
            case "mock":
                services.AddScoped<
                    IRiskPredictionService,
                    MockRiskPredictionService>();
                break;

            case "python":
                services.AddHttpClient<
                    IRiskPredictionService,
                    PythonRiskPredictionService>(client =>
                {
                    client.BaseAddress = routeBaseUrl;
                    client.Timeout = timeout;
                });
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported ML provider '{provider}'. " +
                    "Supported providers are 'Mock' and 'Python'.");
        }

        return services;
    }

    private static Uri GetValidatedUrl(
        string? configuredUrl,
        string fallbackUrl,
        string settingName)
    {
        var value = string.IsNullOrWhiteSpace(configuredUrl)
            ? fallbackUrl
            : configuredUrl;

        if (!Uri.TryCreate(
                value,
                UriKind.Absolute,
                out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp &&
             uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                $"{settingName} must be a valid HTTP URL.");
        }

        return new Uri(uri.AbsoluteUri.TrimEnd('/') + "/");
    }
}
