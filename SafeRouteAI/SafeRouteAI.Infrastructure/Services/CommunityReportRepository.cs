using Microsoft.EntityFrameworkCore;
using SafeRouteAI.Application.Interfaces;
using SafeRouteAI.Domain.Entities;
using SafeRouteAI.Domain.Enums;
using SafeRouteAI.Infrastructure.Data;

namespace SafeRouteAI.Infrastructure.Services;

public class CommunityReportRepository : ICommunityReportRepository
{
    private readonly AppDbContext _db;

    public CommunityReportRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(CommunityReport report)
    {
        _db.CommunityReports.Add(report);
        await _db.SaveChangesAsync();
    }

    public Task<List<CommunityReport>> GetApprovedAsync() =>
        _db.CommunityReports.AsNoTracking()
            .Where(r => r.Status == ReportStatus.Approved.ToString())
            .ToListAsync();

    public async Task UpdateStatusAsync(int reportId, ReportStatus status, int adminId)
    {
        var report = await _db.CommunityReports.FindAsync(reportId)
            ?? throw new KeyNotFoundException($"Report {reportId} not found.");

        report.Status = status.ToString();
        report.ReviewedByAdminID = adminId;
        report.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}