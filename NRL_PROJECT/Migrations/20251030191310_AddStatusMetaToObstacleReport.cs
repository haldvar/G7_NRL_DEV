using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NRL_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusMetaToObstacleReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HandledBy",
                table: "ObstacleReports",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusChangedAt",
                table: "ObstacleReports",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusComment",
                table: "ObstacleReports",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HandledBy",
                table: "ObstacleReports");

            migrationBuilder.DropColumn(
                name: "StatusChangedAt",
                table: "ObstacleReports");

            migrationBuilder.DropColumn(
                name: "StatusComment",
                table: "ObstacleReports");
        }
    }
}
