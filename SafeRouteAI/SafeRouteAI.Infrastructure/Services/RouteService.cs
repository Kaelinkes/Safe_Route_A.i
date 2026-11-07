
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Exceptions;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Application.Validation;

namespace SafeRouteAI.Infrastructure.Services;

public class RouteService(HttpClient http, IConfiguration configuration) : IRouteService
{
    private const string Url =
        "https://api.openrouteservice.org/v2/directions/driving-car/geojson";

    private readonly string _apiKey =
        configuration["OpenRouteService:ApiKey"] ?? "";

    public async Task<List<RouteOptionDto>> GetRoutesAsync(
        Coordinate start,
        Coordinate destination,
        CancellationToken cancellationToken = default)
    {
        if (!Validators.IsValid(start) ||
            !Validators.IsValid(destination))
        {
            throw new ArgumentException("Invalid coordinates.");
        }

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException(
                "OpenRouteService API key is missing.");
        }

        var coordinates = new[]
        {
            new[] { start.Lon, start.Lat },
            new[] { destination.Lon, destination.Lat }
        };

        // First attempt: request up to three alternative routes.
        var response = await SendAsync(
            new
            {
                coordinates,
                alternative_routes = new
                {
                    target_count = 3
                }
            },
            cancellationToken);

        // If OpenRouteService rejects alternative routing,
        // retry once using a standard route request.
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await response.Content.ReadAsStringAsync(
                cancellationToken);

            if (error.Contains(
                    "alternative",
                    StringComparison.OrdinalIgnoreCase))
            {
                response.Dispose();

                response = await SendAsync(
                    new { coordinates },
                    cancellationToken);
            }
        }
        // OpenRouteService may return HTTP 502 when processing
        // alternative routes. Retry once without alternatives.
        else if (response.StatusCode == HttpStatusCode.BadGateway)
        {
            response.Dispose();

            response = await SendAsync(
                new { coordinates },
                cancellationToken);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                var body = await response.Content.ReadAsStringAsync(
                    cancellationToken);

                if (IsNoRouteError(body))
                {
                    throw new RouteNotFoundException(
                        "No road was found near one of the points. " +
                        "Try a location closer to a road.");
                }
            }

            response.EnsureSuccessStatusCode();

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            using var document = JsonDocument.Parse(responseBody);

            var routes = new List<RouteOptionDto>();

            var index = 1;

            foreach (var feature in document.RootElement
                         .GetProperty("features")
                         .EnumerateArray())
            {
                var summary = feature
                    .GetProperty("properties")
                    .GetProperty("summary");

                var distanceKm = Math.Round(
                    summary.GetProperty("distance").GetDouble() / 1000,
                    2);

                var durationMinutes = Math.Round(
                    summary.GetProperty("duration").GetDouble() / 60,
                    1);

                var geometry = feature
                    .GetProperty("geometry")
                    .GetRawText();

                routes.Add(new RouteOptionDto(
                    $"route-{index++}",
                    distanceKm,
                    durationMinutes,
                    geometry));
            }

            return routes;
        }
    }

    private async Task<HttpResponseMessage> SendAsync(
        object body,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            Url)
        {
            Content = JsonContent.Create(body)
        };

        request.Headers.TryAddWithoutValidation(
            "Authorization",
            _apiKey);

        return await http.SendAsync(
            request,
            cancellationToken);
    }

    private static bool IsNoRouteError(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);

            return document.RootElement.TryGetProperty(
                       "error",
                       out var error)
                   && error.ValueKind == JsonValueKind.Object
                   && error.TryGetProperty(
                       "code",
                       out var code)
                   && code.TryGetInt32(out var value)
                   && value is 2009 or 2010;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
