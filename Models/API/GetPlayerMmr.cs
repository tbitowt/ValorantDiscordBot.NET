using System.Collections.Generic;

namespace DiscordBot.Models.API
{
    public class GetPlayerMmr
    {
        public long Version { get; set; }
        public string Subject { get; set; }
        public bool NewPlayerExperienceFinished { get; set; }
        public QueueSkills QueueSkills { get; set; }
        public LatestCompetitiveUpdate LatestCompetitiveUpdate { get; set; }
        public bool IsLeaderboardAnonymized { get; set; }
    }

    public class LatestCompetitiveUpdate
    {
        public string MatchId { get; set; }
        public string MapId { get; set; }
        public long MatchStartTime { get; set; }
        public int TierAfterUpdate { get; set; }
        public int TierBeforeUpdate { get; set; }
        public int RankedRatingAfterUpdate { get; set; }
        public int RankedRatingBeforeUpdate { get; set; }
        public int RankedRatingEarned { get; set; }
        public string CompetitiveMovement { get; set; }
    }

    public class QueueSkills
    {
        public Competitive Competitive { get; set; }
        public Custom Custom { get; set; }
        public Competitive Deathmatch { get; set; }
        public Seeding Seeding { get; set; }
        public Snowball Snowball { get; set; }
        public Competitive Spikerush { get; set; }
        public Competitive Unrated { get; set; }
    }

    public class Competitive
    {
        public int CompetitiveTier { get; set; }
        public int TierProgress { get; set; }
        public int TotalGamesNeededForRating { get; set; }
        public int RecentGamesNeededForRating { get; set; }
        public int CurrentSeasonGamesNeededForRating { get; set; }
        public int TotalGamesNeededForLeaderboard { get; set; }
        public Dictionary<string, SeasonalInfoBySeasonId> SeasonalInfoBySeasonId { get; set; }
    }

    public class SeasonalInfoBySeasonId
    {
        public string SeasonId { get; set; }
        public int NumberOfWins { get; set; }
        public List<int> TopWins { get; set; }
        public int TotalWinsNeededForRank { get; set; }
        public int Rank { get; set; }
        public int CapstoneWins { get; set; }
        public int LeaderboardRank { get; set; }
    }

    public class Custom
    {
        public int CompetitiveTier { get; set; }
        public int TierProgress { get; set; }
        public int TotalGamesNeededForRating { get; set; }
        public int RecentGamesNeededForRating { get; set; }
        public int CurrentSeasonGamesNeededForRating { get; set; }
        public int TotalGamesNeededForLeaderboard { get; set; }
        public CustomSeasonalInfoBySeasonId SeasonalInfoBySeasonId { get; set; }
    }

    public class CustomSeasonalInfoBySeasonId
    {
        public SeasonalInfoBySeasonId Empty { get; set; }
        public SeasonalInfoBySeasonId The3F61C7724560Cd3F5D3FA7Ab5Abda6B3 { get; set; }
    }

    public class Seeding
    {
        public int CompetitiveTier { get; set; }
        public int TierProgress { get; set; }
        public int TotalGamesNeededForRating { get; set; }
        public int RecentGamesNeededForRating { get; set; }
        public int CurrentSeasonGamesNeededForRating { get; set; }
        public int TotalGamesNeededForLeaderboard { get; set; }
        public SeedingSeasonalInfoBySeasonId SeasonalInfoBySeasonId { get; set; }
    }

    public class SeedingSeasonalInfoBySeasonId
    {
    }

    public class Snowball
    {
        public int CompetitiveTier { get; set; }
        public int TierProgress { get; set; }
        public int TotalGamesNeededForRating { get; set; }
        public int RecentGamesNeededForRating { get; set; }
        public int CurrentSeasonGamesNeededForRating { get; set; }
        public int TotalGamesNeededForLeaderboard { get; set; }
        public SnowballSeasonalInfoBySeasonId SeasonalInfoBySeasonId { get; set; }
    }

    public class SnowballSeasonalInfoBySeasonId
    {
        public SeasonalInfoBySeasonId The46Ea6166457311289Cea60A15640059B { get; set; }
    }
}