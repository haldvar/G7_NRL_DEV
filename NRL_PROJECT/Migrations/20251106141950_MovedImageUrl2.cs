using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NRL_PROJECT.Migrations
{
    /// <inheritdoc />
    public partial class MovedImageUrl2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add new column to target table
            migrationBuilder.AddColumn<string>(
                name: "ObstacleImageURL",
                table: "Obstacles",
                type: "varchar(255)",
                nullable: true); // start nullable to avoid blocking if data is missing

            // // 2) Copy data from source to target (adjust join as needed)
            // migrationBuilder.Sql(@"
            //     -- Copy ObstacleImageURL from ObstacleReportData to ObstacleData
            //     UPDATE obs
            //     SET obs.ObstacleImageURL = rpt.ObstacleImageURL
            //     FROM dbo.ObstacleReports AS obs
            //     INNER JOIN dbo.Obstacles AS rpt
            //         ON obs.ObstacleId = rpt.Id
            //     WHERE rpt.ObstacleImageURL IS NOT NULL
            // ");

            // 3) Drop old column from source
            migrationBuilder.DropColumn(
                name: "ObstacleImageURL",
                table: "ObstacleReports"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse order: add back to source, copy back, drop from target
            migrationBuilder.AddColumn<string>(
                name: "ObstacleImageURL",
                table: "ObstacleReports",
                type: "varchar(255)",
                nullable: true);

            // migrationBuilder.Sql(@"
            //     UPDATE obs
            //     SET rpt.ObstacleImageURL = obs.ObstacleImageURL
            //     FROM dbo.ObstacleReports AS rpt
            //     INNER JOIN dbo.Obstacles AS obs
            //         ON rpt.ObstacleId = obs.Id
            //     WHERE obs.ObstacleImageURL IS NOT NULL
            // ");

            migrationBuilder.DropColumn(
                name: "ObstacleImageURL",
                table: "Obstacles");
        }
    }
}
