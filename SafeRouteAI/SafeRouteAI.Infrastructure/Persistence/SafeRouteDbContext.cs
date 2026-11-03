using Microsoft.EntityFrameworkCore;
using SafeRouteAI.Infrastructure.Persistence.Entities;

namespace SafeRouteAI.Infrastructure.Persistence;

public sealed class SafeRouteDbContext(DbContextOptions<SafeRouteDbContext> options) : DbContext(options)
{
    public DbSet<UserRoleRecord> UserRoles => Set<UserRoleRecord>();
    public DbSet<AccountStatusRecord> AccountStatuses => Set<AccountStatusRecord>();
    public DbSet<RequestStatusRecord> RequestStatuses => Set<RequestStatusRecord>();
    public DbSet<RiskCategoryRecord> RiskCategories => Set<RiskCategoryRecord>();
    public DbSet<ReportStatusRecord> ReportStatuses => Set<ReportStatusRecord>();
    public DbSet<WeatherSeverityRecord> WeatherSeverities => Set<WeatherSeverityRecord>();
    public DbSet<ExternalServiceRecord> ExternalServices => Set<ExternalServiceRecord>();
    public DbSet<UserRecord> Users => Set<UserRecord>();
    public DbSet<LoginAttemptRecord> LoginAttempts => Set<LoginAttemptRecord>();
    public DbSet<UserSessionRecord> UserSessions => Set<UserSessionRecord>();
    public DbSet<RoadSegmentRecord> RoadSegments => Set<RoadSegmentRecord>();
    public DbSet<HistoricalAccidentRecord> HistoricalAccidents => Set<HistoricalAccidentRecord>();
    public DbSet<RouteRequestRecord> RouteRequests => Set<RouteRequestRecord>();
    public DbSet<ExternalApiCallRecord> ExternalApiCalls => Set<ExternalApiCallRecord>();
    public DbSet<CandidateRouteRecord> CandidateRoutes => Set<CandidateRouteRecord>();
    public DbSet<RouteSegmentRecord> RouteSegments => Set<RouteSegmentRecord>();
    public DbSet<WeatherObservationRecord> WeatherObservations => Set<WeatherObservationRecord>();
    public DbSet<IncidentTypeRecord> IncidentTypes => Set<IncidentTypeRecord>();
    public DbSet<CommunityReportRecord> CommunityReports => Set<CommunityReportRecord>();
    public DbSet<ReportStatusHistoryRecord> ReportStatusHistory => Set<ReportStatusHistoryRecord>();
    public DbSet<RiskModelVersionRecord> RiskModelVersions => Set<RiskModelVersionRecord>();
    public DbSet<SegmentRiskAssessmentRecord> SegmentRiskAssessments => Set<SegmentRiskAssessmentRecord>();
    public DbSet<AssessmentCommunityReportRecord> AssessmentCommunityReports => Set<AssessmentCommunityReportRecord>();
    public DbSet<RiskFactorRecord> RiskFactors => Set<RiskFactorRecord>();
    public DbSet<AssessmentFactorRecord> AssessmentFactors => Set<AssessmentFactorRecord>();
    public DbSet<HighRiskCriteriaRecord> HighRiskCriteria => Set<HighRiskCriteriaRecord>();
    public DbSet<RouteWarningRecord> RouteWarnings => Set<RouteWarningRecord>();
    public DbSet<RouteRiskLogRecord> RouteRiskLogs => Set<RouteRiskLogRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureLookups(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureRoadAndRouteTables(modelBuilder);
        ConfigureSafetyTables(modelBuilder);
        ConfigureRiskTables(modelBuilder);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }
    }

    private static void ConfigureLookups(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRoleRecord>(entity =>
        {
            entity.ToTable("ref_user_role");
            entity.HasKey(x => x.RoleCode);
            entity.Property(x => x.RoleCode).HasMaxLength(20).ValueGeneratedNever();
            entity.HasData(new UserRoleRecord { RoleCode = "user" }, new UserRoleRecord { RoleCode = "administrator" });
        });
        modelBuilder.Entity<AccountStatusRecord>(entity =>
        {
            entity.ToTable("ref_account_status");
            entity.HasKey(x => x.StatusCode);
            entity.Property(x => x.StatusCode).HasMaxLength(20).ValueGeneratedNever();
            entity.HasData(
                new AccountStatusRecord { StatusCode = "active" },
                new AccountStatusRecord { StatusCode = "suspended" },
                new AccountStatusRecord { StatusCode = "deleted" });
        });
        modelBuilder.Entity<RequestStatusRecord>(entity =>
        {
            entity.ToTable("ref_request_status");
            entity.HasKey(x => x.StatusCode);
            entity.Property(x => x.StatusCode).HasMaxLength(20).ValueGeneratedNever();
            entity.HasData(
                new RequestStatusRecord { StatusCode = "pending" },
                new RequestStatusRecord { StatusCode = "routing" },
                new RequestStatusRecord { StatusCode = "assessing" },
                new RequestStatusRecord { StatusCode = "completed" },
                new RequestStatusRecord { StatusCode = "failed" });
        });
        modelBuilder.Entity<RiskCategoryRecord>(entity =>
        {
            entity.ToTable("ref_risk_category");
            entity.HasKey(x => x.CategoryCode);
            entity.Property(x => x.CategoryCode).HasMaxLength(20).ValueGeneratedNever();
            entity.HasData(
                new RiskCategoryRecord { CategoryCode = "low", SortOrder = 1 },
                new RiskCategoryRecord { CategoryCode = "moderate", SortOrder = 2 },
                new RiskCategoryRecord { CategoryCode = "high", SortOrder = 3 },
                new RiskCategoryRecord { CategoryCode = "insufficient_data", SortOrder = 4 });
        });
        modelBuilder.Entity<ReportStatusRecord>(entity =>
        {
            entity.ToTable("ref_report_status");
            entity.HasKey(x => x.StatusCode);
            entity.Property(x => x.StatusCode).HasMaxLength(20).ValueGeneratedNever();
            entity.HasData(
                new ReportStatusRecord { StatusCode = "pending" },
                new ReportStatusRecord { StatusCode = "approved" },
                new ReportStatusRecord { StatusCode = "rejected" },
                new ReportStatusRecord { StatusCode = "expired" },
                new ReportStatusRecord { StatusCode = "resolved" });
        });
        modelBuilder.Entity<WeatherSeverityRecord>(entity =>
        {
            entity.ToTable("ref_weather_severity");
            entity.HasKey(x => x.SeverityCode);
            entity.Property(x => x.SeverityCode).HasMaxLength(10).ValueGeneratedNever();
            entity.HasIndex(x => x.SeverityLevel).IsUnique();
            entity.HasData(
                new WeatherSeverityRecord { SeverityCode = "good", SeverityLevel = 1 },
                new WeatherSeverityRecord { SeverityCode = "fair", SeverityLevel = 2 },
                new WeatherSeverityRecord { SeverityCode = "poor", SeverityLevel = 3 },
                new WeatherSeverityRecord { SeverityCode = "severe", SeverityLevel = 4 });
        });
        modelBuilder.Entity<ExternalServiceRecord>(entity =>
        {
            entity.ToTable("ref_external_service");
            entity.HasKey(x => x.ServiceCode);
            entity.Property(x => x.ServiceCode).HasMaxLength(20).ValueGeneratedNever();
            entity.HasData(
                new ExternalServiceRecord { ServiceCode = "routing" },
                new ExternalServiceRecord { ServiceCode = "weather" });
        });
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRecord>(entity =>
        {
            entity.ToTable("users", table => table.HasCheckConstraint("ck_users_failed_logins", "[failed_logins] >= 0"));
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.UserId).UseIdentityColumn();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.Property(x => x.EmailNormalized).HasMaxLength(255).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(x => x.RoleCode).HasMaxLength(20).HasDefaultValue("user");
            entity.Property(x => x.StatusCode).HasMaxLength(20).HasDefaultValue("active");
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.EmailNormalized).IsUnique();
            entity.HasOne<UserRoleRecord>().WithMany().HasForeignKey(x => x.RoleCode).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<AccountStatusRecord>().WithMany().HasForeignKey(x => x.StatusCode).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LoginAttemptRecord>(entity =>
        {
            entity.ToTable("login_attempts");
            entity.HasKey(x => x.AttemptId);
            entity.Property(x => x.AttemptId).UseIdentityColumn();
            entity.Property(x => x.EmailTried).HasMaxLength(255).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(100);
            entity.Property(x => x.IpAddress).HasMaxLength(45);
            entity.Property(x => x.AttemptedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => new { x.UserId, x.AttemptedAt });
            entity.HasOne<UserRecord>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserSessionRecord>(entity =>
        {
            entity.ToTable("user_sessions");
            entity.HasKey(x => x.SessionId);
            entity.Property(x => x.SessionId).HasMaxLength(36).ValueGeneratedNever();
            entity.Property(x => x.TokenHash).HasMaxLength(255).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => x.UserId);
            entity.HasOne<UserRecord>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureRoadAndRouteTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoadSegmentRecord>(entity =>
        {
            entity.ToTable("road_segments", table =>
            {
                table.HasCheckConstraint("ck_road_segments_speed", "[speed_limit_kmh] IS NULL OR [speed_limit_kmh] > 0");
                table.HasCheckConstraint("ck_road_segments_lanes", "[lanes] IS NULL OR [lanes] > 0");
                table.HasCheckConstraint("ck_road_segments_length", "[length_m] > 0");
                table.HasCheckConstraint("ck_road_segments_start_lat", "[start_lat] BETWEEN -90 AND 90");
                table.HasCheckConstraint("ck_road_segments_start_lng", "[start_lng] BETWEEN -180 AND 180");
                table.HasCheckConstraint("ck_road_segments_end_lat", "[end_lat] BETWEEN -90 AND 90");
                table.HasCheckConstraint("ck_road_segments_end_lng", "[end_lng] BETWEEN -180 AND 180");
                table.HasCheckConstraint("ck_road_segments_bbox", "[min_lat] <= [max_lat] AND [min_lng] <= [max_lng]");
            });
            entity.HasKey(x => x.SegmentId);
            entity.Property(x => x.SegmentId).UseIdentityColumn();
            entity.Property(x => x.ExternalRef).HasMaxLength(100);
            entity.Property(x => x.RoadName).HasMaxLength(200);
            entity.Property(x => x.RoadType).HasMaxLength(50);
            entity.Property(x => x.LengthM).HasPrecision(10, 2);
            entity.Property(x => x.ExternalRef).IsRequired(false);
            entity.Property(x => x.StartLat).HasPrecision(9, 6);
            entity.Property(x => x.StartLng).HasPrecision(9, 6);
            entity.Property(x => x.EndLat).HasPrecision(9, 6);
            entity.Property(x => x.EndLng).HasPrecision(9, 6);
            entity.Property(x => x.MinLat).HasPrecision(9, 6);
            entity.Property(x => x.MaxLat).HasPrecision(9, 6);
            entity.Property(x => x.MinLng).HasPrecision(9, 6);
            entity.Property(x => x.MaxLng).HasPrecision(9, 6);
            entity.HasIndex(x => x.ExternalRef).IsUnique();
            entity.HasIndex(x => new { x.MinLat, x.MaxLat, x.MinLng, x.MaxLng });
        });

        modelBuilder.Entity<HistoricalAccidentRecord>(entity =>
        {
            entity.ToTable("historical_accidents", table =>
            {
                table.HasCheckConstraint("ck_accidents_latitude", "[latitude] BETWEEN -90 AND 90");
                table.HasCheckConstraint("ck_accidents_longitude", "[longitude] BETWEEN -180 AND 180");
                table.HasCheckConstraint("ck_accidents_severity", "[severity] IS NULL OR [severity] BETWEEN 1 AND 5");
                table.HasCheckConstraint("ck_accidents_vehicles", "[vehicles_involved] IS NULL OR [vehicles_involved] >= 0");
                table.HasCheckConstraint("ck_accidents_casualties", "[casualties] IS NULL OR [casualties] >= 0");
            });
            entity.HasKey(x => x.AccidentId);
            entity.Property(x => x.AccidentId).UseIdentityColumn();
            entity.Property(x => x.Latitude).HasPrecision(9, 6);
            entity.Property(x => x.Longitude).HasPrecision(9, 6);
            entity.Property(x => x.WeatherCondition).HasMaxLength(50);
            entity.Property(x => x.LightCondition).HasMaxLength(50);
            entity.Property(x => x.Source).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.SegmentId, x.OccurredAt });
            entity.HasIndex(x => new { x.Latitude, x.Longitude });
            entity.HasOne<RoadSegmentRecord>().WithMany().HasForeignKey(x => x.SegmentId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RouteRequestRecord>(entity =>
        {
            entity.ToTable("route_requests", table =>
            {
                table.HasCheckConstraint("ck_route_requests_origin_lat", "[origin_lat] BETWEEN -90 AND 90");
                table.HasCheckConstraint("ck_route_requests_origin_lng", "[origin_lng] BETWEEN -180 AND 180");
                table.HasCheckConstraint("ck_route_requests_destination_lat", "[destination_lat] BETWEEN -90 AND 90");
                table.HasCheckConstraint("ck_route_requests_destination_lng", "[destination_lng] BETWEEN -180 AND 180");
                table.HasCheckConstraint("ck_route_requests_distinct_points", "[origin_lat] <> [destination_lat] OR [origin_lng] <> [destination_lng]");
                table.HasCheckConstraint("ck_route_requests_processing_ms", "[processing_ms] IS NULL OR [processing_ms] >= 0");
            });
            entity.HasKey(x => x.RequestId);
            entity.Property(x => x.RequestId).UseIdentityColumn();
            entity.Property(x => x.OriginLabel).HasMaxLength(255).IsRequired();
            entity.Property(x => x.DestinationLabel).HasMaxLength(255).IsRequired();
            entity.Property(x => x.OriginLat).HasPrecision(9, 6);
            entity.Property(x => x.OriginLng).HasPrecision(9, 6);
            entity.Property(x => x.DestinationLat).HasPrecision(9, 6);
            entity.Property(x => x.DestinationLng).HasPrecision(9, 6);
            entity.Property(x => x.StatusCode).HasMaxLength(20).HasDefaultValue("pending");
            entity.Property(x => x.RequestedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => new { x.UserId, x.RequestedAt });
            entity.HasOne<UserRecord>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<RequestStatusRecord>().WithMany().HasForeignKey(x => x.StatusCode).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ExternalApiCallRecord>(entity =>
        {
            entity.ToTable("external_api_calls", table => table.HasCheckConstraint("ck_external_api_calls_latency", "[latency_ms] IS NULL OR [latency_ms] >= 0"));
            entity.HasKey(x => x.CallId);
            entity.Property(x => x.CallId).UseIdentityColumn();
            entity.Property(x => x.ServiceCode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Provider).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CalledAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.RequestId);
            entity.HasOne<RouteRequestRecord>().WithMany().HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ExternalServiceRecord>().WithMany().HasForeignKey(x => x.ServiceCode).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CandidateRouteRecord>(entity =>
        {
            entity.ToTable("candidate_routes", table =>
            {
                table.HasCheckConstraint("ck_candidate_routes_distance", "[distance_m] > 0");
                table.HasCheckConstraint("ck_candidate_routes_duration", "[duration_s] > 0");
                table.HasCheckConstraint("ck_candidate_routes_risk", "([overall_risk_score] IS NULL OR [overall_risk_score] BETWEEN 0 AND 100) AND ([max_segment_risk_score] IS NULL OR [max_segment_risk_score] BETWEEN 0 AND 100)");
                table.HasCheckConstraint("ck_candidate_routes_rank", "([rank_by_time] IS NULL OR [rank_by_time] > 0) AND ([rank_by_risk] IS NULL OR [rank_by_risk] > 0) AND ([overall_rank] IS NULL OR [overall_rank] > 0)");
            });
            entity.HasKey(x => x.RouteId);
            entity.Property(x => x.RouteId).UseIdentityColumn();
            entity.Property(x => x.ProviderRouteRef).HasMaxLength(100);
            entity.Property(x => x.Summary).HasMaxLength(255);
            entity.Property(x => x.DistanceM).HasPrecision(10, 1);
            entity.Property(x => x.Polyline).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(x => x.OverallRiskScore).HasPrecision(5, 2);
            entity.Property(x => x.OverallRiskCategory).HasMaxLength(20);
            entity.Property(x => x.MaxSegmentRiskScore).HasPrecision(5, 2);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.RequestId);
            entity.HasIndex(x => new { x.RequestId, x.OverallRank }).IsUnique().HasFilter("[overall_rank] IS NOT NULL");
            entity.HasOne<RouteRequestRecord>().WithMany().HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<RiskCategoryRecord>().WithMany().HasForeignKey(x => x.OverallRiskCategory).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RouteSegmentRecord>(entity =>
        {
            entity.ToTable("route_segments", table =>
            {
                table.HasCheckConstraint("ck_route_segments_sequence", "[seq_no] > 0");
                table.HasCheckConstraint("ck_route_segments_travel_time", "[est_travel_time_s] IS NULL OR [est_travel_time_s] >= 0");
            });
            entity.HasKey(x => new { x.RouteId, x.SeqNo });
            entity.HasIndex(x => x.SegmentId);
            entity.HasOne<CandidateRouteRecord>().WithMany().HasForeignKey(x => x.RouteId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<RoadSegmentRecord>().WithMany().HasForeignKey(x => x.SegmentId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSafetyTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherObservationRecord>(entity =>
        {
            entity.ToTable("weather_observations", table =>
            {
                table.HasCheckConstraint("ck_weather_coordinates", "[latitude] BETWEEN -90 AND 90 AND [longitude] BETWEEN -180 AND 180");
                table.HasCheckConstraint("ck_weather_window", "[valid_to] > [valid_from]");
                table.HasCheckConstraint("ck_weather_measurements", "([precipitation_mm] IS NULL OR [precipitation_mm] >= 0) AND ([visibility_m] IS NULL OR [visibility_m] >= 0) AND ([wind_speed_kmh] IS NULL OR [wind_speed_kmh] >= 0)");
            });
            entity.HasKey(x => x.WeatherId);
            entity.Property(x => x.WeatherId).UseIdentityColumn();
            entity.Property(x => x.Latitude).HasPrecision(9, 6);
            entity.Property(x => x.Longitude).HasPrecision(9, 6);
            entity.Property(x => x.ConditionText).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TemperatureC).HasPrecision(4, 1);
            entity.Property(x => x.PrecipitationMm).HasPrecision(6, 2);
            entity.Property(x => x.WindSpeedKmh).HasPrecision(5, 1);
            entity.Property(x => x.SeverityCode).HasMaxLength(10).IsRequired();
            entity.Property(x => x.Provider).HasMaxLength(100).IsRequired();
            entity.Property(x => x.FetchedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => new { x.Latitude, x.Longitude });
            entity.HasIndex(x => new { x.ValidFrom, x.ValidTo });
            entity.HasOne<WeatherSeverityRecord>().WithMany().HasForeignKey(x => x.SeverityCode).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IncidentTypeRecord>(entity =>
        {
            entity.ToTable("incident_types", table =>
            {
                table.HasCheckConstraint("ck_incident_types_ttl", "[default_ttl_minutes] > 0");
                table.HasCheckConstraint("ck_incident_types_weight", "[risk_weight] >= 0");
            });
            entity.HasKey(x => x.IncidentTypeId);
            entity.Property(x => x.IncidentTypeId).UseIdentityColumn();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(100).IsRequired();
            entity.Property(x => x.RiskWeight).HasPrecision(4, 2).HasDefaultValue(1m);
            entity.Property(x => x.DefaultTtlMinutes).HasDefaultValue(240);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasData(
                new IncidentTypeRecord { IncidentTypeId = 1, Code = "collision", Label = "Collision / crash", DefaultTtlMinutes = 180, RiskWeight = 1.50m },
                new IncidentTypeRecord { IncidentTypeId = 2, Code = "breakdown", Label = "Broken-down vehicle", DefaultTtlMinutes = 90, RiskWeight = 0.80m },
                new IncidentTypeRecord { IncidentTypeId = 3, Code = "pothole", Label = "Pothole / road damage", DefaultTtlMinutes = 1440, RiskWeight = 0.60m },
                new IncidentTypeRecord { IncidentTypeId = 4, Code = "flooding", Label = "Flooding", DefaultTtlMinutes = 360, RiskWeight = 1.40m },
                new IncidentTypeRecord { IncidentTypeId = 5, Code = "debris", Label = "Debris on road", DefaultTtlMinutes = 120, RiskWeight = 0.90m },
                new IncidentTypeRecord { IncidentTypeId = 6, Code = "roadworks", Label = "Roadworks", DefaultTtlMinutes = 1440, RiskWeight = 0.50m },
                new IncidentTypeRecord { IncidentTypeId = 7, Code = "signal_fail", Label = "Traffic signal failure", DefaultTtlMinutes = 240, RiskWeight = 1.00m });
        });

        modelBuilder.Entity<CommunityReportRecord>(entity =>
        {
            entity.ToTable("community_reports", table =>
            {
                table.HasCheckConstraint("ck_community_reports_coordinates", "[latitude] BETWEEN -90 AND 90 AND [longitude] BETWEEN -180 AND 180");
                table.HasCheckConstraint("ck_community_reports_description", "LEN([description] + '#') - 1 >= 5");
                table.HasCheckConstraint("ck_community_reports_expiry", "[expires_at] > [reported_at]");
                table.HasCheckConstraint("ck_community_reports_moderation", "[status_code] IN ('pending', 'expired') OR ([moderated_by] IS NOT NULL AND [moderated_at] IS NOT NULL)");
            });
            entity.HasKey(x => x.ReportId);
            entity.Property(x => x.ReportId).UseIdentityColumn();
            entity.Property(x => x.Latitude).HasPrecision(9, 6);
            entity.Property(x => x.Longitude).HasPrecision(9, 6);
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.StatusCode).HasMaxLength(20).HasDefaultValue("pending");
            entity.Property(x => x.ReportedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => new { x.StatusCode, x.ExpiresAt });
            entity.HasIndex(x => x.SegmentId);
            entity.HasIndex(x => new { x.Latitude, x.Longitude });
            entity.HasOne<UserRecord>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<IncidentTypeRecord>().WithMany().HasForeignKey(x => x.IncidentTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<RoadSegmentRecord>().WithMany().HasForeignKey(x => x.SegmentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<UserRecord>().WithMany().HasForeignKey(x => x.ModeratedBy).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<ReportStatusRecord>().WithMany().HasForeignKey(x => x.StatusCode).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReportStatusHistoryRecord>(entity =>
        {
            entity.ToTable("report_status_history");
            entity.HasKey(x => x.HistoryId);
            entity.Property(x => x.HistoryId).UseIdentityColumn();
            entity.Property(x => x.OldStatus).HasMaxLength(20);
            entity.Property(x => x.NewStatus).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ChangedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.ReportId);
            entity.HasOne<CommunityReportRecord>().WithMany().HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ReportStatusRecord>().WithMany().HasForeignKey(x => x.OldStatus).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ReportStatusRecord>().WithMany().HasForeignKey(x => x.NewStatus).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<UserRecord>().WithMany().HasForeignKey(x => x.ChangedBy).OnDelete(DeleteBehavior.NoAction);
        });
    }

    private static void ConfigureRiskTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RiskModelVersionRecord>(entity =>
        {
            entity.ToTable("risk_model_versions", table =>
            {
                table.HasCheckConstraint("ck_model_min_data_points", "[min_data_points] >= 0");
                table.HasCheckConstraint("ck_model_active_flag", "[active_flag] IS NULL OR [active_flag] = 'Y'");
            });
            entity.HasKey(x => x.ModelId);
            entity.Property(x => x.ModelId).UseIdentityColumn();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Version).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ModelArtifactUri).HasMaxLength(500).IsRequired();
            entity.Property(x => x.ArtifactChecksum).HasMaxLength(128);
            entity.Property(x => x.MinDataPoints).HasDefaultValue(10);
            entity.Property(x => x.ActiveFlag).HasMaxLength(1);
            entity.HasIndex(x => new { x.Name, x.Version }).IsUnique();
            entity.HasIndex(x => x.ActiveFlag).IsUnique().HasFilter("[active_flag] IS NOT NULL");
        });

        modelBuilder.Entity<SegmentRiskAssessmentRecord>(entity =>
        {
            entity.ToTable("segment_risk_assessments", table =>
            {
                table.HasCheckConstraint("ck_assessment_risk_score", "[risk_score] IS NULL OR [risk_score] BETWEEN 0 AND 100");
                table.HasCheckConstraint("ck_assessment_score_sufficiency", "([data_sufficient] = 1 AND [risk_score] IS NOT NULL AND [category_code] <> 'insufficient_data') OR ([data_sufficient] = 0 AND [risk_score] IS NULL AND [category_code] = 'insufficient_data')");
            });
            entity.HasKey(x => x.AssessmentId);
            entity.Property(x => x.AssessmentId).UseIdentityColumn();
            entity.Property(x => x.RiskScore).HasPrecision(5, 2);
            entity.Property(x => x.CategoryCode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ExplanationSummary).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.AssessedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => new { x.RouteId, x.SegmentId, x.ModelId }).IsUnique();
            entity.HasIndex(x => x.RouteId);
            entity.HasIndex(x => new { x.SegmentId, x.AssessedAt });
            entity.HasOne<CandidateRouteRecord>().WithMany().HasForeignKey(x => x.RouteId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<RoadSegmentRecord>().WithMany().HasForeignKey(x => x.SegmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<RiskModelVersionRecord>().WithMany().HasForeignKey(x => x.ModelId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<WeatherObservationRecord>().WithMany().HasForeignKey(x => x.WeatherId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<RiskCategoryRecord>().WithMany().HasForeignKey(x => x.CategoryCode).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AssessmentCommunityReportRecord>(entity =>
        {
            entity.ToTable("assessment_community_reports");
            entity.HasKey(x => new { x.AssessmentId, x.ReportId });
            entity.HasOne<SegmentRiskAssessmentRecord>().WithMany().HasForeignKey(x => x.AssessmentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<CommunityReportRecord>().WithMany().HasForeignKey(x => x.ReportId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RiskFactorRecord>(entity =>
        {
            entity.ToTable("risk_factors", table => table.HasCheckConstraint("ck_risk_factors_category", "[category] IN ('weather', 'historical', 'community', 'road', 'time', 'data')"));
            entity.HasKey(x => x.FactorId);
            entity.Property(x => x.FactorId).UseIdentityColumn();
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(30).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ExplanationTemplate).HasMaxLength(500).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasData(
                new RiskFactorRecord { FactorId = 1, Code = "heavy_rain", Category = "weather", DisplayName = "Heavy rain", ExplanationTemplate = "Heavy rain reduces visibility and increases braking distance." },
                new RiskFactorRecord { FactorId = 2, Code = "low_visibility", Category = "weather", DisplayName = "Low visibility", ExplanationTemplate = "Fog or low light makes hazards harder to see." },
                new RiskFactorRecord { FactorId = 3, Code = "accident_hotspot", Category = "historical", DisplayName = "Accident hotspot", ExplanationTemplate = "This stretch has a high number of past accidents." },
                new RiskFactorRecord { FactorId = 4, Code = "active_report", Category = "community", DisplayName = "Recent incident", ExplanationTemplate = "Drivers recently reported an incident here." },
                new RiskFactorRecord { FactorId = 5, Code = "high_speed_road", Category = "road", DisplayName = "High-speed road", ExplanationTemplate = "Higher speed limits increase crash severity." },
                new RiskFactorRecord { FactorId = 6, Code = "peak_hour", Category = "time", DisplayName = "Peak hours", ExplanationTemplate = "Traffic density is higher at this time of day." },
                new RiskFactorRecord { FactorId = 7, Code = "no_accident_data", Category = "data", DisplayName = "No accident history", ExplanationTemplate = "There are no recent accident records for this stretch, so risk cannot be scored reliably." },
                new RiskFactorRecord { FactorId = 8, Code = "no_weather_data", Category = "data", DisplayName = "No weather data", ExplanationTemplate = "Weather information was unavailable for this stretch, so risk cannot be scored reliably." });
        });

        modelBuilder.Entity<AssessmentFactorRecord>(entity =>
        {
            entity.ToTable("assessment_factors", table => table.HasCheckConstraint("ck_assessment_factors_rank", "[rank_order] > 0"));
            entity.HasKey(x => new { x.AssessmentId, x.FactorId });
            entity.Property(x => x.Contribution).HasPrecision(6, 3);
            entity.Property(x => x.ExplanationText).HasMaxLength(500).IsRequired();
            entity.HasOne<SegmentRiskAssessmentRecord>().WithMany().HasForeignKey(x => x.AssessmentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<RiskFactorRecord>().WithMany().HasForeignKey(x => x.FactorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HighRiskCriteriaRecord>(entity =>
        {
            entity.ToTable("high_risk_criteria", table => table.HasCheckConstraint("ck_high_risk_criteria_score", "[min_risk_score] BETWEEN 0 AND 100"));
            entity.HasKey(x => x.CriteriaId);
            entity.Property(x => x.CriteriaId).UseIdentityColumn();
            entity.Property(x => x.MinRiskScore).HasPrecision(5, 2);
            entity.Property(x => x.MinWeatherSeverity).HasMaxLength(10).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(255);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne<WeatherSeverityRecord>().WithMany().HasForeignKey(x => x.MinWeatherSeverity).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<UserRecord>().WithMany().HasForeignKey(x => x.UpdatedBy).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<RouteWarningRecord>(entity =>
        {
            entity.ToTable("route_warnings");
            entity.HasKey(x => x.WarningId);
            entity.Property(x => x.WarningId).UseIdentityColumn();
            entity.Property(x => x.Message).HasMaxLength(500).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.RouteId);
            entity.HasOne<CandidateRouteRecord>().WithMany().HasForeignKey(x => x.RouteId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<RoadSegmentRecord>().WithMany().HasForeignKey(x => x.SegmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<HighRiskCriteriaRecord>().WithMany().HasForeignKey(x => x.CriteriaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RouteRiskLogRecord>(entity =>
        {
            entity.ToTable("route_risk_log");
            entity.HasKey(x => x.LogId);
            entity.Property(x => x.LogId).UseIdentityColumn();
            entity.Property(x => x.Snapshot).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(x => x.LoggedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.RequestId);
            entity.HasIndex(x => x.LoggedAt);
            entity.HasOne<RouteRequestRecord>().WithMany().HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<UserRecord>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<RiskModelVersionRecord>().WithMany().HasForeignKey(x => x.ModelId).OnDelete(DeleteBehavior.SetNull);
        });

    }

    private static string ToSnakeCase(string name)
    {
        var result = new System.Text.StringBuilder(name.Length + 8);
        for (var index = 0; index < name.Length; index++)
        {
            var character = name[index];
            if (char.IsUpper(character) && index > 0 && (char.IsLower(name[index - 1]) || (index + 1 < name.Length && char.IsLower(name[index + 1]))))
            {
                result.Append('_');
            }
            result.Append(char.ToLowerInvariant(character));
        }
        return result.ToString();
    }
}