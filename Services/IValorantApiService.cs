using System.Threading.Tasks;
using DiscordBot.Models.API;

namespace DiscordBot.Services
{
    public class PlayerRank
    {
        public int RankInt { get; set; }
        public string RankString { get; set; }
        public int Progress { get; set; }
        public int MatchesLeftForRank { get; set; }
        public Match LastMatch { get; set; }
    }

    public class ValorantPlayerIds
    {
        public string Name { get; set; }
        public string Tag { get; set; }
    }

    public interface IValorantApiService
    {
        string AccessToken { get; }
        string EntitlementToken { get; }
        Task<bool> Login();

        Task<PlayerRank> GetPlayerRank(string playerId);

        Task<ValorantPlayerIds> GetPlayerIds(string playerId);
    }
}