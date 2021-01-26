using System.Collections.Generic;

namespace DiscordBot.Models.API
{
    public class MatchHistory
    {
        public long Version { get; set; }
        public string Subject { get; set; }
        public List<Match> Matches { get; set; }
    }

    public class Match
    {
        public string MatchId { get; set; }
        public string MapId { get; set; }
        public long MatchStartTime { get; set; }
        public long TierAfterUpdate { get; set; }
        public long TierBeforeUpdate { get; set; }
        public long RankedRatingAfterUpdate { get; set; }
        public long RankedRatingBeforeUpdate { get; set; }
        public long RankedRatingEarned { get; set; }
        public long RankedRatingPerformanceBonus { get; set; }
        public string CompetitiveMovement { get; set; }
        public long AfkPenalty { get; set; }
    }
}