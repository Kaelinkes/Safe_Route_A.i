using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeRouteAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReportingViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE VIEW [dbo].[v_active_community_reports]
                AS
                SELECT r.[report_id], r.[user_id], r.[incident_type_id], r.[segment_id],
                       r.[latitude], r.[longitude], r.[description], r.[reported_at], r.[expires_at],
                       t.[code] AS [incident_code], t.[risk_weight]
                FROM [dbo].[community_reports] AS r
                INNER JOIN [dbo].[incident_types] AS t ON t.[incident_type_id] = r.[incident_type_id]
                WHERE r.[status_code] = 'approved'
                  AND r.[expires_at] > SYSUTCDATETIME();
                """);

            migrationBuilder.Sql("""
                CREATE VIEW [dbo].[v_ranked_routes]
                AS
                SELECT [request_id], [route_id], [summary], [overall_rank],
                       ROUND([duration_s] / 60.0, 1) AS [duration_min],
                       ROUND([distance_m] / 1000.0, 2) AS [distance_km],
                       [overall_risk_score], [overall_risk_category], [is_high_risk]
                FROM [dbo].[candidate_routes];
                """);

            migrationBuilder.Sql("""
                CREATE VIEW [dbo].[v_daily_route_stats]
                AS
                SELECT CAST([requested_at] AS date) AS [day],
                       COUNT(*) AS [requests],
                       SUM(CASE WHEN [status_code] = 'failed' THEN 1 ELSE 0 END) AS [failed_requests],
                       AVG(CAST([processing_ms] AS decimal(18, 2))) AS [avg_processing_ms]
                FROM [dbo].[route_requests]
                GROUP BY CAST([requested_at] AS date);
                """);

            migrationBuilder.Sql("""
                CREATE VIEW [dbo].[v_risk_distribution]
                AS
                SELECT [overall_risk_category],
                       COUNT(*) AS [routes],
                       SUM(CASE WHEN [is_high_risk] = 1 THEN 1 ELSE 0 END) AS [flagged_high_risk]
                FROM [dbo].[candidate_routes]
                GROUP BY [overall_risk_category];
                """);

            migrationBuilder.Sql("""
                CREATE VIEW [dbo].[v_report_moderation_stats]
                AS
                SELECT t.[code] AS [incident_type], r.[status_code], COUNT(*) AS [reports]
                FROM [dbo].[community_reports] AS r
                INNER JOIN [dbo].[incident_types] AS t ON t.[incident_type_id] = r.[incident_type_id]
                GROUP BY t.[code], r.[status_code];
                """);

            migrationBuilder.Sql("""
                CREATE VIEW [dbo].[v_external_api_health]
                AS
                SELECT [service_code], [provider],
                       COUNT(*) AS [calls],
                       ROUND(100.0 * SUM(CASE WHEN [success] = 1 THEN 1.0 ELSE 0.0 END) / NULLIF(COUNT(*), 0), 2) AS [success_pct],
                       AVG([latency_ms]) AS [avg_latency_ms]
                FROM [dbo].[external_api_calls]
                GROUP BY [service_code], [provider];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW [dbo].[v_external_api_health];");
            migrationBuilder.Sql("DROP VIEW [dbo].[v_report_moderation_stats];");
            migrationBuilder.Sql("DROP VIEW [dbo].[v_risk_distribution];");
            migrationBuilder.Sql("DROP VIEW [dbo].[v_daily_route_stats];");
            migrationBuilder.Sql("DROP VIEW [dbo].[v_ranked_routes];");
            migrationBuilder.Sql("DROP VIEW [dbo].[v_active_community_reports];");
        }
    }
}
