
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

public class RoutingErrorHandlingTests
{
    [Theory]
    [InlineData(HttpStatusCode.NotFound, 422,
        "Route could not be calculated")]
    [InlineData(HttpStatusCode.BadRequest, 422,
        "Route could not be calculated")]
    [InlineData(HttpStatusCode.TooManyRequests, 503,
        "Routing provider temporarily unavailable")]
    [InlineData(HttpStatusCode.InternalServerError, 502,
        "Routing provider error")]
    [InlineData(HttpStatusCode.Forbidden, 502,
        "Routing provider error")]
    public async Task ProviderHttpFailure_ReturnsExpectedProblem(
        HttpStatusCode providerStatus,
        int expectedStatus,
        string expectedTitle)
    {
        await AssertRoutingErrorAsync(
            new HttpRequestException(
                "Sensitive upstream error",
                null,
                providerStatus),
            expectedStatus,
            expectedTitle);
    }

    [Fact]
    public async Task ProviderConnectionFailure_Returns502()
    {
        await AssertRoutingErrorAsync(
            new HttpRequestException(
                "Sensitive network error"),
            502,
            "Routing provider error");
    }

    private static async Task AssertRoutingErrorAsync(
        Exception exception,
        int expectedStatus,
        string expectedTitle)
    {
        await using var factory =
            new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddScoped<IRouteComparisonService>(
                            _ => new ThrowingRouteService(exception));
                    });
                });

        using var client = factory.CreateClient();

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

        using var response = await client.PostAsJsonAsync(
            "/api/v1/routes/compare",
            request);

        Assert.Equal(
            (HttpStatusCode)expectedStatus,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            expectedStatus,
            json.RootElement.GetProperty("status").GetInt32());

        Assert.Equal(
            expectedTitle,
            json.RootElement.GetProperty("title").GetString());

        var responseBody = json.RootElement.GetRawText();

        Assert.DoesNotContain(
            "Sensitive upstream error",
            responseBody);

        Assert.DoesNotContain(
            "Sensitive network error",
            responseBody);
    }

    private sealed class ThrowingRouteService
        : IRouteComparisonService
    {
        private readonly Exception _exception;

        public ThrowingRouteService(Exception exception)
        {
            _exception = exception;
        }

        public Task<RouteComparisonResponse> CompareAsync(
            RouteComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<RouteComparisonResponse>(
                _exception);
        }
    }
}
