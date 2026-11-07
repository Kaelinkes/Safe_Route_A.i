
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
                    "Only 'Mock' is available until the ML " +
                    "integration is implemented.");
        }

        return services;
    }
}
