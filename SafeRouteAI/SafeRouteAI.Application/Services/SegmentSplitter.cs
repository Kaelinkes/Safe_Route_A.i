using System.Text.Json;
using SafeRouteAI.Application.DTOs;

namespace SafeRouteAI.Application.Services;

public record SegmentDto(Coordinate Start, Coordinate End, double LengthM);

public static class SegmentSplitter
{
    public static List<SegmentDto> Split(string geoJsonLineString, double maxLengthM = 500)
    {
        if (!double.IsFinite(maxLengthM) || maxLengthM <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxLengthM),
                "Maximum segment length must be positive.");
        }

        using var document = JsonDocument.Parse(geoJsonLineString);

        var points = document.RootElement.GetProperty("coordinates")
            .EnumerateArray()
            .Select(point => new Coordinate(
                point[1].GetDouble(),
                point[0].GetDouble()))
            .ToList();

        var segments = new List<SegmentDto>();

        for (var index = 0; index < points.Count - 1; index++)
        {
            var start = points[index];
            var end = points[index + 1];

            var length = Distance(start, end);
            var partCount = Math.Max(1, (int)Math.Ceiling(length / maxLengthM));

            var previous = start;

            for (var part = 1; part <= partCount; part++)
            {
                var fraction = (double)part / partCount;

                var next = new Coordinate(
                    start.Lat + (end.Lat - start.Lat) * fraction,
                    start.Lon + (end.Lon - start.Lon) * fraction);

                segments.Add(new SegmentDto(previous, next, length / partCount));
                previous = next;
            }
        }

        return segments;
    }

    private static double Distance(Coordinate start, Coordinate end)
    {
        const double earthRadiusMeters = 6_371_000;

        var latitudeDelta = ToRadians(end.Lat - start.Lat);
        var longitudeDelta = ToRadians(end.Lon - start.Lon);

        var haversine =
            Math.Pow(Math.Sin(latitudeDelta / 2), 2) +
            Math.Cos(ToRadians(start.Lat)) *
            Math.Cos(ToRadians(end.Lat)) *
            Math.Pow(Math.Sin(longitudeDelta / 2), 2);

        return 2 * earthRadiusMeters * Math.Asin(Math.Sqrt(haversine));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}