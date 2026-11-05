using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Application.Validation;
using SafeRouteAI.Domain.Entities;
using SafeRouteAI.Domain.Enums;

namespace SafeRouteAI.Application.Services;

public class CommunityReportService
{
    private readonly ICommunityReportRepository _repo;

    public CommunityReportService(ICommunityReportRepository repo) => _repo = repo;

    public async Task<bool> SubmitAsync(CommunityReport report)
    {
        if (!Validators.IsValid(report)) return false;

        report.Status = ReportStatus.Pending.ToString();
        report.ReportDateTime = DateTime.UtcNow;
        await _repo.AddAsync(report);
        return true;
    }

    public Task ModerateAsync(int reportId, ReportStatus status, int adminId) =>
        _repo.UpdateStatusAsync(reportId, status, adminId);
}