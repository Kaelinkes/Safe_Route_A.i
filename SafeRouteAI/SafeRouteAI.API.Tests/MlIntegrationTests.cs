
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafeRouteAI.API.Services;
using Xunit;

namespace SafeRouteAI.API.Tests;

public class MlIntegrationTests
{
    [Fact]
    public async Task DefaultProvider_ReturnsMockPrediction()
    {
        using var factory = new WebApplicationFactory<Program>();
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
            "/api/v1/risk/predict", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.True(
            json.RootElement.GetProperty("isMock").GetBoolean());

        Assert.Equal(
            "InsufficientData",
            json.RootElement.GetProperty("riskLevel").GetString());
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

        var exception = Assert.Throws<InvalidOperationException>(
            () => services.AddMlIntegration(configuration));

        Assert.Contains(
            "Unsupported ML provider",
            exception.Message);
    }

    [Fact]
    public async Task ExplicitMockProvider_ReturnsMockPrediction()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    (context, configuration) =>
                    {
                        configuration.AddInMemoryCollection(
                            new Dictionary<string, string?>
                            {
                                ["MlIntegration:Provider"] = "Mock"
                            });
                    });
            });

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

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.True(
            json.RootElement.GetProperty("isMock").GetBoolean());
    }
}
