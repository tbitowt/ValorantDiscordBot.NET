namespace DiscordBot.Services
{
    public class PlayerRank
    {
        public int RankInt { get; set; }
        public string RankString { get; set; }
        public int Progress { get; set; }
        public int MatchesLeftForRank { get; set; }
    }
    public interface IValorantApiService
    {
        
        string AccessToken { get; }
        string EntitlementToken { get; }
        bool Login();

        PlayerRank GetPlayerRank(string playerId);

    }
}