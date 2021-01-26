using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DiscordBot.Services;

namespace DiscordBot.Models.Database
{
    public class ValorantAccount
    {
        [Key] public int ID { get; set; }

        public string Subject { get; set; }
        public string DisplayName { get; set; }

        public int Rank { get; set; }
        public string RankName { get; set; }
        public int RankProgress { get; set; }
        public DbDiscordUser DbDiscordUser { get; set; }

        public List<RegisteredGuild> RegisteredGuilds { get; } = new();

        public List<RankInfo> RankInfos { get; } = new();

        public void UpdateRank(PlayerRank newRank)
        {
            if (newRank != null)
            {
                Rank = newRank.RankInt;
                RankName = newRank.RankString;
                RankProgress = newRank.Progress;
            }
        }
    }
}