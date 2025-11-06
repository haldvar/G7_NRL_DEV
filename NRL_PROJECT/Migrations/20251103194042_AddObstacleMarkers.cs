using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NRL_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class AddObstacleMarkers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ObstacleMarkers",
                columns: table => new
                {
                    ObstacleMarkerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ObstacleID = table.Column<int>(type: "int", nullable: false),
                    MarkerNo = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObstacleMarkers", x => x.ObstacleMarkerID);
                    table.ForeignKey(
                        name: "FK_ObstacleMarkers_Obstacles_ObstacleID",
                        column: x => x.ObstacleID,
                        principalTable: "Obstacles",
                        principalColumn: "ObstacleId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ObstacleMarkers_ObstacleID",
                table: "ObstacleMarkers",
                column: "ObstacleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObstacleMarkers");
        }
    }
}
