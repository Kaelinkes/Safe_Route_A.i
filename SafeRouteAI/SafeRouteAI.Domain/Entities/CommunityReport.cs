namespace SafeRouteAI.Domain.Entities;

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

    public string Status { get; set; } = "Pending";

    public int? ReviewedByAdminID { get; set; }

    public DateTime? ReviewedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }

    public User? ReviewedByAdmin { get; set; }
}