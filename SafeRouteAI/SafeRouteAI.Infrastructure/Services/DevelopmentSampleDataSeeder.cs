using Microsoft.EntityFrameworkCore;
using SafeRouteAI.Domain.Entities;
using SafeRouteAI.Domain.Enums;
using SafeRouteAI.Infrastructure.Data;

namespace SafeRouteAI.Infrastructure.Services;

public sealed class DevelopmentSampleDataSeeder(AppDbContext db)
{
    private static readonly DateTime SampleDate = new(2026, 1, 16, 0, 0, 0, DateTimeKind.Utc);

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstAsync(
            item => item.Email == "safertest.user@example.test",
            cancellationToken);
        var admin = await db.Users.FirstAsync(
            item => item.Email == "safertest.admin@example.test",
            cancellationToken);

        var roads = await SeedRoadsAsync(cancellationToken);
        var weather = await SeedWeatherAsync(cancellationToken);
        await SeedRiskAssessmentsAsync(roads, weather, cancellationToken);
        var routes = await SeedRoutesAsync(user.UserID, roads, cancellationToken);
        await SeedReportsAsync(user.UserID, admin.UserID, cancellationToken);
        await SeedAccidentsAsync(cancellationToken);
        await SeedModelVersionsAsync(cancellationToken);
        await SeedRouteRiskLogsAsync(routes, cancellationToken);
    }

    private async Task<Dictionary<string, RoadSegment>> SeedRoadsAsync(
        CancellationToken cancellationToken)
    {
        var values = new[]
        {
            new RoadSegment
            {
                RoadName = "Sample Road A",
                RoadType = "Urban",
                StartLatitude = -26.204100m,
                StartLongitude = 28.047300m,
                EndLatitude = -26.205000m,
                EndLongitude = 28.050000m
            },
            new RoadSegment
            {
                RoadName = "Sample Road B",
                RoadType = "Urban",
                StartLatitude = -26.205000m,
                StartLongitude = 28.050000m,
                EndLatitude = -26.207000m,
                EndLongitude = 28.053000m
            },
            new RoadSegment
            {
                RoadName = "Sample Road C",
                RoadType = "Urban",
                StartLatitude = -26.207000m,
                StartLongitude = 28.053000m,
                EndLatitude = -26.210000m,
                EndLongitude = 28.057000m
            }
        };

        foreach (var road in values)
        {
            if (!await db.RoadSegments.AnyAsync(
                    item => item.RoadName == road.RoadName,
                    cancellationToken))
            {
                db.RoadSegments.Add(road);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        var roads = new Dictionary<string, RoadSegment>();
        foreach (var name in new[] { "Sample Road A", "Sample Road B", "Sample Road C" })
        {
            roads[name] = await db.RoadSegments
                .FirstAsync(item => item.RoadName == name, cancellationToken);
        }

        return roads;
    }

    private async Task<Dictionary<string, WeatherLog>> SeedWeatherAsync(
        CancellationToken cancellationToken)
    {
        var values = new[]
        {
            new WeatherLog
            {
                Latitude = -26.205000m,
                Longitude = 28.050000m,
                DateTime = SampleDate.AddHours(8),
                Precipitation = 0.0,
                Humidity = 45.0,
                Visibility = 10.0,
                WindSpeed = 12.5
            },
            new WeatherLog
            {
                Latitude = -26.207000m,
                Longitude = 28.053000m,
                DateTime = SampleDate.AddHours(8),
                Precipitation = 0.2,
                Humidity = 52.0,
                Visibility = 8.0,
                WindSpeed = 18.0
            }
        };

        foreach (var row in values)
        {
            if (!await db.WeatherLogs.AnyAsync(
                    item => item.Latitude == row.Latitude &&
                            item.Longitude == row.Longitude &&
                            item.DateTime == row.DateTime,
                    cancellationToken))
            {
                db.WeatherLogs.Add(row);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        var weather = new Dictionary<string, WeatherLog>();
        foreach (var location in new[]
                 {
                     (Location: "-26.205000,28.050000", Latitude: -26.205000m, Longitude: 28.050000m),
                     (Location: "-26.207000,28.053000", Latitude: -26.207000m, Longitude: 28.053000m)
                 })
        {
            weather[location.Location] = await db.WeatherLogs.FirstAsync(
                item => item.Latitude == location.Latitude &&
                        item.Longitude == location.Longitude &&
                        item.DateTime == SampleDate.AddHours(8),
                cancellationToken);
        }

        return weather;
    }

    private async Task SeedRiskAssessmentsAsync(
        IReadOnlyDictionary<string, RoadSegment> roads,
        IReadOnlyDictionary<string, WeatherLog> weather,
        CancellationToken cancellationToken)
    {
        var values = new[]
        {
            (Road: "Sample Road A", Location: "-26.205000,28.050000", Score: 0.18, Level: RiskLevel.Low, Explanation: "Sample low-risk assessment for test data."),
            (Road: "Sample Road B", Location: "-26.207000,28.053000", Score: 0.46, Level: RiskLevel.Medium, Explanation: "Sample moderate-risk assessment for test data.")
        };

        foreach (var value in values)
        {
            var road = roads[value.Road];
            var existing = await db.RiskAssessments
                .Include(item => item.RiskExplanation)
                .FirstOrDefaultAsync(
                    item => item.RoadSegmentID == road.RoadSegmentID,
                    cancellationToken);
            if (existing is null)
            {
                existing = new RiskAssessment
                {
                    RoadSegmentID = road.RoadSegmentID,
                    WeatherLogID = weather[value.Location].WeatherLogID,
                    RiskScore = value.Score,
                    RiskLevel = value.Level,
                    AssessmentStatus = AssessmentStatus.Valid
                };
                db.RiskAssessments.Add(existing);
                await db.SaveChangesAsync(cancellationToken);
            }

            if (existing.RiskExplanation is null &&
                !await db.Set<RiskExplanation>().AnyAsync(
                    item => item.RiskAssessmentID == existing.RiskAssessmentID,
                    cancellationToken))
            {
                db.Set<RiskExplanation>().Add(new RiskExplanation
                {
                    RiskAssessmentID = existing.RiskAssessmentID,
                    ExplanationText = value.Explanation
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Dictionary<string, Route>> SeedRoutesAsync(
        int userId,
        IReadOnlyDictionary<string, RoadSegment> roads,
        CancellationToken cancellationToken)
    {
        var values = new[]
        {
            (Origin: "Braamfontein", Destination: "Rosebank", Time: SampleDate.AddHours(8.5)),
            (Origin: "Rosebank", Destination: "Sandton", Time: SampleDate.AddHours(17.25))
        };

        foreach (var value in values)
        {
            if (!await db.Routes.AnyAsync(
                    item => item.UserID == userId &&
                            item.Origin == value.Origin &&
                            item.Destination == value.Destination &&
                            item.RequestDateTime == value.Time,
                    cancellationToken))
            {
                db.Routes.Add(new Route
                {
                    UserID = userId,
                    Origin = value.Origin,
                    Destination = value.Destination,
                    RequestDateTime = value.Time
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        var routes = new Dictionary<string, Route>();
        foreach (var (origin, destination) in new[]
                 {
                     ("Braamfontein", "Rosebank"),
                     ("Rosebank", "Sandton")
                 })
        {
            routes[$"{origin}|{destination}"] = await db.Routes.FirstAsync(
                item => item.UserID == userId &&
                        item.Origin == origin &&
                        item.Destination == destination &&
                        item.RequestDateTime >= SampleDate &&
                        item.RequestDateTime < SampleDate.AddDays(1),
                cancellationToken);
        }

        var firstRoute = routes["Braamfontein|Rosebank"];
        var options = new[]
        {
            (Rank: 1, Info: "Sample route via Road A and B", Distance: 4.2, Minutes: 14.0, Score: 0.28, Roads: new[] { "Sample Road A", "Sample Road B" }),
            (Rank: 2, Info: "Alternative sample route", Distance: 4.8, Minutes: 17.0, Score: 0.19, Roads: new[] { "Sample Road A", "Sample Road B", "Sample Road C" })
        };

        foreach (var option in options)
        {
            var existing = await db.RouteOptions.SingleOrDefaultAsync(
                item => item.RouteID == firstRoute.RouteID &&
                        item.Rank == option.Rank,
                cancellationToken);
            if (existing is null)
            {
                existing = new RouteOption
                {
                    RouteID = firstRoute.RouteID,
                    Rank = option.Rank,
                    RouteInfo = option.Info,
                    Geometry = "_p~iF~ps|U_ulLnnqC_mqNvxq`@",
                    Distance = option.Distance,
                    EstTimeMin = option.Minutes,
                    RiskScore = option.Score
                };
                db.RouteOptions.Add(existing);
                await db.SaveChangesAsync(cancellationToken);
            }

            foreach (var roadName in option.Roads)
            {
                if (!await db.RouteOptionRoadSegments.AnyAsync(
                        item => item.RouteOptionID == existing.RouteOptionID &&
                                item.RoadSegmentID == roads[roadName].RoadSegmentID,
                        cancellationToken))
                {
                    db.RouteOptionRoadSegments.Add(new RouteOptionRoadSegment
                    {
                        RouteOptionID = existing.RouteOptionID,
                        RoadSegmentID = roads[roadName].RoadSegmentID,
                        SegmentOrder = Array.IndexOf(option.Roads, roadName) + 1
                    });
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return routes;
    }

    private async Task SeedReportsAsync(
        int userId,
        int adminId,
        CancellationToken cancellationToken)
    {
        var values = new[]
        {
            new CommunityReport
            {
                UserID = userId,
                Latitude = -26.205000m,
                Longitude = 28.050000m,
                IncidentType = "Hazard",
                Description = "Sample road hazard report.",
                ReportDateTime = SampleDate.AddDays(1).AddHours(8).AddMinutes(10),
                Status = CommunityReportStatus.Reviewed,
                ReviewedByAdminID = adminId,
                ReviewedAt = SampleDate.AddDays(1).AddHours(8).AddMinutes(20)
            },
            new CommunityReport
            {
                UserID = userId,
                Latitude = -26.207000m,
                Longitude = 28.053000m,
                IncidentType = "Lighting",
                Description = "Sample report of poor lighting.",
                ReportDateTime = SampleDate.AddDays(1).AddHours(18).AddMinutes(30),
                Status = CommunityReportStatus.Pending
            }
        };

        foreach (var report in values)
        {
            if (!await db.CommunityReports.AnyAsync(
                    item => item.UserID == report.UserID &&
                            item.Latitude == report.Latitude &&
                            item.Longitude == report.Longitude &&
                            item.IncidentType == report.IncidentType &&
                            item.ReportDateTime == report.ReportDateTime,
                    cancellationToken))
            {
                db.CommunityReports.Add(report);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAccidentsAsync(CancellationToken cancellationToken)
    {
        var values = new[]
        {
            new AccidentRecord { Latitude = -26.205000m, Longitude = 28.050000m, DateTime = new DateTime(2026, 1, 10, 14, 0, 0, DateTimeKind.Utc), AccidentType = "Collision", Severity = "Minor" },
            new AccidentRecord { Latitude = -26.207000m, Longitude = 28.053000m, DateTime = new DateTime(2026, 1, 12, 19, 30, 0, DateTimeKind.Utc), AccidentType = "Pedestrian incident", Severity = "Moderate" },
            new AccidentRecord { Latitude = -26.210000m, Longitude = 28.057000m, DateTime = new DateTime(2026, 1, 14, 7, 45, 0, DateTimeKind.Utc), AccidentType = "Collision", Severity = "Minor" }
        };

        foreach (var row in values)
        {
            if (!await db.AccidentRecords.AnyAsync(
                    item => item.Latitude == row.Latitude &&
                            item.Longitude == row.Longitude &&
                            item.DateTime == row.DateTime,
                    cancellationToken))
            {
                db.AccidentRecords.Add(row);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedModelVersionsAsync(CancellationToken cancellationToken)
    {
        var values = new[]
        {
            new ModelVersion { VersionLabel = "seed-v1", TrainingDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), DatasetVersion = "sample-dataset-1", ModelPath = "/models/seed-v1", ValidationSummary = "Sample seed metadata only." },
            new ModelVersion { VersionLabel = "seed-v2", TrainingDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc), DatasetVersion = "sample-dataset-2", ModelPath = "/models/seed-v2", ValidationSummary = "Sample seed metadata only." }
        };

        foreach (var row in values)
        {
            if (!await db.ModelVersions.AnyAsync(
                    item => item.VersionLabel == row.VersionLabel,
                    cancellationToken))
            {
                db.ModelVersions.Add(row);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedRouteRiskLogsAsync(
        IReadOnlyDictionary<string, Route> routes,
        CancellationToken cancellationToken)
    {
        var values = new[]
        {
            (Route: "Braamfontein|Rosebank", At: SampleDate.AddHours(8).AddMinutes(35), Snapshot: "{\"source\":\"seed\",\"riskScore\":0.28,\"riskLevel\":\"Low\"}"),
            (Route: "Braamfontein|Rosebank", At: SampleDate.AddHours(17).AddMinutes(20), Snapshot: "{\"source\":\"seed\",\"riskScore\":0.19,\"riskLevel\":\"Low\"}")
        };

        foreach (var value in values)
        {
            var route = routes[value.Route];
            if (!await db.RouteRiskLogs.AnyAsync(
                    item => item.RouteID == route.RouteID &&
                            item.LoggedAt == value.At,
                    cancellationToken))
            {
                db.RouteRiskLogs.Add(new RouteRiskLog
                {
                    RouteID = route.RouteID,
                    LoggedAt = value.At,
                    ResultSnapshot = value.Snapshot
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
