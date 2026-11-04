using System.Text.Json;
using SafeRouteAI.Application.DTOs;

namespace SafeRouteAI.Application.Services;

public record SegmentDto(Coordinate Start, Coordinate End, double LengthM);

public static class SegmentSplitter
{
    public static List<SegmentDto> Split(string geoJsonLineString, double maxLengthM = 500)
    {
        using var doc = JsonDocument.Parse(geoJsonLineString);
        var points = doc.RootElement.GetProperty("coordinates").EnumerateArray()
            .Select(p => new Coordinate(p[1].GetDouble(), p[0].GetDouble())) // GeoJSON is [lon, lat]
            .ToList();

        var segments = new List<SegmentDto>();
        for (var i = 0; i < points.Count - 1; i++)
        {
            var a = points[i];
            var b = points[i + 1];
            var length = Distance(a, b);
            var parts = Math.Max(1, (int)Math.Ceiling(length / maxLengthM));

            var prev = a;
            for (var p = 1; p <= parts; p++)
            {
                var t = (double)p / parts;
                var next = new Coordinate(a.Lat + (b.Lat - a.Lat) * t, a.Lon + (b.Lon - a.Lon) * t);
                segments.Add(new SegmentDto(prev, next, length / parts));
                prev = next;
            }
        }
        return segments;
    }

    // Haversine distance in metres
    private static double Distance(Coordinate a, Coordinate b)
    {
        const double R = 6371000;
        var dLat = Rad(b.Lat - a.Lat);
        var dLon = Rad(b.Lon - a.Lon);
        var h = Math.Pow(Math.Sin(dLat / 2), 2)
              + Math.Cos(Rad(a.Lat)) * Math.Cos(Rad(b.Lat)) * Math.Pow(Math.Sin(dLon / 2), 2);
        return 2 * R * Math.Asin(Math.Sqrt(h));
    }

    private static double Rad(double deg) => deg * Math.PI / 180;
}