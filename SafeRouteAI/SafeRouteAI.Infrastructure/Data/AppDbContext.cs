using Microsoft.EntityFrameworkCore;
using SafeRouteAI.Domain.Entities;
using System.Reflection.Emit;

namespace SafeRouteAI.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public const string DefaultConnection =
        "Server=(localdb)\\MSSQLLocalDB;Database=SafeRouteAI;Trusted_Connection=True;TrustServerCertificate=True";

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Route> Routes => Set<Route>();
    public DbSet<RouteOption> RouteOptions => Set<RouteOption>();
    public DbSet<RoadSegment> RoadSegments => Set<RoadSegment>();
    public DbSet<CommunityReport> CommunityReports => Set<CommunityReport>();
    public DbSet<WeatherLog> WeatherLogs => Set<WeatherLog>();
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Sizes and types follow the Phase 3 data dictionary
        b.Entity<CommunityReport>(e =>
        {
            e.Property(r => r.IncidentType).HasMaxLength(50);
            e.Property(r => r.Description).HasMaxLength(500);
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        });

        b.Entity<RiskAssessment>(e =>
        {
            e.Property(r => r.RiskLevel).HasConversion<string>().HasMaxLength(10);
            e.Property(r => r.AssessmentStatus).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.RiskScore).HasPrecision(5, 4);
        });

        b.Entity<RouteOption>(e =>
        {
            e.Property(r => r.Distance).HasPrecision(10, 2);
            e.Property(r => r.RiskScore).HasPrecision(5, 4);
        });
    }
}