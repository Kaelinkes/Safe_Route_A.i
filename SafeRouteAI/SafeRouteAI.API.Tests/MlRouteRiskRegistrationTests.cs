
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafeRouteAI.API.Services;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Infrastructure.Services;
using Xunit;

namespace SafeRouteAI.API.Tests;

public class MlRouteRiskRegistrationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("Fake")]
    public void FakeProvider_UsesBackendRiskService(
        string? riskProvider)
    {
        var services = CreateServices(riskProvider);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var riskService =
            scope.ServiceProvider.GetRequiredService<IRiskService>();

        Assert.IsType<FakeRiskService>(riskService);
    }

    [Fact]
    public void ApiBridgeProvider_UsesApiRiskServiceBridge()
    {
        var services = CreateServices("ApiBridge");

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var riskService =
            scope.ServiceProvider.GetRequiredService<IRiskService>();

        Assert.IsType<ApiRiskServiceBridge>(riskService);
    }

    [Fact]
    public void UnsupportedRiskProvider_IsRejected()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => CreateServices("Unknown"));

        Assert.Contains(
            "Unsupported route risk provider",
            exception.Message);
    }

    private static IServiceCollection CreateServices(
        string? riskProvider)
    {
        var settings = new Dictionary<string, string?>
        {
            ["RoutingIntegration:Provider"] = "Backend"
        };

        if (riskProvider is not null)
        {
            settings["MlIntegration:RouteRiskProvider"] =
                riskProvider;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);

        // Register the existing mock prediction provider.
        // The ApiBridge can then be resolved without a real ML model.
        services.AddMlIntegration(configuration);

        services.AddRoutingIntegration(configuration);

        return services;
    }
}

