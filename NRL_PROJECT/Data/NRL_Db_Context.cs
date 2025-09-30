using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Models;

namespace NRL_PROJECT.Data
{
    public class NRL_Db_Context : DbContext
    {
        public NRL_Db_Context(DbContextOptions<NRL_Db_Context> options) : base(options) { }

        public DbSet<ObstacleData> Obstacles { get; set; }
    }
}
