using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Models.Database
{
    public class DatabaseDbContext : DbContext
    {
        public DbSet<DiscordUser> DiscordUsers { get; set; }
        public DbSet<ValorantAccount> ValorantAccount { get; set; }
        public DbSet<GuildConfig> GuildConfigs { get; set; }
        public DbSet<CustomHeader> CustomHeaders { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=database.db");
    }
}