
using System.Text.Json;
using SafeRouteAI.API.DTOs;

namespace SafeRouteAI.API.Services;

public sealed class RouteRiskPredictionAdapter
{
    private readonly PythonMlPredictionClient _python;

    public RouteRiskPredictionAdapter(
        PythonMlPredictionClient python)
    {
        _python = python;
    }

    public async Task<PythonMlPredictionResult> PredictAsync(
        RouteRiskPredictionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.RouteId))
        {
            throw new ArgumentException(
                "RouteId is required.");
        }

        if (request.Segments.ValueKind != JsonValueKind.Array ||
            request.Segments.GetArrayLength() == 0)
        {
            throw new ArgumentException(
                "At least one road segment is required.");
        }

        foreach (var segment in request.Segments.EnumerateArray())
        {
            if (segment.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException(
                    "Each segment must be a JSON object.");
            }

            string[] requiredFields =
            [
                "latitude",
                "longitude",
                "arrival_time",
                "road_type",
                "num_lanes",
                "curvature",
                "speed_limit_kmh",
                "road_signs_present",
                "public_road",
                "time_of_day",
                "holiday",
                "school_season",
                "num_reported_accidents"
            ];

            foreach (var field in requiredFields)
            {
                if (!segment.TryGetProperty(field, out var value) ||
                    value.ValueKind == JsonValueKind.Null ||
                    value.ValueKind == JsonValueKind.Undefined)
                {
                    throw new ArgumentException(
                        $"Missing required segment field: {field}");
                }
            }
        }

        var pythonRequest = JsonSerializer.SerializeToElement(
            new
            {
                segments = request.Segments
            });

        var result = await _python.PredictRouteAsync(
            pythonRequest,
            cancellationToken);

        if (result.StatusCode < 200 || result.StatusCode >= 300)
        {
            return result;
        }

        var response = JsonSerializer.SerializeToElement(
            new
            {
                routeId = request.RouteId,
                prediction = result.Payload
            });

        return new PythonMlPredictionResult(
            result.StatusCode,
            response);
    }
}
