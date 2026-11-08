using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeRouteAI.API.DTOs.Admin;
using SafeRouteAI.Application.Auth;
using SafeRouteAI.Domain.Enums;
using SafeRouteAI.Infrastructure.Data;

namespace SafeRouteAI.API.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/v1/admin/analytics")]
public sealed class AdminAnalyticsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AnalyticsResponse>> Get(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var endDate = to ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = from ?? endDate.AddDays(-365);
        if (startDate > endDate || endDate.DayNumber - startDate.DayNumber > 365)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid analytics date range",
                Detail = "Choose a range no longer than 366 days with From on or before To."
            });
        }

        var start = startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endExclusive = endDate.AddDays(1)
            .ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var routes = db.Routes.Where(
            route => route.RequestDateTime >= start &&
                     route.RequestDateTime < endExclusive);
        var reports = db.CommunityReports.Where(
            report => report.ReportDateTime >= start &&
                      report.ReportDateTime < endExclusive);
        var assessments = db.RiskAssessments.AsNoTracking();

        var routeDayRows = await routes
            .GroupBy(route => route.RequestDateTime.Date)
            .Select(group => new
            {
                Date = group.Key,
                Count = group.Count()
            })
            .OrderBy(item => item.Date)
            .ToListAsync(cancellationToken);
        var routeDays = routeDayRows
            .Select(item => new DailyCount(
                DateOnly.FromDateTime(item.Date),
                item.Count))
            .ToArray();

        var reportStatus = await reports
            .GroupBy(report => report.Status)
            .Select(group => new NamedCount(
                group.Key.ToString(),
                group.Count()))
            .ToListAsync(cancellationToken);

        var reportTypeRows = await reports
            .GroupBy(report => report.IncidentType)
            .Select(group => new
            {
                Name = group.Key,
                Count = group.Count()
            })
            .ToListAsync(cancellationToken);
        var reportTypes = reportTypeRows
            .OrderByDescending(item => item.Count)
            .Select(item => new NamedCount(item.Name, item.Count))
            .ToArray();

        var riskLevels = await assessments
            .Where(assessment => assessment.RiskLevel != null)
            .GroupBy(assessment => assessment.RiskLevel)
            .Select(group => new NamedCount(
                group.Key!.Value.ToString(),
                group.Count()))
            .ToListAsync(cancellationToken);

        var riskRows = await assessments
            .OrderByDescending(assessment => assessment.RiskAssessmentID)
            .Take(200)
            .Select(assessment => new
            {
                RoadName = assessment.RoadSegment == null
                    ? null
                    : assessment.RoadSegment.RoadName,
                assessment.RiskScore,
                RiskLevel = assessment.RiskLevel,
                Status = assessment.AssessmentStatus,
                Explanation = assessment.RiskExplanation == null
                    ? null
                    : assessment.RiskExplanation.ExplanationText,
                WeatherObservedAt = assessment.WeatherLog == null
                    ? (DateTime?)null
                    : assessment.WeatherLog.DateTime,
                Precipitation = assessment.WeatherLog == null
                    ? (double?)null
                    : assessment.WeatherLog.Precipitation,
                Humidity = assessment.WeatherLog == null
                    ? (double?)null
                    : assessment.WeatherLog.Humidity,
                Visibility = assessment.WeatherLog == null
                    ? (double?)null
                    : assessment.WeatherLog.Visibility,
                WindSpeed = assessment.WeatherLog == null
                    ? (double?)null
                    : assessment.WeatherLog.WindSpeed
            })
            .ToListAsync(cancellationToken);
        var riskDetails = riskRows
            .Select(item => new RiskAssessmentDetail(
                item.RoadName ?? "Unnamed road segment",
                item.RiskScore,
                item.RiskLevel?.ToString(),
                item.Status.ToString(),
                item.Explanation,
                item.WeatherObservedAt,
                item.Precipitation,
                item.Humidity,
                item.Visibility,
                item.WindSpeed))
            .ToArray();

        var accidents = await db.AccidentRecords
            .AsNoTracking()
            .OrderByDescending(item => item.DateTime)
            .Take(200)
            .Select(item => new AccidentRecordDetail(
                item.DateTime,
                item.AccidentType,
                item.Severity,
                (double)item.Latitude,
                (double)item.Longitude))
            .ToListAsync(cancellationToken);

        var modelVersions = await db.ModelVersions
            .AsNoTracking()
            .OrderByDescending(item => item.TrainingDate)
            .Take(200)
            .Select(item => new ModelVersionDetail(
                item.VersionLabel,
                item.TrainingDate,
                item.DatasetVersion,
                item.ValidationSummary))
            .ToListAsync(cancellationToken);

        var routeCount = await routes.CountAsync(cancellationToken);
        var reportCount = await reports.CountAsync(cancellationToken);
        var riskCount = await assessments.CountAsync(cancellationToken);
        var insufficientRiskCount = await assessments.CountAsync(
            assessment => assessment.AssessmentStatus == AssessmentStatus.InsufficientData,
            cancellationToken);
        var accidentCount = await db.AccidentRecords.CountAsync(cancellationToken);
        var modelVersionCount = await db.ModelVersions.CountAsync(cancellationToken);

        return Ok(new AnalyticsResponse(
            startDate,
            endDate,
            routeCount,
            reportCount,
            riskCount,
            insufficientRiskCount,
            accidentCount,
            modelVersionCount,
            routeDays,
            reportStatus,
            reportTypes,
            riskLevels,
            riskDetails,
            accidents,
            modelVersions));
    }
}
