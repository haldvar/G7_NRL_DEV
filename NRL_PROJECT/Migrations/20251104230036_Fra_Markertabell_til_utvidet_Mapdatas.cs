using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NRL_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class Fra_Markertabell_til_utvidet_Mapdatas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObstacleMarkers");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "Obstacles",
                newName: "Longitude2");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "Obstacles",
                newName: "Longitude1");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "MapDatas",
                newName: "Longitude2");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "MapDatas",
                newName: "Longitude1");

            migrationBuilder.AddColumn<double>(
                name: "Latitude1",
                table: "Obstacles",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Latitude2",
                table: "Obstacles",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Latitude1",
                table: "MapDatas",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Latitude2",
                table: "MapDatas",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude1",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "Latitude2",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "Latitude1",
                table: "MapDatas");

            migrationBuilder.DropColumn(
                name: "Latitude2",
                table: "MapDatas");

            migrationBuilder.RenameColumn(
                name: "Longitude2",
                table: "Obstacles",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "Longitude1",
                table: "Obstacles",
                newName: "Latitude");

            migrationBuilder.RenameColumn(
                name: "Longitude2",
                table: "MapDatas",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "Longitude1",
                table: "MapDatas",
                newName: "Latitude");

            migrationBuilder.CreateTable(
                name: "ObstacleMarkers",
                columns: table => new
                {
                    ObstacleMarkerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ObstacleID = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    MarkerNo = table.Column<byte>(type: "tinyint unsigned", nullable: false)
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
    }
}
