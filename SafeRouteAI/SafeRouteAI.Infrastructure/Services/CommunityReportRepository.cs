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
            Status = r.Status.ToString()
        };
        db.Set<CommunityReport>().Add(entity);
        await db.SaveChangesAsync();
        r.ReportId = entity.ReportID;
    }

    public Task<List<CommunityReportDto>> GetPendingAsync() => GetByStatus(ReportStatus.Pending);
    public Task<List<CommunityReportDto>> GetApprovedAsync() => GetByStatus(ReportStatus.Approved);

    public async Task UpdateStatusAsync(int reportId, ReportStatus status, int adminId)
    {
        var entity = await db.Set<CommunityReport>().FindAsync(reportId)
            ?? throw new KeyNotFoundException($"Report {reportId} not found.");

        entity.Status = status.ToString();
        entity.ReviewedByAdminID = adminId;
        entity.ReviewedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private async Task<List<CommunityReportDto>> GetByStatus(ReportStatus status)
    {
        var name = status.ToString();
        var rows = await db.Set<CommunityReport>().AsNoTracking()
            .Where(r => r.Status == name).ToListAsync();
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
        Status = Enum.TryParse<ReportStatus>(e.Status, true, out var s) ? s : ReportStatus.Pending,
        ReviewedByAdminId = e.ReviewedByAdminID,
        ReviewedAt = e.ReviewedAt
    };
}