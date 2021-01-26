using System;

namespace DiscordBot.Models.Database
{
    public class RankInfo
    {
        public int ID { get; set; }
        public ValorantAccount ValorantAccount { get; set; }
        public int RankInt { get; set; }
        public int Progress { get; set; }
        public DateTime DateTime { get; set; }
    }
}