using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeRouteAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DomainAlignedInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accident_records",
                columns: table => new
                {
                    accident_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    date_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    accident_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    severity = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accident_records", x => x.accident_id);
                });

            migrationBuilder.CreateTable(
                name: "model_versions",
                columns: table => new
                {
                    model_version_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    version_label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    training_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dataset_version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    model_path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    validation_summary = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model_versions", x => x.model_version_id);
                });

            migrationBuilder.CreateTable(
                name: "road_segments",
                columns: table => new
                {
                    road_segment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    start_latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    start_longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    end_latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    end_longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    road_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    road_name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segments", x => x.road_segment_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date_registered = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "weather_logs",
                columns: table => new
                {
                    weather_log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    date_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    precipitation = table.Column<double>(type: "float", nullable: true),
                    visibility = table.Column<double>(type: "float", nullable: true),
                    wind_speed = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weather_logs", x => x.weather_log_id);
                });

            migrationBuilder.CreateTable(
                name: "community_reports",
                columns: table => new
                {
                    report_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    incident_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    report_date_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    reviewed_by_admin_id = table.Column<int>(type: "int", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_reports", x => x.report_id);
                    table.ForeignKey(
                        name: "FK_community_reports_users_reviewed_by_admin_id",
                        column: x => x.reviewed_by_admin_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_community_reports_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "routes",
                columns: table => new
                {
                    route_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    origin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    destination = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    request_date_time = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routes", x => x.route_id);
                    table.ForeignKey(
                        name: "FK_routes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "risk_assessments",
                columns: table => new
                {
                    risk_assessment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    road_segment_id = table.Column<int>(type: "int", nullable: false),
                    weather_log_id = table.Column<int>(type: "int", nullable: true),
                    risk_score = table.Column<double>(type: "float", nullable: true),
                    risk_level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    assessment_status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_assessments", x => x.risk_assessment_id);
                    table.ForeignKey(
                        name: "FK_risk_assessments_road_segments_road_segment_id",
                        column: x => x.road_segment_id,
                        principalTable: "road_segments",
                        principalColumn: "road_segment_id");
                    table.ForeignKey(
                        name: "FK_risk_assessments_weather_logs_weather_log_id",
                        column: x => x.weather_log_id,
                        principalTable: "weather_logs",
                        principalColumn: "weather_log_id");
                });

            migrationBuilder.CreateTable(
                name: "route_options",
                columns: table => new
                {
                    route_option_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    route_id = table.Column<int>(type: "int", nullable: false),
                    route_info = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    geometry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    distance = table.Column<double>(type: "float", nullable: false),
                    est_time_min = table.Column<double>(type: "float", nullable: false),
                    risk_score = table.Column<double>(type: "float", nullable: true),
                    rank = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_options", x => x.route_option_id);
                    table.ForeignKey(
                        name: "FK_route_options_routes_route_id",
                        column: x => x.route_id,
                        principalTable: "routes",
                        principalColumn: "route_id");
                });

            migrationBuilder.CreateTable(
                name: "route_risk_logs",
                columns: table => new
                {
                    route_risk_log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    route_id = table.Column<int>(type: "int", nullable: false),
                    logged_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    result_snapshot = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_risk_logs", x => x.route_risk_log_id);
                    table.ForeignKey(
                        name: "FK_route_risk_logs_routes_route_id",
                        column: x => x.route_id,
                        principalTable: "routes",
                        principalColumn: "route_id");
                });

            migrationBuilder.CreateTable(
                name: "risk_explanations",
                columns: table => new
                {
                    risk_explanation_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    risk_assessment_id = table.Column<int>(type: "int", nullable: false),
                    explanation_text = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_explanations", x => x.risk_explanation_id);
                    table.ForeignKey(
                        name: "FK_risk_explanations_risk_assessments_risk_assessment_id",
                        column: x => x.risk_assessment_id,
                        principalTable: "risk_assessments",
                        principalColumn: "risk_assessment_id");
                });

            migrationBuilder.CreateTable(
                name: "route_option_road_segments",
                columns: table => new
                {
                    route_option_id = table.Column<int>(type: "int", nullable: false),
                    road_segment_id = table.Column<int>(type: "int", nullable: false),
                    segment_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_option_road_segments", x => new { x.route_option_id, x.road_segment_id });
                    table.ForeignKey(
                        name: "FK_route_option_road_segments_road_segments_road_segment_id",
                        column: x => x.road_segment_id,
                        principalTable: "road_segments",
                        principalColumn: "road_segment_id");
                    table.ForeignKey(
                        name: "FK_route_option_road_segments_route_options_route_option_id",
                        column: x => x.route_option_id,
                        principalTable: "route_options",
                        principalColumn: "route_option_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_community_reports_reviewed_by_admin_id",
                table: "community_reports",
                column: "reviewed_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_community_reports_user_id",
                table: "community_reports",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_risk_assessments_road_segment_id",
                table: "risk_assessments",
                column: "road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_risk_assessments_weather_log_id",
                table: "risk_assessments",
                column: "weather_log_id");

            migrationBuilder.CreateIndex(
                name: "IX_risk_explanations_risk_assessment_id",
                table: "risk_explanations",
                column: "risk_assessment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_route_option_road_segments_road_segment_id",
                table: "route_option_road_segments",
                column: "road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_options_route_id",
                table: "route_options",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_risk_logs_route_id",
                table: "route_risk_logs",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "IX_routes_user_id",
                table: "routes",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accident_records");

            migrationBuilder.DropTable(
                name: "community_reports");

            migrationBuilder.DropTable(
                name: "model_versions");

            migrationBuilder.DropTable(
                name: "risk_explanations");

            migrationBuilder.DropTable(
                name: "route_option_road_segments");

            migrationBuilder.DropTable(
                name: "route_risk_logs");

            migrationBuilder.DropTable(
                name: "risk_assessments");

            migrationBuilder.DropTable(
                name: "route_options");

            migrationBuilder.DropTable(
                name: "road_segments");

            migrationBuilder.DropTable(
                name: "weather_logs");

            migrationBuilder.DropTable(
                name: "routes");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
