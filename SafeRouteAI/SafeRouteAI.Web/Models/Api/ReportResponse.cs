namespace SafeRouteAI.Web.Models.Api;

public sealed record ReportResponse(
    int ReportId,
    int UserId,
    double Latitude,
    double Longitude,
    string IncidentType,
    string? Description,
    DateTime ReportedAt,
    string Status,
    int? ReviewedByAdminId,
    DateTime? ReviewedAt);

public sealed record CreateReportRequest(
    double Latitude,
    double Longitude,
    string IncidentType,
    string? Description);

public sealed record ModerateReportRequest(string Status);
