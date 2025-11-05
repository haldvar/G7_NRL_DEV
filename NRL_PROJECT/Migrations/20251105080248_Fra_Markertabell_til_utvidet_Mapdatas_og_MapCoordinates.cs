using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NRL_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class Fra_Markertabell_til_utvidet_Mapdatas_og_MapCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude2",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "Longitude2",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "Latitude1",
                table: "MapDatas");

            migrationBuilder.DropColumn(
                name: "Latitude2",
                table: "MapDatas");

            migrationBuilder.DropColumn(
                name: "Longitude1",
                table: "MapDatas");

            migrationBuilder.DropColumn(
                name: "Longitude2",
                table: "MapDatas");

            migrationBuilder.RenameColumn(
                name: "Longitude1",
                table: "Obstacles",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "Latitude1",
                table: "Obstacles",
                newName: "Latitude");

            migrationBuilder.AddColumn<string>(
                name: "GeometryType",
                table: "MapDatas",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MapCoordinate",
                columns: table => new
                {
                    CoordinateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MapDataID = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<double>(type: "double", nullable: false),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapCoordinate", x => x.CoordinateId);
                    table.ForeignKey(
                        name: "FK_MapCoordinate_MapDatas_MapDataID",
                        column: x => x.MapDataID,
                        principalTable: "MapDatas",
                        principalColumn: "MapDataID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MapCoordinate_MapDataID",
                table: "MapCoordinate",
                column: "MapDataID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MapCoordinate");

            migrationBuilder.DropColumn(
                name: "GeometryType",
                table: "MapDatas");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "Obstacles",
                newName: "Longitude1");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "Obstacles",
                newName: "Latitude1");

            migrationBuilder.AddColumn<double>(
                name: "Latitude2",
                table: "Obstacles",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude2",
                table: "Obstacles",
                type: "double",
                nullable: true);

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
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude1",
                table: "MapDatas",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude2",
                table: "MapDatas",
                type: "double",
                nullable: true);
        }
    }
}
