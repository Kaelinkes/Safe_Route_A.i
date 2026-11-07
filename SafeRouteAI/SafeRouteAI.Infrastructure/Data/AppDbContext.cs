using Microsoft.EntityFrameworkCore;
using SafeRouteAI.Domain.Entities;

namespace SafeRouteAI.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public const string DefaultConnection =
        "Server=(localdb)\\MSSQLLocalDB;Database=SafeRouteAI;Trusted_Connection=True;TrustServerCertificate=True";

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<RouteOption> RouteOptions => Set<RouteOption>();
    public DbSet<RouteOptionRoadSegment> RouteOptionRoadSegments => Set<RouteOptionRoadSegment>();
    public DbSet<RoadSegment> RoadSegments => Set<RoadSegment>();
    public DbSet<CommunityReport> CommunityReports => Set<CommunityReport>();
    public DbSet<WeatherLog> WeatherLogs => Set<WeatherLog>();
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();
    public DbSet<ModelVersion> ModelVersions => Set<ModelVersion>();
    public DbSet<AccidentRecord> AccidentRecords => Set<AccidentRecord>();
    public DbSet<RiskExplanation> RiskExplanations => Set<RiskExplanation>();
    public DbSet<RouteRiskLog> RouteRiskLogs => Set<RouteRiskLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        foreach (var entity in b.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.ClrType.Name) + "s");

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }

        b.Entity<User>(entity =>
        {
            entity.HasKey(x => x.UserID);
            entity.Property(x => x.UserID).ValueGeneratedOnAdd();
        });

        b.Entity<Route>(entity =>
        {
            entity.HasKey(x => x.RouteID);
            entity.Property(x => x.RouteID).ValueGeneratedOnAdd();
            entity.HasIndex(x => new { x.UserID, x.RequestDateTime });
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserID)
                .OnDelete(DeleteBehavior.NoAction);
        });

        b.Entity<RouteOption>(entity =>
        {
            entity.HasKey(x => x.RouteOptionID);
            entity.Property(x => x.RouteOptionID).ValueGeneratedOnAdd();
            entity.HasIndex(x => new { x.RouteID, x.Rank });
            entity.HasOne(x => x.Route)
                .WithMany()
                .HasForeignKey(x => x.RouteID)
                .OnDelete(DeleteBehavior.NoAction);
        });

        b.Entity<RouteOptionRoadSegment>(entity =>
        {
            entity.HasKey(x => new { x.RouteOptionID, x.RoadSegmentID });
            entity.HasIndex(x => new { x.RouteOptionID, x.SegmentOrder });
            entity.HasOne(x => x.RouteOption)
                .WithMany(x => x.RouteOptionRoadSegments)
                .HasForeignKey(x => x.RouteOptionID)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.RoadSegment)
                .WithMany(x => x.RouteOptionRoadSegments)
                .HasForeignKey(x => x.RoadSegmentID)
                .OnDelete(DeleteBehavior.NoAction);
        });

        b.Entity<RoadSegment>(entity =>
        {
            entity.HasKey(x => x.RoadSegmentID);
            entity.Property(x => x.RoadSegmentID).ValueGeneratedOnAdd();
            entity.Property(x => x.StartLatitude).HasPrecision(9, 6);
            entity.Property(x => x.StartLongitude).HasPrecision(9, 6);
            entity.Property(x => x.EndLatitude).HasPrecision(9, 6);
            entity.Property(x => x.EndLongitude).HasPrecision(9, 6);
            entity.Property(x => x.RoadType).IsRequired(false);
            entity.Property(x => x.RoadName).IsRequired(false);
        });

        b.Entity<CommunityReport>(entity =>
        {
            entity.HasKey(x => x.ReportID);
            entity.Property(x => x.ReportID).ValueGeneratedOnAdd();
            entity.Property(x => x.Status).HasConversion<string>();
            entity.Property(x => x.Latitude).HasPrecision(9, 6);
            entity.Property(x => x.Longitude).HasPrecision(9, 6);
            entity.Property(x => x.Description).IsRequired(false);
            entity.Property(x => x.ReviewedByAdminID).IsRequired(false);
            entity.Property(x => x.ReviewedAt).IsRequired(false);
            entity.HasIndex(x => x.ReportDateTime);
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserID)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.ReviewedByAdmin)
                .WithMany()
                .HasForeignKey(x => x.ReviewedByAdminID)
                .OnDelete(DeleteBehavior.NoAction);
        });

        b.Entity<RiskAssessment>(entity =>
        {
            entity.HasKey(x => x.RiskAssessmentID);
            entity.Property(x => x.RiskAssessmentID).ValueGeneratedOnAdd();
            entity.HasOne(x => x.RoadSegment)
                .WithMany(x => x.RiskAssessments)
                .HasForeignKey(x => x.RoadSegmentID)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.WeatherLog)
                .WithMany(x => x.RiskAssessments)
                .HasForeignKey(x => x.WeatherLogID)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.RiskExplanation)
                .WithOne(x => x.RiskAssessment)
                .HasForeignKey<RiskExplanation>(x => x.RiskAssessmentID)
                .OnDelete(DeleteBehavior.NoAction);
        });

        b.Entity<WeatherLog>(entity =>
        {
            entity.HasKey(x => x.WeatherLogID);
            entity.Property(x => x.WeatherLogID).ValueGeneratedOnAdd();
            entity.Property(x => x.Latitude).HasPrecision(9, 6);
            entity.Property(x => x.Longitude).HasPrecision(9, 6);
            entity.HasIndex(x => new { x.Latitude, x.Longitude, x.DateTime });
        });

        b.Entity<ModelVersion>(entity =>
        {
            entity.HasKey(x => x.ModelVersionID);
            entity.Property(x => x.ModelVersionID).ValueGeneratedOnAdd();
        });

        b.Entity<AccidentRecord>(entity =>
        {
            entity.HasKey(x => x.AccidentID);
            entity.Property(x => x.AccidentID).ValueGeneratedOnAdd();
            entity.Property(x => x.Latitude).HasPrecision(9, 6);
            entity.Property(x => x.Longitude).HasPrecision(9, 6);
            entity.Property(x => x.AccidentType).IsRequired(false);
            entity.Property(x => x.Severity).IsRequired(false);
            entity.HasIndex(x => new { x.Latitude, x.Longitude, x.DateTime });
        });

        b.Entity<RiskExplanation>(entity =>
        {
            entity.HasKey(x => x.RiskExplanationID);
            entity.Property(x => x.RiskExplanationID).ValueGeneratedOnAdd();
        });

        b.Entity<RouteRiskLog>(entity =>
        {
            entity.HasKey(x => x.RouteRiskLogID);
            entity.Property(x => x.RouteRiskLogID).ValueGeneratedOnAdd();
            entity.Property(x => x.ResultSnapshot).IsRequired();
            entity.HasIndex(x => new { x.RouteID, x.LoggedAt });
            entity.HasOne(x => x.Route)
                .WithMany()
                .HasForeignKey(x => x.RouteID)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }

    private static string ToSnakeCase(string name)
    {
        var result = new System.Text.StringBuilder(name.Length + 8);
        for (var index = 0; index < name.Length; index++)
        {
            var character = name[index];
            if (char.IsUpper(character) && index > 0 &&
                (char.IsLower(name[index - 1]) ||
                 (index + 1 < name.Length && char.IsLower(name[index + 1]))))
            {
                result.Append('_');
            }

            result.Append(char.ToLowerInvariant(character));
        }

        return result.ToString();
    }
}