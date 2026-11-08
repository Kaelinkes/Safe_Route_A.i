using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Domain.Enums;

namespace SafeRouteAI.Application.Services;

public class CommunityReportService(ICommunityReportRepository repository)
{
    public async Task<bool> SubmitAsync(CommunityReportDto report)
    {
        if (string.IsNullOrWhiteSpace(report.IncidentType) ||
            report.Latitude is < -90 or > 90 ||
            report.Longitude is < -180 or > 180)
        {
            return false;
        }

        report.ReportedAt = DateTime.UtcNow;
        report.Status = ReportStatus.Pending;

        await repository.AddAsync(report);

        return true;
    }

    public Task<List<CommunityReportDto>> GetPendingAsync() =>
        repository.GetPendingAsync();

    public Task<List<CommunityReportDto>> GetApprovedAsync() =>
        repository.GetApprovedAsync();

    public Task<List<CommunityReportDto>> GetByUserAsync(int userId) =>
        repository.GetByUserAsync(userId);

    public Task ModerateAsync(int reportId, ReportStatus status, int adminId)
    {
        if (status is not (ReportStatus.Approved or ReportStatus.Rejected))
        {
            throw new ArgumentException("Invalid moderation status.");
        }

        return repository.UpdateStatusAsync(reportId, status, adminId);
    }
}