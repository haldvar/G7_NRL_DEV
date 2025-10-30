using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NRL_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class UpdateObstacleModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CenterLongitude",
                table: "MapDatas",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "CenterLatitude",
                table: "MapDatas",
                newName: "Latitude");

            migrationBuilder.AlterColumn<double>(
                name: "ObstacleWidth",
                table: "Obstacles",
                type: "double",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "ObstacleHeight",
                table: "Obstacles",
                type: "double",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "Obstacles",
                type: "double",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "Obstacles",
                type: "double",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AddColumn<string>(
                name: "GeoJsonCoordinates",
                table: "Obstacles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "MapDataMapViewID",
                table: "Obstacles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MapViewID",
                table: "Obstacles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GeoJsonCoordinates",
                table: "MapDatas",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Obstacles_MapDataMapViewID",
                table: "Obstacles",
                column: "MapDataMapViewID");

            migrationBuilder.AddForeignKey(
                name: "FK_Obstacles_MapDatas_MapDataMapViewID",
                table: "Obstacles",
                column: "MapDataMapViewID",
                principalTable: "MapDatas",
                principalColumn: "MapViewID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Obstacles_MapDatas_MapDataMapViewID",
                table: "Obstacles");

            migrationBuilder.DropIndex(
                name: "IX_Obstacles_MapDataMapViewID",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "GeoJsonCoordinates",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "MapDataMapViewID",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "MapViewID",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "GeoJsonCoordinates",
                table: "MapDatas");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "MapDatas",
                newName: "CenterLongitude");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "MapDatas",
                newName: "CenterLatitude");

            migrationBuilder.AlterColumn<float>(
                name: "ObstacleWidth",
                table: "Obstacles",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<float>(
                name: "ObstacleHeight",
                table: "Obstacles",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Obstacles",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Obstacles",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");
        }
    }
}
