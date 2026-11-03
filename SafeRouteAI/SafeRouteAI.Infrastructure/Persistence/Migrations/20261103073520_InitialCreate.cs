using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafeRouteAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "incident_types",
                columns: table => new
                {
                    incident_type_id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    default_ttl_minutes = table.Column<int>(type: "int", nullable: false, defaultValue: 240),
                    risk_weight = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false, defaultValue: 1m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incident_types", x => x.incident_type_id);
                    table.CheckConstraint("ck_incident_types_ttl", "[default_ttl_minutes] > 0");
                    table.CheckConstraint("ck_incident_types_weight", "[risk_weight] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "ref_account_status",
                columns: table => new
                {
                    status_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_account_status", x => x.status_code);
                });

            migrationBuilder.CreateTable(
                name: "ref_external_service",
                columns: table => new
                {
                    service_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_external_service", x => x.service_code);
                });

            migrationBuilder.CreateTable(
                name: "ref_report_status",
                columns: table => new
                {
                    status_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_report_status", x => x.status_code);
                });

            migrationBuilder.CreateTable(
                name: "ref_request_status",
                columns: table => new
                {
                    status_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_request_status", x => x.status_code);
                });

            migrationBuilder.CreateTable(
                name: "ref_risk_category",
                columns: table => new
                {
                    category_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    sort_order = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_risk_category", x => x.category_code);
                });

            migrationBuilder.CreateTable(
                name: "ref_user_role",
                columns: table => new
                {
                    role_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_user_role", x => x.role_code);
                });

            migrationBuilder.CreateTable(
                name: "ref_weather_severity",
                columns: table => new
                {
                    severity_code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    severity_level = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_weather_severity", x => x.severity_code);
                });

            migrationBuilder.CreateTable(
                name: "risk_factors",
                columns: table => new
                {
                    factor_id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    display_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    explanation_template = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_factors", x => x.factor_id);
                    table.CheckConstraint("ck_risk_factors_category", "[category] IN ('weather', 'historical', 'community', 'road', 'time', 'data')");
                });

            migrationBuilder.CreateTable(
                name: "risk_model_versions",
                columns: table => new
                {
                    model_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    version = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    trained_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    training_data_from = table.Column<DateOnly>(type: "date", nullable: true),
                    training_data_to = table.Column<DateOnly>(type: "date", nullable: true),
                    metrics = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    model_artifact_uri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    artifact_checksum = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    min_data_points = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    active_flag = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_model_versions", x => x.model_id);
                    table.CheckConstraint("ck_model_active_flag", "[active_flag] IS NULL OR [active_flag] = 'Y'");
                    table.CheckConstraint("ck_model_min_data_points", "[min_data_points] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "road_segments",
                columns: table => new
                {
                    segment_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    external_ref = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    road_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    road_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    speed_limit_kmh = table.Column<short>(type: "smallint", nullable: true),
                    lanes = table.Column<short>(type: "smallint", nullable: true),
                    length_m = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    start_lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    start_lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    end_lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    end_lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    min_lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    max_lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    min_lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    max_lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    polyline = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segments", x => x.segment_id);
                    table.CheckConstraint("ck_road_segments_bbox", "[min_lat] <= [max_lat] AND [min_lng] <= [max_lng]");
                    table.CheckConstraint("ck_road_segments_end_lat", "[end_lat] BETWEEN -90 AND 90");
                    table.CheckConstraint("ck_road_segments_end_lng", "[end_lng] BETWEEN -180 AND 180");
                    table.CheckConstraint("ck_road_segments_lanes", "[lanes] IS NULL OR [lanes] > 0");
                    table.CheckConstraint("ck_road_segments_length", "[length_m] > 0");
                    table.CheckConstraint("ck_road_segments_speed", "[speed_limit_kmh] IS NULL OR [speed_limit_kmh] > 0");
                    table.CheckConstraint("ck_road_segments_start_lat", "[start_lat] BETWEEN -90 AND 90");
                    table.CheckConstraint("ck_road_segments_start_lng", "[start_lng] BETWEEN -180 AND 180");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email_normalized = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    role_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "user"),
                    status_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "active"),
                    failed_logins = table.Column<short>(type: "smallint", nullable: false),
                    locked_until = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    last_login_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                    table.CheckConstraint("ck_users_failed_logins", "[failed_logins] >= 0");
                    table.ForeignKey(
                        name: "FK_users_ref_account_status_status_code",
                        column: x => x.status_code,
                        principalTable: "ref_account_status",
                        principalColumn: "status_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_users_ref_user_role_role_code",
                        column: x => x.role_code,
                        principalTable: "ref_user_role",
                        principalColumn: "role_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "weather_observations",
                columns: table => new
                {
                    weather_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    valid_from = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    valid_to = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    is_forecast = table.Column<bool>(type: "bit", nullable: false),
                    condition_text = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    temperature_c = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: true),
                    precipitation_mm = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    visibility_m = table.Column<int>(type: "int", nullable: true),
                    wind_speed_kmh = table.Column<decimal>(type: "decimal(5,1)", precision: 5, scale: 1, nullable: true),
                    severity_code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    fetched_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    raw_payload = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weather_observations", x => x.weather_id);
                    table.CheckConstraint("ck_weather_coordinates", "[latitude] BETWEEN -90 AND 90 AND [longitude] BETWEEN -180 AND 180");
                    table.CheckConstraint("ck_weather_measurements", "([precipitation_mm] IS NULL OR [precipitation_mm] >= 0) AND ([visibility_m] IS NULL OR [visibility_m] >= 0) AND ([wind_speed_kmh] IS NULL OR [wind_speed_kmh] >= 0)");
                    table.CheckConstraint("ck_weather_window", "[valid_to] > [valid_from]");
                    table.ForeignKey(
                        name: "FK_weather_observations_ref_weather_severity_severity_code",
                        column: x => x.severity_code,
                        principalTable: "ref_weather_severity",
                        principalColumn: "severity_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "historical_accidents",
                columns: table => new
                {
                    accident_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    segment_id = table.Column<long>(type: "bigint", nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    severity = table.Column<short>(type: "smallint", nullable: true),
                    vehicles_involved = table.Column<short>(type: "smallint", nullable: true),
                    casualties = table.Column<short>(type: "smallint", nullable: true),
                    weather_condition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    light_condition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historical_accidents", x => x.accident_id);
                    table.CheckConstraint("ck_accidents_casualties", "[casualties] IS NULL OR [casualties] >= 0");
                    table.CheckConstraint("ck_accidents_latitude", "[latitude] BETWEEN -90 AND 90");
                    table.CheckConstraint("ck_accidents_longitude", "[longitude] BETWEEN -180 AND 180");
                    table.CheckConstraint("ck_accidents_severity", "[severity] IS NULL OR [severity] BETWEEN 1 AND 5");
                    table.CheckConstraint("ck_accidents_vehicles", "[vehicles_involved] IS NULL OR [vehicles_involved] >= 0");
                    table.ForeignKey(
                        name: "FK_historical_accidents_road_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "road_segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "community_reports",
                columns: table => new
                {
                    report_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    incident_type_id = table.Column<short>(type: "smallint", nullable: false),
                    segment_id = table.Column<long>(type: "bigint", nullable: true),
                    latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    status_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    reported_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    expires_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    moderated_by = table.Column<long>(type: "bigint", nullable: true),
                    moderated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    moderation_note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_reports", x => x.report_id);
                    table.CheckConstraint("ck_community_reports_coordinates", "[latitude] BETWEEN -90 AND 90 AND [longitude] BETWEEN -180 AND 180");
                    table.CheckConstraint("ck_community_reports_description", "LEN([description] + '#') - 1 >= 5");
                    table.CheckConstraint("ck_community_reports_expiry", "[expires_at] > [reported_at]");
                    table.CheckConstraint("ck_community_reports_moderation", "[status_code] IN ('pending', 'expired') OR ([moderated_by] IS NOT NULL AND [moderated_at] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_community_reports_incident_types_incident_type_id",
                        column: x => x.incident_type_id,
                        principalTable: "incident_types",
                        principalColumn: "incident_type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_community_reports_ref_report_status_status_code",
                        column: x => x.status_code,
                        principalTable: "ref_report_status",
                        principalColumn: "status_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_community_reports_road_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "road_segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_community_reports_users_moderated_by",
                        column: x => x.moderated_by,
                        principalTable: "users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_community_reports_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "high_risk_criteria",
                columns: table => new
                {
                    criteria_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    min_risk_score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    min_weather_severity = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_high_risk_criteria", x => x.criteria_id);
                    table.CheckConstraint("ck_high_risk_criteria_score", "[min_risk_score] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_high_risk_criteria_ref_weather_severity_min_weather_severity",
                        column: x => x.min_weather_severity,
                        principalTable: "ref_weather_severity",
                        principalColumn: "severity_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_high_risk_criteria_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "login_attempts",
                columns: table => new
                {
                    attempt_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    email_tried = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    success = table.Column<bool>(type: "bit", nullable: false),
                    failure_reason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    attempted_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login_attempts", x => x.attempt_id);
                    table.ForeignKey(
                        name: "FK_login_attempts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "route_requests",
                columns: table => new
                {
                    request_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    origin_label = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    origin_lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    origin_lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    destination_label = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    destination_lat = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    destination_lng = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    planned_departure = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    status_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    error_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requested_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    completed_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    processing_ms = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_requests", x => x.request_id);
                    table.CheckConstraint("ck_route_requests_destination_lat", "[destination_lat] BETWEEN -90 AND 90");
                    table.CheckConstraint("ck_route_requests_destination_lng", "[destination_lng] BETWEEN -180 AND 180");
                    table.CheckConstraint("ck_route_requests_distinct_points", "[origin_lat] <> [destination_lat] OR [origin_lng] <> [destination_lng]");
                    table.CheckConstraint("ck_route_requests_origin_lat", "[origin_lat] BETWEEN -90 AND 90");
                    table.CheckConstraint("ck_route_requests_origin_lng", "[origin_lng] BETWEEN -180 AND 180");
                    table.CheckConstraint("ck_route_requests_processing_ms", "[processing_ms] IS NULL OR [processing_ms] >= 0");
                    table.ForeignKey(
                        name: "FK_route_requests_ref_request_status_status_code",
                        column: x => x.status_code,
                        principalTable: "ref_request_status",
                        principalColumn: "status_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_route_requests_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    session_id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    expires_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_sessions", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_user_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_status_history",
                columns: table => new
                {
                    history_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    report_id = table.Column<long>(type: "bigint", nullable: false),
                    old_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    new_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<long>(type: "bigint", nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    changed_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_status_history", x => x.history_id);
                    table.ForeignKey(
                        name: "FK_report_status_history_community_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "community_reports",
                        principalColumn: "report_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_status_history_ref_report_status_new_status",
                        column: x => x.new_status,
                        principalTable: "ref_report_status",
                        principalColumn: "status_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_report_status_history_ref_report_status_old_status",
                        column: x => x.old_status,
                        principalTable: "ref_report_status",
                        principalColumn: "status_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_report_status_history_users_changed_by",
                        column: x => x.changed_by,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "candidate_routes",
                columns: table => new
                {
                    route_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    request_id = table.Column<long>(type: "bigint", nullable: false),
                    provider_route_ref = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    summary = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    distance_m = table.Column<decimal>(type: "decimal(10,1)", precision: 10, scale: 1, nullable: false),
                    duration_s = table.Column<int>(type: "int", nullable: false),
                    polyline = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    overall_risk_score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    overall_risk_category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    max_segment_risk_score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    is_high_risk = table.Column<bool>(type: "bit", nullable: false),
                    high_risk_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    rank_by_time = table.Column<short>(type: "smallint", nullable: true),
                    rank_by_risk = table.Column<short>(type: "smallint", nullable: true),
                    overall_rank = table.Column<short>(type: "smallint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candidate_routes", x => x.route_id);
                    table.CheckConstraint("ck_candidate_routes_distance", "[distance_m] > 0");
                    table.CheckConstraint("ck_candidate_routes_duration", "[duration_s] > 0");
                    table.CheckConstraint("ck_candidate_routes_rank", "([rank_by_time] IS NULL OR [rank_by_time] > 0) AND ([rank_by_risk] IS NULL OR [rank_by_risk] > 0) AND ([overall_rank] IS NULL OR [overall_rank] > 0)");
                    table.CheckConstraint("ck_candidate_routes_risk", "([overall_risk_score] IS NULL OR [overall_risk_score] BETWEEN 0 AND 100) AND ([max_segment_risk_score] IS NULL OR [max_segment_risk_score] BETWEEN 0 AND 100)");
                    table.ForeignKey(
                        name: "FK_candidate_routes_ref_risk_category_overall_risk_category",
                        column: x => x.overall_risk_category,
                        principalTable: "ref_risk_category",
                        principalColumn: "category_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_candidate_routes_route_requests_request_id",
                        column: x => x.request_id,
                        principalTable: "route_requests",
                        principalColumn: "request_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "external_api_calls",
                columns: table => new
                {
                    call_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    request_id = table.Column<long>(type: "bigint", nullable: true),
                    service_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    http_status = table.Column<short>(type: "smallint", nullable: true),
                    success = table.Column<bool>(type: "bit", nullable: false),
                    latency_ms = table.Column<int>(type: "int", nullable: true),
                    error_detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    called_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_api_calls", x => x.call_id);
                    table.CheckConstraint("ck_external_api_calls_latency", "[latency_ms] IS NULL OR [latency_ms] >= 0");
                    table.ForeignKey(
                        name: "FK_external_api_calls_ref_external_service_service_code",
                        column: x => x.service_code,
                        principalTable: "ref_external_service",
                        principalColumn: "service_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_external_api_calls_route_requests_request_id",
                        column: x => x.request_id,
                        principalTable: "route_requests",
                        principalColumn: "request_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "route_risk_log",
                columns: table => new
                {
                    log_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    request_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    model_id = table.Column<int>(type: "int", nullable: true),
                    snapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    logged_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_risk_log", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_route_risk_log_risk_model_versions_model_id",
                        column: x => x.model_id,
                        principalTable: "risk_model_versions",
                        principalColumn: "model_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_route_risk_log_route_requests_request_id",
                        column: x => x.request_id,
                        principalTable: "route_requests",
                        principalColumn: "request_id");
                    table.ForeignKey(
                        name: "FK_route_risk_log_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "route_segments",
                columns: table => new
                {
                    route_id = table.Column<long>(type: "bigint", nullable: false),
                    seq_no = table.Column<int>(type: "int", nullable: false),
                    segment_id = table.Column<long>(type: "bigint", nullable: false),
                    estimated_travel_time_s = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_segments", x => new { x.route_id, x.seq_no });
                    table.CheckConstraint("ck_route_segments_sequence", "[seq_no] > 0");
                    table.CheckConstraint("ck_route_segments_travel_time", "[estimated_travel_time_s] IS NULL OR [estimated_travel_time_s] >= 0");
                    table.ForeignKey(
                        name: "FK_route_segments_candidate_routes_route_id",
                        column: x => x.route_id,
                        principalTable: "candidate_routes",
                        principalColumn: "route_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_route_segments_road_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "road_segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "route_warnings",
                columns: table => new
                {
                    warning_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    route_id = table.Column<long>(type: "bigint", nullable: false),
                    segment_id = table.Column<long>(type: "bigint", nullable: true),
                    criteria_id = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_warnings", x => x.warning_id);
                    table.ForeignKey(
                        name: "FK_route_warnings_candidate_routes_route_id",
                        column: x => x.route_id,
                        principalTable: "candidate_routes",
                        principalColumn: "route_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_route_warnings_high_risk_criteria_criteria_id",
                        column: x => x.criteria_id,
                        principalTable: "high_risk_criteria",
                        principalColumn: "criteria_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_route_warnings_road_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "road_segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "segment_risk_assessments",
                columns: table => new
                {
                    assessment_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    route_id = table.Column<long>(type: "bigint", nullable: false),
                    segment_id = table.Column<long>(type: "bigint", nullable: false),
                    model_id = table.Column<int>(type: "int", nullable: false),
                    weather_id = table.Column<long>(type: "bigint", nullable: true),
                    risk_score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    category_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    data_sufficient = table.Column<bool>(type: "bit", nullable: false),
                    has_accident_data = table.Column<bool>(type: "bit", nullable: false),
                    has_weather_data = table.Column<bool>(type: "bit", nullable: false),
                    has_community_data = table.Column<bool>(type: "bit", nullable: false),
                    explanation_summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    assessed_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_segment_risk_assessments", x => x.assessment_id);
                    table.CheckConstraint("ck_assessment_risk_score", "[risk_score] IS NULL OR [risk_score] BETWEEN 0 AND 100");
                    table.CheckConstraint("ck_assessment_score_sufficiency", "([data_sufficient] = 1 AND [risk_score] IS NOT NULL AND [category_code] <> 'insufficient_data') OR ([data_sufficient] = 0 AND [risk_score] IS NULL AND [category_code] = 'insufficient_data')");
                    table.ForeignKey(
                        name: "FK_segment_risk_assessments_candidate_routes_route_id",
                        column: x => x.route_id,
                        principalTable: "candidate_routes",
                        principalColumn: "route_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_segment_risk_assessments_ref_risk_category_category_code",
                        column: x => x.category_code,
                        principalTable: "ref_risk_category",
                        principalColumn: "category_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_segment_risk_assessments_risk_model_versions_model_id",
                        column: x => x.model_id,
                        principalTable: "risk_model_versions",
                        principalColumn: "model_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_segment_risk_assessments_road_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "road_segments",
                        principalColumn: "segment_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_segment_risk_assessments_weather_observations_weather_id",
                        column: x => x.weather_id,
                        principalTable: "weather_observations",
                        principalColumn: "weather_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "assessment_community_reports",
                columns: table => new
                {
                    assessment_id = table.Column<long>(type: "bigint", nullable: false),
                    report_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessment_community_reports", x => new { x.assessment_id, x.report_id });
                    table.ForeignKey(
                        name: "FK_assessment_community_reports_community_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "community_reports",
                        principalColumn: "report_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assessment_community_reports_segment_risk_assessments_assessment_id",
                        column: x => x.assessment_id,
                        principalTable: "segment_risk_assessments",
                        principalColumn: "assessment_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assessment_factors",
                columns: table => new
                {
                    assessment_id = table.Column<long>(type: "bigint", nullable: false),
                    factor_id = table.Column<short>(type: "smallint", nullable: false),
                    contribution = table.Column<decimal>(type: "decimal(6,3)", precision: 6, scale: 3, nullable: false),
                    rank_order = table.Column<short>(type: "smallint", nullable: false),
                    explanation_text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessment_factors", x => new { x.assessment_id, x.factor_id });
                    table.CheckConstraint("ck_assessment_factors_rank", "[rank_order] > 0");
                    table.ForeignKey(
                        name: "FK_assessment_factors_risk_factors_factor_id",
                        column: x => x.factor_id,
                        principalTable: "risk_factors",
                        principalColumn: "factor_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assessment_factors_segment_risk_assessments_assessment_id",
                        column: x => x.assessment_id,
                        principalTable: "segment_risk_assessments",
                        principalColumn: "assessment_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "incident_types",
                columns: new[] { "incident_type_id", "code", "default_ttl_minutes", "label", "risk_weight" },
                values: new object[,]
                {
                    { (short)1, "collision", 180, "Collision / crash", 1.50m },
                    { (short)2, "breakdown", 90, "Broken-down vehicle", 0.80m },
                    { (short)3, "pothole", 1440, "Pothole / road damage", 0.60m },
                    { (short)4, "flooding", 360, "Flooding", 1.40m },
                    { (short)5, "debris", 120, "Debris on road", 0.90m },
                    { (short)6, "roadworks", 1440, "Roadworks", 0.50m },
                    { (short)7, "signal_fail", 240, "Traffic signal failure", 1.00m }
                });

            migrationBuilder.InsertData(
                table: "ref_account_status",
                column: "status_code",
                values: new object[]
                {
                    "active",
                    "deleted",
                    "suspended"
                });

            migrationBuilder.InsertData(
                table: "ref_external_service",
                column: "service_code",
                values: new object[]
                {
                    "routing",
                    "weather"
                });

            migrationBuilder.InsertData(
                table: "ref_report_status",
                column: "status_code",
                values: new object[]
                {
                    "approved",
                    "expired",
                    "pending",
                    "rejected",
                    "resolved"
                });

            migrationBuilder.InsertData(
                table: "ref_request_status",
                column: "status_code",
                values: new object[]
                {
                    "assessing",
                    "completed",
                    "failed",
                    "pending",
                    "routing"
                });

            migrationBuilder.InsertData(
                table: "ref_risk_category",
                columns: new[] { "category_code", "sort_order" },
                values: new object[,]
                {
                    { "high", (short)3 },
                    { "insufficient_data", (short)4 },
                    { "low", (short)1 },
                    { "moderate", (short)2 }
                });

            migrationBuilder.InsertData(
                table: "ref_user_role",
                column: "role_code",
                values: new object[]
                {
                    "administrator",
                    "user"
                });

            migrationBuilder.InsertData(
                table: "ref_weather_severity",
                columns: new[] { "severity_code", "severity_level" },
                values: new object[,]
                {
                    { "fair", (short)2 },
                    { "good", (short)1 },
                    { "poor", (short)3 },
                    { "severe", (short)4 }
                });

            migrationBuilder.InsertData(
                table: "risk_factors",
                columns: new[] { "factor_id", "category", "code", "display_name", "explanation_template" },
                values: new object[,]
                {
                    { (short)1, "weather", "heavy_rain", "Heavy rain", "Heavy rain reduces visibility and increases braking distance." },
                    { (short)2, "weather", "low_visibility", "Low visibility", "Fog or low light makes hazards harder to see." },
                    { (short)3, "historical", "accident_hotspot", "Accident hotspot", "This stretch has a high number of past accidents." },
                    { (short)4, "community", "active_report", "Recent incident", "Drivers recently reported an incident here." },
                    { (short)5, "road", "high_speed_road", "High-speed road", "Higher speed limits increase crash severity." },
                    { (short)6, "time", "peak_hour", "Peak hours", "Traffic density is higher at this time of day." },
                    { (short)7, "data", "no_accident_data", "No accident history", "There are no recent accident records for this stretch, so risk cannot be scored reliably." },
                    { (short)8, "data", "no_weather_data", "No weather data", "Weather information was unavailable for this stretch, so risk cannot be scored reliably." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_assessment_community_reports_report_id",
                table: "assessment_community_reports",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_factors_factor_id",
                table: "assessment_factors",
                column: "factor_id");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_routes_overall_risk_category",
                table: "candidate_routes",
                column: "overall_risk_category");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_routes_request_id",
                table: "candidate_routes",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_routes_request_id_overall_rank",
                table: "candidate_routes",
                columns: new[] { "request_id", "overall_rank" },
                unique: true,
                filter: "[overall_rank] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_community_reports_incident_type_id",
                table: "community_reports",
                column: "incident_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_community_reports_latitude_longitude",
                table: "community_reports",
                columns: new[] { "latitude", "longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_community_reports_moderated_by",
                table: "community_reports",
                column: "moderated_by");

            migrationBuilder.CreateIndex(
                name: "IX_community_reports_segment_id",
                table: "community_reports",
                column: "segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_community_reports_status_code_expires_at",
                table: "community_reports",
                columns: new[] { "status_code", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "IX_community_reports_user_id",
                table: "community_reports",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_external_api_calls_request_id",
                table: "external_api_calls",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_external_api_calls_service_code",
                table: "external_api_calls",
                column: "service_code");

            migrationBuilder.CreateIndex(
                name: "IX_high_risk_criteria_min_weather_severity",
                table: "high_risk_criteria",
                column: "min_weather_severity");

            migrationBuilder.CreateIndex(
                name: "IX_high_risk_criteria_updated_by",
                table: "high_risk_criteria",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_historical_accidents_latitude_longitude",
                table: "historical_accidents",
                columns: new[] { "latitude", "longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_historical_accidents_segment_id_occurred_at",
                table: "historical_accidents",
                columns: new[] { "segment_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_incident_types_code",
                table: "incident_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_login_attempts_user_id_attempted_at",
                table: "login_attempts",
                columns: new[] { "user_id", "attempted_at" });

            migrationBuilder.CreateIndex(
                name: "IX_ref_weather_severity_severity_level",
                table: "ref_weather_severity",
                column: "severity_level",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_status_history_changed_by",
                table: "report_status_history",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "IX_report_status_history_new_status",
                table: "report_status_history",
                column: "new_status");

            migrationBuilder.CreateIndex(
                name: "IX_report_status_history_old_status",
                table: "report_status_history",
                column: "old_status");

            migrationBuilder.CreateIndex(
                name: "IX_report_status_history_report_id",
                table: "report_status_history",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_risk_factors_code",
                table: "risk_factors",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_risk_model_versions_active_flag",
                table: "risk_model_versions",
                column: "active_flag",
                unique: true,
                filter: "[active_flag] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_risk_model_versions_name_version",
                table: "risk_model_versions",
                columns: new[] { "name", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_road_segments_external_ref",
                table: "road_segments",
                column: "external_ref",
                unique: true,
                filter: "[external_ref] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_road_segments_min_lat_max_lat_min_lng_max_lng",
                table: "road_segments",
                columns: new[] { "min_lat", "max_lat", "min_lng", "max_lng" });

            migrationBuilder.CreateIndex(
                name: "IX_route_requests_status_code",
                table: "route_requests",
                column: "status_code");

            migrationBuilder.CreateIndex(
                name: "IX_route_requests_user_id_requested_at",
                table: "route_requests",
                columns: new[] { "user_id", "requested_at" });

            migrationBuilder.CreateIndex(
                name: "IX_route_risk_log_logged_at",
                table: "route_risk_log",
                column: "logged_at");

            migrationBuilder.CreateIndex(
                name: "IX_route_risk_log_model_id",
                table: "route_risk_log",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_risk_log_request_id",
                table: "route_risk_log",
                column: "request_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_risk_log_user_id",
                table: "route_risk_log",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_segments_segment_id",
                table: "route_segments",
                column: "segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_warnings_criteria_id",
                table: "route_warnings",
                column: "criteria_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_warnings_route_id",
                table: "route_warnings",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_warnings_segment_id",
                table: "route_warnings",
                column: "segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_segment_risk_assessments_category_code",
                table: "segment_risk_assessments",
                column: "category_code");

            migrationBuilder.CreateIndex(
                name: "IX_segment_risk_assessments_model_id",
                table: "segment_risk_assessments",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "IX_segment_risk_assessments_route_id",
                table: "segment_risk_assessments",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "IX_segment_risk_assessments_route_id_segment_id_model_id",
                table: "segment_risk_assessments",
                columns: new[] { "route_id", "segment_id", "model_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_segment_risk_assessments_segment_id_assessed_at",
                table: "segment_risk_assessments",
                columns: new[] { "segment_id", "assessed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_segment_risk_assessments_weather_id",
                table: "segment_risk_assessments",
                column: "weather_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_token_hash",
                table: "user_sessions",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_user_id",
                table: "user_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email_normalized",
                table: "users",
                column: "email_normalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_role_code",
                table: "users",
                column: "role_code");

            migrationBuilder.CreateIndex(
                name: "IX_users_status_code",
                table: "users",
                column: "status_code");

            migrationBuilder.CreateIndex(
                name: "IX_weather_observations_latitude_longitude",
                table: "weather_observations",
                columns: new[] { "latitude", "longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_weather_observations_severity_code",
                table: "weather_observations",
                column: "severity_code");

            migrationBuilder.CreateIndex(
                name: "IX_weather_observations_valid_from_valid_to",
                table: "weather_observations",
                columns: new[] { "valid_from", "valid_to" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assessment_community_reports");

            migrationBuilder.DropTable(
                name: "assessment_factors");

            migrationBuilder.DropTable(
                name: "external_api_calls");

            migrationBuilder.DropTable(
                name: "historical_accidents");

            migrationBuilder.DropTable(
                name: "login_attempts");

            migrationBuilder.DropTable(
                name: "report_status_history");

            migrationBuilder.DropTable(
                name: "route_risk_log");

            migrationBuilder.DropTable(
                name: "route_segments");

            migrationBuilder.DropTable(
                name: "route_warnings");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "risk_factors");

            migrationBuilder.DropTable(
                name: "segment_risk_assessments");

            migrationBuilder.DropTable(
                name: "ref_external_service");

            migrationBuilder.DropTable(
                name: "community_reports");

            migrationBuilder.DropTable(
                name: "high_risk_criteria");

            migrationBuilder.DropTable(
                name: "candidate_routes");

            migrationBuilder.DropTable(
                name: "risk_model_versions");

            migrationBuilder.DropTable(
                name: "weather_observations");

            migrationBuilder.DropTable(
                name: "incident_types");

            migrationBuilder.DropTable(
                name: "ref_report_status");

            migrationBuilder.DropTable(
                name: "road_segments");

            migrationBuilder.DropTable(
                name: "ref_risk_category");

            migrationBuilder.DropTable(
                name: "route_requests");

            migrationBuilder.DropTable(
                name: "ref_weather_severity");

            migrationBuilder.DropTable(
                name: "ref_request_status");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "ref_account_status");

            migrationBuilder.DropTable(
                name: "ref_user_role");
        }
    }
}
