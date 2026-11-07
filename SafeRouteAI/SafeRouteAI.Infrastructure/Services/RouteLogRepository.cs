using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Domain.Entities;
using SafeRouteAI.Infrastructure.Data;

namespace SafeRouteAI.Infrastructure.Services;

public class RouteLogRepository(AppDbContext db) : IRouteLogRepository
{
    private static readonly JsonSerializerOptions Json = new() { Converters = { new JsonStringEnumConverter() } };

    public async Task SaveAsync(RouteLogEntry entry, CancellationToken ct = default)
    {
        var route = new Route
        {
            UserID = entry.UserId,   // null for anonymous requests
            Origin = Format(entry.Origin),
            Destination = Format(entry.Destination),
            RequestDateTime = entry.RequestedAt
        };

        foreach (var r in entry.Routes)
            db.Set<RouteOption>().Add(new RouteOption
            {
                Route = route,
                RouteInfo = r.Route.RouteId,
                Geometry = r.Route.Geometry,
                Distance = r.Route.DistanceKm,
                EstTimeMin = r.Route.DurationMinutes,
                RiskScore = r.Risk.Score,
                Rank = r.Rank
            });

        db.Set<RouteRiskLog>().Add(new RouteRiskLog
        {
            Route = route,
            LoggedAt = DateTime.UtcNow,
            ResultSnapshot = JsonSerializer.Serialize(entry.Routes, Json)
        });

        await db.SaveChangesAsync(ct);
    }

    private static string Format(Coordinate c) =>
        string.Create(CultureInfo.InvariantCulture, $"{c.Lat},{c.Lon}");
}