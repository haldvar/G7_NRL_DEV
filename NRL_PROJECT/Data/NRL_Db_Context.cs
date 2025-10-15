using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NRL_PROJECT.Models;

namespace NRL_PROJECT.Data
{
    public class NRL_Db_Context : DbContext
    {
        public NRL_Db_Context(DbContextOptions<NRL_Db_Context> options) : base(options) { }

        public DbSet<ObstacleData> Obstacles { get; set; }
        public DbSet<ObstacleReportData> ObstacleReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ObstacleReportData>()
                .HasOne(r => r.Obstacle)
                .WithMany(o => o.Reports)
                .HasForeignKey(r => r.ObstacleId);
        }

    }

}
