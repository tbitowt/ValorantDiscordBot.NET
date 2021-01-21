using System.ComponentModel.DataAnnotations;

namespace DiscordBot.Models.Database
{
    public class GuildConfig
    {
        [Key]
        public ulong Guild { get; set; }
        public ulong? UpdatesChannel { get; set; }
        public bool EnableDebug { get; set; }
    }
}