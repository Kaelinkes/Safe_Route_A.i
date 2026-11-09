using Microsoft.EntityFrameworkCore;
using SafeRouteAI.Application.DTOs;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Domain.Entities;
using SafeRouteAI.Domain.Enums;
using SafeRouteAI.Infrastructure.Data;

namespace SafeRouteAI.Infrastructure.Services;

public class CommunityReportRepository(AppDbContext db) : ICommunityReportRepository
{
    public async Task AddAsync(CommunityReportDto r)
    {
        var entity = new CommunityReport
        {
            UserID = r.UserId,
            Latitude = (decimal)r.Latitude,
            Longitude = (decimal)r.Longitude,
            IncidentType = r.IncidentType,
            Description = r.Description,
            ReportDateTime = r.ReportedAt,
            Status = CommunityReportStatus.Pending
        };

        db.Set<CommunityReport>().Add(entity);
        await db.SaveChangesAsync();

        r.ReportId = entity.ReportID;
    }

    public Task<List<CommunityReportDto>> GetPendingAsync() =>
        GetByStatus(ReportStatus.Pending);

    public Task<List<CommunityReportDto>> GetApprovedAsync() =>
        GetByStatuses(ReportStatus.Approved, ReportStatus.Reviewed);

    public async Task UpdateStatusAsync(int reportId, ReportStatus status, int adminId)
    {
        var entity = await db.Set<CommunityReport>().FindAsync(reportId)
            ?? throw new KeyNotFoundException($"Report {reportId} not found.");

        entity.Status = Enum.Parse<CommunityReportStatus>(status.ToString());
        entity.ReviewedByAdminID = adminId;
        entity.ReviewedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    public async Task<List<CommunityReportDto>> GetByUserAsync(int userId)
    {
        var rows = await db.Set<CommunityReport>()
            .AsNoTracking()
            .Where(report => report.UserID == userId)
            .OrderByDescending(report => report.ReportDateTime)
            .ToListAsync();

        return rows.Select(ToDto).ToList();
    }

    private async Task<List<CommunityReportDto>> GetByStatus(ReportStatus status) =>
        await GetByStatuses(status);

    private async Task<List<CommunityReportDto>> GetByStatuses(
        params ReportStatus[] statuses)
    {
        var entityStatuses = statuses
            .Select(status => Enum.Parse<CommunityReportStatus>(status.ToString()))
            .ToArray();

        var rows = await db.Set<CommunityReport>()
            .AsNoTracking()
            .Where(report => entityStatuses.Contains(report.Status))
            .OrderByDescending(report => report.ReportDateTime)
            .ToListAsync();

        return rows.Select(ToDto).ToList();
    }

    private static CommunityReportDto ToDto(CommunityReport e) => new()
    {
        ReportId = e.ReportID,
        UserId = e.UserID,
        Latitude = (double)e.Latitude,
        Longitude = (double)e.Longitude,
        IncidentType = e.IncidentType,
        Description = e.Description,
        ReportedAt = e.ReportDateTime,
        Status = Enum.Parse<ReportStatus>(e.Status.ToString()),
        ReviewedByAdminId = e.ReviewedByAdminID,
        ReviewedAt = e.ReviewedAt
    };
}