using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DiscordBot.Models.Database
{
    public class GuildConfig
    {
        [Key] public ulong Guild { get; set; }

        public ulong? UpdatesChannel { get; set; }
        public bool EnableDebug { get; set; }
        [DefaultValue("EU")]
        public string Region { get; set; }
    }
}