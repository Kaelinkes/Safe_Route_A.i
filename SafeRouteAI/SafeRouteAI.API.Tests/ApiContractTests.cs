
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SafeRouteAI.API.Tests;

public class ApiContractTests :
    IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiContractTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsRunningStatus()
    {
        var response = await _client.GetAsync(
            "/api/v1/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            "Running",
            json.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task RiskPrediction_ReturnsMockResponse()
    {
        var request = new
        {
            segmentId = "segment-001",
            features = new
            {
                trafficVolume = 750,
                rainfallMm = 12,
                speedLimitKmh = 60
            }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/risk/predict", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            "segment-001",
            json.RootElement.GetProperty("segmentId").GetString());

        Assert.Equal(
            "InsufficientData",
            json.RootElement.GetProperty("riskLevel").GetString());

        Assert.True(
            json.RootElement.GetProperty("isMock").GetBoolean());
    }

    [Fact]
    public async Task RouteComparison_ReturnsTwoMockRoutes()
    {
        var request = new
        {
            origin = new
            {
                latitude = -26.2041,
                longitude = 28.0473
            },
            destination = new
            {
                latitude = -26.1367,
                longitude = 28.1976
            }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/routes/compare", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var routes = json.RootElement.GetProperty("routes");

        Assert.Equal(2, routes.GetArrayLength());

        Assert.True(
            json.RootElement.GetProperty("isMock").GetBoolean());
    }

    [Fact]
    public async Task RiskPrediction_EmptySegment_ReturnsBadRequest()
    {
        var request = new
        {
            segmentId = "",
            features = new
            {
                trafficVolume = 750
            }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/risk/predict", request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}
