
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using SafeRouteAI.API.DTOs;
using SafeRouteAI.API.Interfaces;
using Xunit;

namespace SafeRouteAI.API.Tests;

public class ErrorHandlingTests
{
    [Fact]
    public async Task UnexpectedException_ReturnsProblemDetails()
    {
        await using var factory =
            new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddScoped<
                            IRiskPredictionService,
                            FailingRiskPredictionService>();
                    });
                });

        using var client = factory.CreateClient();

        var request = new
        {
            segmentId = "segment-001",
            features = new
            {
                trafficVolume = 750
            }
        };

        var response = await client.PostAsJsonAsync(
            "/api/v1/risk/predict", request);

        Assert.Equal(
            HttpStatusCode.InternalServerError,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            500,
            json.RootElement.GetProperty("status").GetInt32());

        var responseBody = json.RootElement.GetRawText();

        Assert.DoesNotContain(
            "Sensitive internal error",
            responseBody);
    }

    private sealed class FailingRiskPredictionService
        : IRiskPredictionService
    {
        public Task<RiskPredictionResponse> PredictAsync(
            RiskPredictionRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Sensitive internal error");
        }
    }
}
