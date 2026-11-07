
using System.ComponentModel.DataAnnotations;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Domain.Enums;

namespace SafeRouteAI.API.DTOs.Reports;

public sealed class CreateReportRequest
{
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Required]
    public string IncidentType { get; set; } = "";

    [MaxLength(500)]
    public string? Description { get; set; }
}

public sealed record ReportResponse(
    int ReportId,
    int UserId,
    double Latitude,
    double Longitude,
    string IncidentType,
    string? Description,
    DateTime ReportedAt,
    ReportStatus Status,
    int? ReviewedByAdminId,
    DateTime? ReviewedAt)
{
    public static ReportResponse FromDto(CommunityReportDto report) =>
        new(
            report.ReportId,
            report.UserId,
            report.Latitude,
            report.Longitude,
            report.IncidentType,
            report.Description,
            report.ReportedAt,
            report.Status,
            report.ReviewedByAdminId,
            report.ReviewedAt);
}

public sealed class ModerateReportRequest
{
    [Required]
    public string Status { get; set; } = "";
}
