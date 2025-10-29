using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NRL_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessLevelDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AccessLevelDescription",
                table: "AccessLevels",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AccessLevels",
                keyColumn: "AccessLevelDescription",
                keyValue: null,
                column: "AccessLevelDescription",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "AccessLevelDescription",
                table: "AccessLevels",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
