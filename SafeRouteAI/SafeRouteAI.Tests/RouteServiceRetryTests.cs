using System.Net;
using Microsoft.Extensions.Configuration;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Infrastructure.Services;

public class RouteServiceRetryTests
{
    private const string Ok = """
    {"features":[{"properties":{"summary":{"distance":1000.0,"duration":60.0}},
    "geometry":{"type":"LineString","coordinates":[[28.04,-26.2],[28.05,-26.21]]}}]}
    """;

    private class SequenceHandler(params (HttpStatusCode Status, string Body)[] responses) : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken c)
        {
            var (status, body) = responses[Math.Min(Calls++, responses.Length - 1)];
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
        }
    }

    private static RouteService Create(HttpMessageHandler handler) => new(
        new HttpClient(handler),
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["OpenRouteService:ApiKey"] = "test" })
            .Build());

    private static readonly Coordinate A = new(-26.2, 28.04);
    private static readonly Coordinate B = new(-26.21, 28.05);

    [Fact]
    public async Task Alternatives_error_retries_with_a_single_route()
    {
        var handler = new SequenceHandler(
            (HttpStatusCode.BadRequest, "alternative routes exceed the distance limit"),
            (HttpStatusCode.OK, Ok));

        var routes = await Create(handler).GetRoutesAsync(A, B);

        Assert.Single(routes);
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task Other_errors_throw_without_retrying()
    {
        var handler = new SequenceHandler((HttpStatusCode.Unauthorized, "invalid key"));

        await Assert.ThrowsAsync<HttpRequestException>(() => Create(handler).GetRoutesAsync(A, B));

        Assert.Equal(1, handler.Calls);
    }
}