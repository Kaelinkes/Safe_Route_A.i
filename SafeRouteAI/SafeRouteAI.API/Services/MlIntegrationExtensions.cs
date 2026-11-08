
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
        // Read ML integration configuration.
        var section = configuration.GetSection(
            MlIntegrationOptions.SectionName);

        services.Configure<MlIntegrationOptions>(section);

        // Python FastAPI base address.
        var baseUrl = section["BaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "http://127.0.0.1:8001/";
        }

        // Validate Python service URL.
        if (!Uri.TryCreate(
                baseUrl,
                UriKind.Absolute,
                out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp &&
             uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "MlIntegration:BaseUrl must be a valid HTTP URL.");
        }

        // Ensure relative endpoints resolve correctly.
        var normalizedBaseUrl = new Uri(
            uri.AbsoluteUri.TrimEnd('/') + "/");

        // Configure request timeout.
        var timeout = int.TryParse(
            section["TimeoutSeconds"],
            out var seconds) && seconds > 0
                ? seconds
                : 10;

        // Python ML health client.
        services.AddHttpClient<PythonMlHealthClient>(client =>
        {
            client.BaseAddress = normalizedBaseUrl;
            client.Timeout = TimeSpan.FromSeconds(timeout);
        });

        // Python ML prediction client.
        services.AddHttpClient<PythonMlPredictionClient>(client =>
        {
            client.BaseAddress = normalizedBaseUrl;
            client.Timeout = TimeSpan.FromSeconds(timeout);
        });

        // Select the prediction provider.
        var provider = section["Provider"] ?? "Mock";

        switch (provider.Trim().ToLowerInvariant())
        {
            case "mock":
                services.AddScoped<
                    IRiskPredictionService,
                    MockRiskPredictionService>();
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported ML provider '{provider}'. " +
                    "Only 'Mock' is currently available for " +
                    "the existing IRiskPredictionService contract.");
        }

        return services;
    }
}
