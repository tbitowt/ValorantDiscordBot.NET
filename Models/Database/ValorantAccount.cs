using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DiscordBot.Services;

namespace DiscordBot.Models.Database
{
    public enum Region
    {
        EU, 
        NA,
        AP,
        KO
    }

    public class ValorantAccount
    {
        public ValorantAccount()
        {
            RegionString = Region.EU.ToString();
        }

        [Key] public int ID { get; set; }

        public string Subject { get; set; }
        public string DisplayName { get; set; }

        public int Rank { get; set; }
        public string RankName { get; set; }
        public int RankProgress { get; set; }
        public DbDiscordUser DbDiscordUser { get; set; }

        [Column("Region")]
        [Required]
        public string RegionString
        {
            get
            {
                return Region.ToString();
            }
            set
            {
                Region = Enum.Parse<Region>(value, true);
            }
        } 

        [NotMapped] public Region Region { get; set; } = Region.EU;

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