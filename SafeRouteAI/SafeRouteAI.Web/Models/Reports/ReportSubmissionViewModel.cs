using System.ComponentModel.DataAnnotations;

namespace SafeRouteAI.Web.Models.Reports;

public sealed class ReportSubmissionViewModel
{
    [Required]
    [StringLength(32)]
    public string Latitude { get; set; } = "";

    [Required]
    [StringLength(32)]
    public string Longitude { get; set; } = "";

    [Required]
    [StringLength(100)]
    public string IncidentType { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }

    public bool ResponsibleUse { get; set; }
}
