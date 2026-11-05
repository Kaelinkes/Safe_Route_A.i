using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeRouteAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHumidityToWeatherLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "humidity",
                table: "weather_logs",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "humidity",
                table: "weather_logs");
        }
    }
}
