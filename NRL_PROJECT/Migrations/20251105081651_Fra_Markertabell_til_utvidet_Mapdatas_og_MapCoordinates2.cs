using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NRL_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class Fra_Markertabell_til_utvidet_Mapdatas_og_MapCoordinates2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MapCoordinate_MapDatas_MapDataID",
                table: "MapCoordinate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MapCoordinate",
                table: "MapCoordinate");

            migrationBuilder.RenameTable(
                name: "MapCoordinate",
                newName: "MapCoordinates");

            migrationBuilder.RenameIndex(
                name: "IX_MapCoordinate_MapDataID",
                table: "MapCoordinates",
                newName: "IX_MapCoordinates_MapDataID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MapCoordinates",
                table: "MapCoordinates",
                column: "CoordinateId");

            migrationBuilder.AddForeignKey(
                name: "FK_MapCoordinates_MapDatas_MapDataID",
                table: "MapCoordinates",
                column: "MapDataID",
                principalTable: "MapDatas",
                principalColumn: "MapDataID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MapCoordinates_MapDatas_MapDataID",
                table: "MapCoordinates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MapCoordinates",
                table: "MapCoordinates");

            migrationBuilder.RenameTable(
                name: "MapCoordinates",
                newName: "MapCoordinate");

            migrationBuilder.RenameIndex(
                name: "IX_MapCoordinates_MapDataID",
                table: "MapCoordinate",
                newName: "IX_MapCoordinate_MapDataID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MapCoordinate",
                table: "MapCoordinate",
                column: "CoordinateId");

            migrationBuilder.AddForeignKey(
                name: "FK_MapCoordinate_MapDatas_MapDataID",
                table: "MapCoordinate",
                column: "MapDataID",
                principalTable: "MapDatas",
                principalColumn: "MapDataID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
