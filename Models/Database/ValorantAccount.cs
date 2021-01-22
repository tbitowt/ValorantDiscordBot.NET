using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DiscordBot.Services;

namespace DiscordBot.Models.Database
{
    public class ValorantAccount
    {
        [Key]
        public int ValorantAccountId { get; set; }
        public string Subject { get; set; }
        public string DisplayName { get; set; }

        public int Rank { get; set; }
        public string RankName { get; set; }
        public int RankProgress { get; set; }
        public DiscordUser DiscordUser { get; set; }

        public List<RegisteredGuild> RegisteredGuilds { get; } = new List<RegisteredGuild>();

        public List<RankInfo> RankInfos { get; } = new List<RankInfo>();

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