using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeRouteAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ContextQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_routes_user_id",
                table: "routes");

            migrationBuilder.DropIndex(
                name: "IX_route_risk_logs_route_id",
                table: "route_risk_logs");

            migrationBuilder.DropIndex(
                name: "IX_route_options_route_id",
                table: "route_options");

            migrationBuilder.CreateIndex(
                name: "IX_weather_logs_latitude_longitude_date_time",
                table: "weather_logs",
                columns: new[] { "latitude", "longitude", "date_time" });

            migrationBuilder.CreateIndex(
                name: "IX_routes_user_id_request_date_time",
                table: "routes",
                columns: new[] { "user_id", "request_date_time" });

            migrationBuilder.CreateIndex(
                name: "IX_route_risk_logs_route_id_logged_at",
                table: "route_risk_logs",
                columns: new[] { "route_id", "logged_at" });

            migrationBuilder.CreateIndex(
                name: "IX_route_options_route_id_rank",
                table: "route_options",
                columns: new[] { "route_id", "rank" });

            migrationBuilder.CreateIndex(
                name: "IX_route_option_road_segments_route_option_id_segment_order",
                table: "route_option_road_segments",
                columns: new[] { "route_option_id", "segment_order" });

            migrationBuilder.CreateIndex(
                name: "IX_community_reports_report_date_time",
                table: "community_reports",
                column: "report_date_time");

            migrationBuilder.CreateIndex(
                name: "IX_accident_records_latitude_longitude_date_time",
                table: "accident_records",
                columns: new[] { "latitude", "longitude", "date_time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_weather_logs_latitude_longitude_date_time",
                table: "weather_logs");

            migrationBuilder.DropIndex(
                name: "IX_routes_user_id_request_date_time",
                table: "routes");

            migrationBuilder.DropIndex(
                name: "IX_route_risk_logs_route_id_logged_at",
                table: "route_risk_logs");

            migrationBuilder.DropIndex(
                name: "IX_route_options_route_id_rank",
                table: "route_options");

            migrationBuilder.DropIndex(
                name: "IX_route_option_road_segments_route_option_id_segment_order",
                table: "route_option_road_segments");

            migrationBuilder.DropIndex(
                name: "IX_community_reports_report_date_time",
                table: "community_reports");

            migrationBuilder.DropIndex(
                name: "IX_accident_records_latitude_longitude_date_time",
                table: "accident_records");

            migrationBuilder.CreateIndex(
                name: "IX_routes_user_id",
                table: "routes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_risk_logs_route_id",
                table: "route_risk_logs",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_options_route_id",
                table: "route_options",
                column: "route_id");
        }
    }
}
