using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Application.Validation;
using SafeRouteAI.Domain.Enums;

namespace SafeRouteAI.Application.Services;

public class CommunityReportService(ICommunityReportRepository repo)
{
    public async Task<bool> SubmitAsync(CommunityReportDto report)
    {
        if (!Validators.IsValid(report)) return false;

        report.Status = CommunityReportStatus.Pending;
        report.ReportDateTime = DateTime.UtcNow;
        await _repo.AddAsync(report);
        return true;
    }

    public Task ModerateAsync(int reportId, CommunityReportStatus status, int adminId) =>
        _repo.UpdateStatusAsync(reportId, status, adminId);
        report.Status = ReportStatus.Pending;
        report.ReportedAt = DateTime.UtcNow;
        await repo.AddAsync(report);
        return true;
    }

    public Task<List<CommunityReportDto>> GetPendingAsync() => repo.GetPendingAsync();
    public Task<List<CommunityReportDto>> GetApprovedAsync() => repo.GetApprovedAsync();

    public Task ModerateAsync(int reportId, ReportStatus status, int adminId) =>
        repo.UpdateStatusAsync(reportId, status, adminId);
}