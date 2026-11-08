using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeRouteAI.API.DTOs.Routes;
using SafeRouteAI.Infrastructure.Data;

namespace SafeRouteAI.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/routes/history")]
public sealed class RouteHistoryController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId) || userId <= 0)
        {
            return Unauthorized();
        }

        var routes = await db.Routes
            .AsNoTracking()
            .Where(route => route.UserID == userId)
            .OrderByDescending(route => route.RequestDateTime)
            .Take(100)
            .Select(route => new
            {
                route.RouteID,
                route.Origin,
                route.Destination,
                route.RequestDateTime
            })
            .ToListAsync(cancellationToken);

        if (routes.Count == 0)
        {
            return Ok(Array.Empty<RouteHistoryResponse>());
        }

        var routeIds = routes.Select(route => route.RouteID).ToArray();
        var options = await db.RouteOptions
            .AsNoTracking()
            .Where(option => routeIds.Contains(option.RouteID))
            .OrderBy(option => option.Rank)
            .Select(option => new
            {
                option.RouteID,
                option.Rank,
                DistanceKm = option.Distance,
                DurationMinutes = option.EstTimeMin,
                option.RiskScore,
                option.RouteOptionID
            })
            .ToListAsync(cancellationToken);

        var optionIds = options.Select(option => option.RouteOptionID).ToArray();
        var segmentRows = await db.RouteOptionRoadSegments
            .AsNoTracking()
            .Where(item => optionIds.Contains(item.RouteOptionID))
            .OrderBy(item => item.SegmentOrder)
            .Select(item => new
            {
                item.RouteOptionID,
                item.SegmentOrder,
                RoadName = item.RoadSegment == null
                    ? null
                    : item.RoadSegment.RoadName
            })
            .ToListAsync(cancellationToken);
        var segmentsByOption = segmentRows
            .GroupBy(item => item.RouteOptionID)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .OrderBy(item => item.SegmentOrder)
                    .Select(item => item.RoadName ?? "Unnamed road segment")
                    .ToArray());
        var optionsByRoute = options
            .GroupBy(option => option.RouteID)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<RouteHistoryOptionResponse>)group
                    .Select(option => new RouteHistoryOptionResponse(
                        option.Rank,
                        option.DistanceKm,
                        option.DurationMinutes,
                        option.RiskScore,
                        segmentsByOption.GetValueOrDefault(option.RouteOptionID, [])))
                    .ToArray());

        var logs = await db.RouteRiskLogs
            .AsNoTracking()
            .Where(log => routeIds.Contains(log.RouteID))
            .OrderByDescending(log => log.LoggedAt)
            .Select(log => new
            {
                log.RouteID,
                log.LoggedAt,
                log.ResultSnapshot
            })
            .ToListAsync(cancellationToken);
        var logsByRoute = logs
            .GroupBy(log => log.RouteID)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<RouteRiskLogResponse>)group
                    .Select(log => new RouteRiskLogResponse(
                        log.LoggedAt,
                        log.ResultSnapshot))
                    .ToArray());

        return Ok(routes.Select(route => new RouteHistoryResponse(
            route.RouteID,
            route.Origin,
            route.Destination,
            route.RequestDateTime,
            optionsByRoute.GetValueOrDefault(route.RouteID, []),
            logsByRoute.GetValueOrDefault(route.RouteID, []))));
    }
}
