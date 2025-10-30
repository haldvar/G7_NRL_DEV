using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NRL_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeoJsonCoordinates",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "MapViewID",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "ObstacleDataID",
                table: "Obstacles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeoJsonCoordinates",
                table: "Obstacles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "MapViewID",
                table: "Obstacles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ObstacleDataID",
                table: "Obstacles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
