
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Exceptions;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Application.Validation;

namespace SafeRouteAI.Infrastructure.Services;

public class RouteService : IRouteService
{
    private const string Url =
        "https://api.openrouteservice.org/v2/directions/driving-car/geojson";

    private static readonly string[] ExtraInfo =
    [
        "waytype",
        "surface",
        "waycategory",
        "steepness",
        "tollways"
    ];

    private readonly HttpClient _http;
    private readonly string _apiKey;

    public RouteService(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _apiKey = configuration["OpenRouteService:ApiKey"] ?? "";
    }

    public async Task<List<RouteOptionDto>> GetRoutesAsync(
        Coordinate start,
        Coordinate destination,
        CancellationToken cancellationToken = default)
    {
        if (!Validators.IsValid(start) || !Validators.IsValid(destination))
            throw new ArgumentException("Invalid coordinates.");

        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("OpenRouteService API key is missing.");

        var coordinates = new[]
        {
            new[] { start.Lon, start.Lat },
            new[] { destination.Lon, destination.Lat }
        };

        var response = await SendAsync(
            new
            {
                coordinates,
                alternative_routes = new { target_count = 3 },
                extra_info = ExtraInfo
            },
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);

            if (error.Contains("alternative", StringComparison.OrdinalIgnoreCase))
            {
                response.Dispose();

                response = await SendAsync(
                    new { coordinates, extra_info = ExtraInfo },
                    cancellationToken);
            }
        }
        else if (response.StatusCode == HttpStatusCode.BadGateway)
        {
            response.Dispose();

            response = await SendAsync(
                new { coordinates, extra_info = ExtraInfo },
                cancellationToken);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (IsNoRouteError(body))
                {
                    throw new RouteNotFoundException(
                        "No road was found near one of the points. Try a location closer to a road.");
                }
            }

            response.EnsureSuccessStatusCode();

            var responseBody =
                await response.Content.ReadAsStringAsync(cancellationToken);
Console.WriteLine("===== ORS RAW RESPONSE =====");
Console.WriteLine(responseBody);


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
                    summary.GetProperty("distance").GetDouble() / 1000, 2);

                var durationMinutes = Math.Round(
                    summary.GetProperty("duration").GetDouble() / 60, 1);

                var geometry = feature
                    .GetProperty("geometry")
                    .GetRawText();

                routes.Add(new RouteOptionDto(
                    $"route-{index++}",
                    distanceKm,
                    durationMinutes,
                    geometry)
                {
                    RoadSegments = ReadRoadSegments(feature)
                });
            }

            return routes;
        }
    }

    private static List<RoadSegmentAttributeDto> ReadRoadSegments(
        JsonElement feature)
    {
        var segments = new List<RoadSegmentAttributeDto>();

        if (!feature.TryGetProperty("properties", out var properties) ||
            !properties.TryGetProperty("extras", out var extras))
        {
            return segments;
        }

        foreach (var attribute in extras.EnumerateObject())
        {
            if (!attribute.Value.TryGetProperty("values", out var values) ||
                values.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var attributeName = attribute.Name == "waytypes"
                ? "waytype"
                : attribute.Name;

            foreach (var range in values.EnumerateArray())
            {
                if (range.ValueKind != JsonValueKind.Array ||
                    range.GetArrayLength() < 3)
                {
                    continue;
                }

                var fromIndex = range[0].GetInt32();
                var toIndex = range[1].GetInt32();
                var value = range[2].ToString();

                segments.Add(new RoadSegmentAttributeDto(
                    fromIndex,
                    toIndex,
                    attributeName,
                    value));
            }
        }

        return segments;
    }

    private async Task<HttpResponseMessage> SendAsync(
        object body,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, Url)
        {
            Content = JsonContent.Create(body)
        };

        request.Headers.TryAddWithoutValidation("Authorization", _apiKey);

        return await _http.SendAsync(request, cancellationToken);
    }

    private static bool IsNoRouteError(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);

            return document.RootElement.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.Object
                && error.TryGetProperty("code", out var code)
                && code.TryGetInt32(out var value)
                && value is 2009 or 2010;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
