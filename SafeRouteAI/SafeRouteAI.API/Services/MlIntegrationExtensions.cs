
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

        var baseUrl = section["BaseUrl"]
            ?? "http://127.0.0.1:8001/";

        var timeoutSeconds = section.GetValue<int?>(
            "TimeoutSeconds") ?? 90;

        services.AddHttpClient<PythonRoutePredictionClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });

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
                    client.BaseAddress = new Uri(baseUrl);
                    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                });
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported ML provider '{provider}'. " +
                    "Supported providers are 'Mock' and 'Python'.");
        }

        return services;
    }
}
