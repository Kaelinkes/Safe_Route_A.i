
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace SafeRouteAI.API.Tests;

public class WeatherApiTests
{
    [Fact]
    public async Task Weather_ValidCoordinates_ReturnsWeatherData()
    {
        using var factory = CreateFactory(
            HttpStatusCode.OK,
            """
            {
              "current": {
                "time": "2026-11-03T18:00",
                "temperature_2m": 17.8,
                "relative_humidity_2m": 81,
                "precipitation": 0.1,
                "visibility": 9440,
                "wind_speed_10m": 10.2,
                "weather_code": 51
              }
            }
            """);

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/v1/weather?latitude=-26.2041&longitude=28.0473");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"isAvailable\":true", body);
        Assert.Contains("\"temperatureCelsius\":17.8", body);
        Assert.Contains("\"humidityPercent\":81", body);
        Assert.Contains("\"weatherCode\":51", body);
    }

    [Theory]
    [InlineData(91, 28)]
    [InlineData(-91, 28)]
    [InlineData(-26, 181)]
    [InlineData(-26, -181)]
    public async Task Weather_InvalidCoordinates_ReturnsBadRequest(
        double latitude,
        double longitude)
    {
        using var factory = CreateFactory(
            HttpStatusCode.OK, "{}");

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/v1/weather?latitude={latitude}&longitude={longitude}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Weather_ProviderUnavailable_Returns503()
    {
        using var factory = CreateFactory(
            HttpStatusCode.ServiceUnavailable, "{}");

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/v1/weather?latitude=-26.2041&longitude=28.0473");

        Assert.Equal(
            HttpStatusCode.ServiceUnavailable,
            response.StatusCode);
    }

    [Fact]
    public async Task Weather_MalformedProviderResponse_Returns503()
    {
        using var factory = CreateFactory(
            HttpStatusCode.OK, "{ invalid json }");

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/v1/weather?latitude=-26.2041&longitude=28.0473");

        Assert.Equal(
            HttpStatusCode.ServiceUnavailable,
            response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory(
        HttpStatusCode status,
        string json)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IHttpClientFactory>();

                    services.AddHttpClient("OpenMeteo")
                        .ConfigurePrimaryHttpMessageHandler(
                            () => new FakeWeatherHandler(status, json));
                });
            });
    }

    private sealed class FakeWeatherHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _json;

        public FakeWeatherHandler(
            HttpStatusCode status,
            string json)
        {
            _status = status;
            _json = json;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(
                    _json,
                    Encoding.UTF8,
                    "application/json")
            });
        }
    }
}
