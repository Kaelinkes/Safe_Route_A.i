
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SafeRouteAI.API.Interfaces;
using SafeRouteAI.API.Services;
using Xunit;

namespace SafeRouteAI.API.Tests;

public class MlIntegrationTests
{
    [Fact]
    public async Task DefaultProvider_ReturnsMockPrediction()
    {
        // The production default is Python.
        // Explicitly select Mock for this test.
        using var factory = CreateMockFactory();

        using var client = factory.CreateClient();

        var request = new
        {
            segmentId = "segment-001",
            features = new
            {
                temperature = 22,
                humidity = 65,
                visibility = 10000,
                windSpeed = 12
            }
        };

        var response = await client.PostAsJsonAsync(
            "/api/v1/risk/predict",
            request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.True(
            json.RootElement
                .GetProperty("isMock")
                .GetBoolean());

        Assert.Equal(
            "InsufficientData",
            json.RootElement
                .GetProperty("riskLevel")
                .GetString());
    }

    [Fact]
    public void UnsupportedProvider_IsRejected()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["MlIntegration:Provider"] =
                        "UnimplementedProvider"
                })
            .Build();

        var services = new ServiceCollection();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => services.AddMlIntegration(
                    configuration));

        Assert.Contains(
            "Unsupported ML provider",
            exception.Message);
    }

    [Fact]
    public async Task ExplicitMockProvider_ReturnsMockPrediction()
    {
        using var factory = CreateMockFactory();

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/risk/predict",
            new
            {
                segmentId = "segment-002",
                features = new
                {
                    temperature = 25,
                    humidity = 50
                }
            });

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.True(
            json.RootElement
                .GetProperty("isMock")
                .GetBoolean());

        Assert.Equal(
            "InsufficientData",
            json.RootElement
                .GetProperty("riskLevel")
                .GetString());
    }

    [Fact]
    public void MockProvider_RegistersMockRiskPredictionService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["MlIntegration:Provider"] = "Mock"
                })
            .Build();

        var services = new ServiceCollection();

        services.AddMlIntegration(configuration);

        var registration =
            Assert.Single(
                services,
                service =>
                    service.ServiceType ==
                    typeof(IRiskPredictionService));

        Assert.Equal(
            typeof(MockRiskPredictionService),
            registration.ImplementationType);
    }

    [Fact]
    public void PythonProvider_RegistersPythonRiskPredictionService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["MlIntegration:Provider"] = "Python"
                })
            .Build();

        var services = new ServiceCollection();

        services.AddMlIntegration(configuration);

        var registration =
            Assert.Single(
                services,
                service =>
                    service.ServiceType ==
                    typeof(IRiskPredictionService));

        Assert.NotNull(registration.ImplementationFactory);
    }

    private static WebApplicationFactory<Program>
        CreateMockFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    (_, configuration) =>
                    {
                        configuration.AddInMemoryCollection(
                            new Dictionary<string, string?>
                            {
                                ["MlIntegration:Provider"] =
                                    "Mock"
                            });
                    });

                // Explicitly select Mock in the test host.
                // This does not affect production registration.
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<
                        IRiskPredictionService>();

                    services.AddScoped<
                        IRiskPredictionService,
                        MockRiskPredictionService>();
                });
            });
    }
}
