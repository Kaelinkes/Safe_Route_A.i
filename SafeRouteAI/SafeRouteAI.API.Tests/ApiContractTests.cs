
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

    public ApiContractTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsRunning()
    {
        var response = await _client.GetAsync("/api/v1/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Running", body,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RiskPrediction_ReturnsMockPrediction()
    {
        var request = new
        {
            segmentId = "segment-001",
            features = new
            {
                temperature = 20,
                humidity = 70
            }
        };

        var response = await _client.PostAsJsonAsync(
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
    public async Task Routes_ValidCoordinates_Returns503()
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
                latitude = -25.7479,
                longitude = 28.2293
            }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/routes/compare", request);

        Assert.Equal(
            HttpStatusCode.ServiceUnavailable,
            response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            503,
            json.RootElement.GetProperty("status").GetInt32());

        Assert.Equal(
            "Routing service unavailable",
            json.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task RiskPrediction_EmptySegmentId_Returns400()
    {
        var request = new
        {
            segmentId = "",
            features = new
            {
                temperature = 20
            }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/risk/predict", request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task Routes_IdenticalCoordinates_Returns400()
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
                latitude = -26.2041,
                longitude = 28.0473
            }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/routes/compare", request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task Routes_MissingLatitude_Returns400()
    {
        var request = new
        {
            origin = new
            {
                longitude = 28.0473
            },
            destination = new
            {
                latitude = -25.7479,
                longitude = 28.2293
            }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/routes/compare", request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task RiskPrediction_EmptyFeatures_Returns400()
    {
        var request = new
        {
            segmentId = "segment-001",
            features = new { }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/risk/predict", request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}
