using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeRouteAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllowAnonymousRoutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "user_id",
                table: "routes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM [routes] WHERE [user_id] IS NULL)
                    THROW 50001, 'Cannot make routes.user_id required while anonymous routes exist.', 1;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "user_id",
                table: "routes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
