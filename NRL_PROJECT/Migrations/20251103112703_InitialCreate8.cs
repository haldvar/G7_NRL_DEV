using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NRL_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ObstacleReports",
                table: "ObstacleReports");

            migrationBuilder.AlterColumn<int>(
                name: "ObstacleReportID",
                table: "ObstacleReports",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "Report_Id",
                table: "ObstacleReports",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<string>(
                name: "HandledBy",
                table: "ObstacleReports",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ReportStatus",
                table: "ObstacleReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Reported_Item",
                table: "ObstacleReports",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Reported_Location",
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

            migrationBuilder.AddColumn<DateTime>(
                name: "Time_of_Submitted_Report",
                table: "ObstacleReports",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObstacleReports",
                table: "ObstacleReports",
                column: "Report_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ObstacleReports",
                table: "ObstacleReports");

            migrationBuilder.DropColumn(
                name: "Report_Id",
                table: "ObstacleReports");

            migrationBuilder.DropColumn(
                name: "HandledBy",
                table: "ObstacleReports");

            migrationBuilder.DropColumn(
                name: "ReportStatus",
                table: "ObstacleReports");

            migrationBuilder.DropColumn(
                name: "Reported_Item",
                table: "ObstacleReports");

            migrationBuilder.DropColumn(
                name: "Reported_Location",
                table: "ObstacleReports");

            migrationBuilder.DropColumn(
                name: "StatusChangedAt",
                table: "ObstacleReports");

            migrationBuilder.DropColumn(
                name: "StatusComment",
                table: "ObstacleReports");

            migrationBuilder.DropColumn(
                name: "Time_of_Submitted_Report",
                table: "ObstacleReports");

            migrationBuilder.AlterColumn<int>(
                name: "ObstacleReportID",
                table: "ObstacleReports",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObstacleReports",
                table: "ObstacleReports",
                column: "ObstacleReportID");
        }
    }
}
