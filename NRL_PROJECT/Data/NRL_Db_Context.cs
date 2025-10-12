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

    public class NRL_Db_ContextFactory : IDesignTimeDbContextFactory<NRL_Db_Context>
    {
        public NRL_Db_Context CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NRL_Db_Context>();
            optionsBuilder.UseMySql("server=localhost;port=3306;database=nrl_project_db;user=root;password=Begripeligvis1214;",
                ServerVersion.AutoDetect("server=localhost;port=3306;database=nrl_project_db;user=root;password=Begripeligvis1214;")
            );

            return new NRL_Db_Context(optionsBuilder.Options);
        }
    }

}
