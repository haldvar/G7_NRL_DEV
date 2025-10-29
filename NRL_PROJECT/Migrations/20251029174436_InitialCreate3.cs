using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NRL_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObstacleReports_MapDatas_MapDataID",
                table: "ObstacleReports");

            migrationBuilder.DropForeignKey(
                name: "FK_ObstacleReports_Obstacles_ObstacleID",
                table: "ObstacleReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Obstacles_MapDatas_MapDataMapViewID",
                table: "Obstacles");

            migrationBuilder.RenameColumn(
                name: "MapDataMapViewID",
                table: "Obstacles",
                newName: "MapDataID");

            migrationBuilder.RenameIndex(
                name: "IX_Obstacles_MapDataMapViewID",
                table: "Obstacles",
                newName: "IX_Obstacles_MapDataID");

            migrationBuilder.RenameColumn(
                name: "MapViewID",
                table: "MapDatas",
                newName: "MapDataID");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "ObstacleReports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ReviewedByUserID",
                table: "ObstacleReports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ObstacleImageURL",
                table: "ObstacleReports",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "ObstacleID",
                table: "ObstacleReports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MapDataID",
                table: "ObstacleReports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ObstacleReports_MapDatas_MapDataID",
                table: "ObstacleReports",
                column: "MapDataID",
                principalTable: "MapDatas",
                principalColumn: "MapDataID");

            migrationBuilder.AddForeignKey(
                name: "FK_ObstacleReports_Obstacles_ObstacleID",
                table: "ObstacleReports",
                column: "ObstacleID",
                principalTable: "Obstacles",
                principalColumn: "ObstacleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Obstacles_MapDatas_MapDataID",
                table: "Obstacles",
                column: "MapDataID",
                principalTable: "MapDatas",
                principalColumn: "MapDataID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObstacleReports_MapDatas_MapDataID",
                table: "ObstacleReports");

            migrationBuilder.DropForeignKey(
                name: "FK_ObstacleReports_Obstacles_ObstacleID",
                table: "ObstacleReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Obstacles_MapDatas_MapDataID",
                table: "Obstacles");

            migrationBuilder.RenameColumn(
                name: "MapDataID",
                table: "Obstacles",
                newName: "MapDataMapViewID");

            migrationBuilder.RenameIndex(
                name: "IX_Obstacles_MapDataID",
                table: "Obstacles",
                newName: "IX_Obstacles_MapDataMapViewID");

            migrationBuilder.RenameColumn(
                name: "MapDataID",
                table: "MapDatas",
                newName: "MapViewID");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "ObstacleReports",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ReviewedByUserID",
                table: "ObstacleReports",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "ObstacleReports",
                keyColumn: "ObstacleImageURL",
                keyValue: null,
                column: "ObstacleImageURL",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ObstacleImageURL",
                table: "ObstacleReports",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "ObstacleID",
                table: "ObstacleReports",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MapDataID",
                table: "ObstacleReports",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ObstacleReports_MapDatas_MapDataID",
                table: "ObstacleReports",
                column: "MapDataID",
                principalTable: "MapDatas",
                principalColumn: "MapViewID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ObstacleReports_Obstacles_ObstacleID",
                table: "ObstacleReports",
                column: "ObstacleID",
                principalTable: "Obstacles",
                principalColumn: "ObstacleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Obstacles_MapDatas_MapDataMapViewID",
                table: "Obstacles",
                column: "MapDataMapViewID",
                principalTable: "MapDatas",
                principalColumn: "MapViewID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
