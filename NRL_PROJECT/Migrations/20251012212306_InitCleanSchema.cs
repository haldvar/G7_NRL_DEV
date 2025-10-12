using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NRL_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class InitCleanSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Obstacles",
                columns: table => new
                {
                    ObstacleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ObstacleName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ObstacleType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ObstacleHeight = table.Column<int>(type: "int", nullable: false),
                    ObstacleWidth = table.Column<int>(type: "int", nullable: false),
                    ObstacleDescription = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    Latitude = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Obstacles", x => x.ObstacleId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ObstacleReports",
                columns: table => new
                {
                    Report_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Reported_Item = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Reported_Location = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Time_of_Submitted_Report = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ObstacleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObstacleReports", x => x.Report_Id);
                    table.ForeignKey(
                        name: "FK_ObstacleReports_Obstacles_ObstacleId",
                        column: x => x.ObstacleId,
                        principalTable: "Obstacles",
                        principalColumn: "ObstacleId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ObstacleReports_ObstacleId",
                table: "ObstacleReports",
                column: "ObstacleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObstacleReports");

            migrationBuilder.DropTable(
                name: "Obstacles");
        }
    }
}
