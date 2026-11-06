namespace SafeRouteAI.Domain.Entities;
using SafeRouteAI.Domain.Enums;

public class CommunityReport
{
    public int ReportID { get; set; }

    public int UserID { get; set; }

    // Coordinates
    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public string IncidentType { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime ReportDateTime { get; set; }

    public CommunityReportStatus Status { get; set; } = CommunityReportStatus.Pending;

    public int? ReviewedByAdminID { get; set; }

    public DateTime? ReviewedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }

    public User? ReviewedByAdmin { get; set; }
}