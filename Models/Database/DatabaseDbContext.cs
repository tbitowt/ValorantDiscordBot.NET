using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Models.Database
{
    public class DatabaseDbContext : DbContext
    {
        public DatabaseDbContext()
        {
            // Database.EnsureCreated();
        }

        public DbSet<DiscordUser> DiscordUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=database.db");
    }
}