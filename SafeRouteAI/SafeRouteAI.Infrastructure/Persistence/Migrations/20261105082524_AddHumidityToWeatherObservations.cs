using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeRouteAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHumidityToWeatherObservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_weather_measurements",
                table: "weather_observations");

            migrationBuilder.AddColumn<decimal>(
                name: "humidity_percent",
                table: "weather_observations",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_weather_measurements",
                table: "weather_observations",
                sql: "([precipitation_mm] IS NULL OR [precipitation_mm] >= 0) AND ([visibility_m] IS NULL OR [visibility_m] >= 0) AND ([wind_speed_kmh] IS NULL OR [wind_speed_kmh] >= 0) AND ([humidity_percent] IS NULL OR [humidity_percent] BETWEEN 0 AND 100)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_weather_measurements",
                table: "weather_observations");

            migrationBuilder.DropColumn(
                name: "humidity_percent",
                table: "weather_observations");

            migrationBuilder.AddCheckConstraint(
                name: "ck_weather_measurements",
                table: "weather_observations",
                sql: "([precipitation_mm] IS NULL OR [precipitation_mm] >= 0) AND ([visibility_m] IS NULL OR [visibility_m] >= 0) AND ([wind_speed_kmh] IS NULL OR [wind_speed_kmh] >= 0)");
        }
    }
}
