using Microsoft.EntityFrameworkCore;
using Wyrobot.Web.Models;

namespace Wyrobot.Web.Data
{
    public class WyrobotWebContext : DbContext
    {
        public WyrobotWebContext(DbContextOptions<WyrobotWebContext> options)
            : base(options)
        {
        }

        public DbSet<ServerSettings> ServerSettings { get; set; }
        public DbSet<LevelReward> LevelReward { get; set; }
        public DbSet<AutoRole> AutoRole { get; set; }
    }
}
