using SafeRouteAI.Domain.Enums;
namespace SafeRouteAI.Domain.Entities;

public class CommunityReport
{
    public int ReportId { get; set; }
    public string UserId { get; set; } = "";
    public double Lat { get; set; }
    public double Lon { get; set; }
    public string IncidentType { get; set; } = "";
    public string? Description { get; set; }
    public DateTime ReportDateTime { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public int? ReviewedByAdminId { get; set; }
    public DateTime? ReviewedAt { get; set; }
}