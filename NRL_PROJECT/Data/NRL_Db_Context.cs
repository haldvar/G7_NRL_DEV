using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NRL_PROJECT.Models;

namespace NRL_PROJECT.Data
{
    public class NRL_Db_Context : IdentityDbContext<User>
    {
        public NRL_Db_Context(DbContextOptions<NRL_Db_Context> options) : base(options) { }

        public DbSet<ObstacleData> Obstacles { get; set; }
        public DbSet<ObstacleReportData> ObstacleReports { get; set; }
        public DbSet<AccessLevel> AccessLevels { get; set; }
        public DbSet<Organisation> Organisations { get; set; }
        public DbSet<User> Users  { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<MapData> MapDatas { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<ObstacleReportData>()
                .HasOne(r => r.Obstacle)
                .WithMany(o => o.ObstacleReports)
                .HasForeignKey(r => r.ObstacleID);


           modelBuilder.Entity<ObstacleReportData>()
                .HasOne(r => r.User)
                .WithMany(u => u.ObstacleReportsSubmitted)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ObstacleReportData>()
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.ObstacleReportsReviewed)
                .HasForeignKey(r => r.ReviewedByUserID)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<ObstacleReportData>()
                .HasOne(r => r.MapData)
                .WithMany(m => m.ObstacleReports)
                .HasForeignKey(r => r.MapDataID);

            modelBuilder.Entity<UserRole>()
                .Property(r => r.RoleName)
                .HasConversion<string>();


        }

    }

}
