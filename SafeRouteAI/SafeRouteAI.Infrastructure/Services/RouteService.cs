using Microsoft.Extensions.Configuration;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Application.Validation;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SafeRouteAI.Application.Exceptions;

namespace SafeRouteAI.Infrastructure.Services;

public class RouteService : IRouteService
{
    private const string Url = "https://api.openrouteservice.org/v2/directions/driving-car/geojson";
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public RouteService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["OpenRouteService:ApiKey"] ?? "";
    }

    public async Task<List<RouteOptionDto>> GetRoutesAsync(
        Coordinate start, Coordinate destination, CancellationToken cancellationToken = default)
    {
        if (!Validators.IsValid(start) || !Validators.IsValid(destination))
            throw new ArgumentException("Invalid coordinates.");
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("OpenRouteService API key is missing.");

        // OpenRouteService expects [longitude, latitude]
        var coords = new[] { new[] { start.Lon, start.Lat }, new[] { destination.Lon, destination.Lat } };

        // Ask for alternatives first; ORS rejects this for long trips, so retry with one route
        var response = await SendAsync(
    new { coordinates = coords, alternative_routes = new { target_count = 3 } }, cancellationToken);

        // Only retry when ORS rejects the alternatives option (400 mentioning "alternative").
        // Anything else (bad key, rate limit, server error) falls through and throws.
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            if (error.Contains("alternative", StringComparison.OrdinalIgnoreCase))
            {
                response.Dispose();
                response = await SendAsync(new { coordinates = coords }, cancellationToken);
            }
        }
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (IsNoRouteError(body))
            {
                response.Dispose();
                throw new RouteNotFoundException(
                    "No road was found near one of the points. Try a location closer to a road.");
            }
        }
        using (response)
        {
            response.EnsureSuccessStatusCode();
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));

            var routes = new List<RouteOptionDto>();
            var i = 1;
            foreach (var feature in doc.RootElement.GetProperty("features").EnumerateArray())
            {
                var summary = feature.GetProperty("properties").GetProperty("summary");
                routes.Add(new RouteOptionDto(
                    $"route-{i++}",
                    Math.Round(summary.GetProperty("distance").GetDouble() / 1000, 2),  // metres to km
                    Math.Round(summary.GetProperty("duration").GetDouble() / 60, 1),    // seconds to minutes
                    feature.GetProperty("geometry").GetRawText()));                      // GeoJSON LineString
            }
            return routes;
        }
    }

    private Task<HttpResponseMessage> SendAsync(object body, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, Url) { Content = JsonContent.Create(body) };
        request.Headers.TryAddWithoutValidation("Authorization", _apiKey);
        return _http.SendAsync(request, ct);
    }

    // ORS 2010 = no routable point near a coordinate, 2009 = no route between the points
    private static bool IsNoRouteError(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.Object
                && error.TryGetProperty("code", out var code)
                && code.TryGetInt32(out var c)
                && c is 2009 or 2010;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}