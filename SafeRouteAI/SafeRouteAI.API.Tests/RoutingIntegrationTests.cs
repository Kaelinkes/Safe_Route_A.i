
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafeRouteAI.API.Interfaces;
using SafeRouteAI.API.Services;
using Xunit;

namespace SafeRouteAI.API.Tests;

public class RoutingIntegrationTests
{
    [Theory]
    [InlineData("Unavailable")]
    [InlineData("Backend")]
    public async Task RoutingProviders_Return503UntilConnected(
        string provider)
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    (_, configuration) =>
                    {
                        configuration.AddInMemoryCollection(
                            new Dictionary<string, string?>
                            {
                                ["RoutingIntegration:Provider"] = provider
                            });
                    });
            });

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/routes/compare",
            new
            {
                origin = new
                {
                    latitude = -26.2041,
                    longitude = 28.0473
                },
                destination = new
                {
                    latitude = -25.7479,
                    longitude = 28.2293
                }
            });

        Assert.Equal(
            HttpStatusCode.ServiceUnavailable,
            response.StatusCode);
    }

    [Fact]
    public void BackendProvider_RegistersBackendAdapter()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["RoutingIntegration:Provider"] = "Backend"
                })
            .Build();

        var services = new ServiceCollection();

        services.AddRoutingIntegration(configuration);

        using var provider = services.BuildServiceProvider();

        var routingService =
            provider.GetRequiredService<IRouteComparisonService>();

        Assert.IsType<BackendRouteComparisonService>(
            routingService);
    }

    [Fact]
    public void UnsupportedRoutingProvider_IsRejected()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["RoutingIntegration:Provider"] = "Unknown"
                })
            .Build();

        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(
            () => services.AddRoutingIntegration(configuration));

        Assert.Contains(
            "Unsupported routing provider",
            exception.Message);
    }
}
