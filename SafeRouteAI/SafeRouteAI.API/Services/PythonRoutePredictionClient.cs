
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SafeRouteAI.API.Services;

public sealed class PythonRoutePredictionClient
{
    private readonly HttpClient _http;

    public PythonRoutePredictionClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<PythonRoutePredictionResponse> PredictRouteAsync(
        string geometry,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(geometry);

        var arrivalTime = DateTimeOffset.Now;
        var segments = ReadCoordinates(geometry, arrivalTime);

        if (segments.Count == 0)
        {
            throw new InvalidOperationException(
                "The route geometry contains no valid coordinates.");
        }

        var response = await _http.PostAsJsonAsync(
            "predict-route",
            new { segments },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(
                cancellationToken);

            throw new HttpRequestException(
                $"Python prediction failed: {(int)response.StatusCode} " +
                $"{response.ReasonPhrase}. Details: {error}");
        }

        var result =
            await response.Content.ReadFromJsonAsync<PythonRoutePredictionResponse>(
                cancellationToken: cancellationToken);

        return result ?? throw new InvalidOperationException(
            "Python returned an empty prediction response.");
    }

    private static List<PythonRouteSegment> ReadCoordinates(
        string geometry,
        DateTimeOffset arrivalTime)
    {
        using var document = JsonDocument.Parse(geometry);

        var coordinates = document.RootElement
            .GetProperty("coordinates");

        if (coordinates.ValueKind != JsonValueKind.Array ||
            coordinates.GetArrayLength() == 0)
        {
            return [];
        }

        var points = coordinates
            .EnumerateArray()
            .Select(point =>
            {
                if (point.ValueKind != JsonValueKind.Array ||
                    point.GetArrayLength() < 2)
                {
                    throw new InvalidOperationException(
                        "Invalid route coordinate.");
                }

                var longitude = point[0].GetDouble();
                var latitude = point[1].GetDouble();

                if (!double.IsFinite(latitude) ||
                    !double.IsFinite(longitude) ||
                    latitude < -90 || latitude > 90 ||
                    longitude < -180 || longitude > 180)
                {
                    throw new InvalidOperationException(
                        "Route coordinate is outside valid WGS84 ranges.");
                }

                return new PythonRouteSegment
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    ArrivalTime = arrivalTime
                };
            })
            .ToList();

        if (points.Count <= 100)
            return points;

        return Enumerable.Range(0, 100)
            .Select(i => points[
                (int)Math.Round(
                    i * (points.Count - 1) / 99.0)])
            .ToList();
    }
}

public sealed class PythonRouteSegment
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("arrival_time")]
    public DateTimeOffset ArrivalTime { get; set; }
}

public sealed class PythonRoutePredictionResponse
{
    [JsonPropertyName("results")]
    public List<PythonSegmentPrediction> Results { get; set; } = [];

    [JsonPropertyName("weatherProvider")]
    public string? WeatherProvider { get; set; }

    [JsonPropertyName("routePointsProcessed")]
    public int RoutePointsProcessed { get; set; }
}

public sealed class PythonSegmentPrediction
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("riskScore")]
    public double? RiskScore { get; set; }

    [JsonPropertyName("riskLevel")]
    public string? RiskLevel { get; set; }

    [JsonPropertyName("missingFields")]
    public List<string> MissingFields { get; set; } = [];

    [JsonPropertyName("invalidFields")]
    public List<string> InvalidFields { get; set; } = [];
}
