using System.Threading.Tasks;
using Discord.Commands;
using DiscordBot.Services;

namespace DiscordBot.Commands
{
    public class GetRanks : ModuleBase<SocketCommandContext>
    {
        public ValorantApiService ValorantApiService { get; set; }
        
        [Command("me")]
        [Alias("rank")]
        public async Task Rank(string subject)
        {
            var loginResult = ValorantApiService.Login();
            if (loginResult)
            {
                var mmr = ValorantApiService.GetPlayerRank(subject);
                await Context.Channel.SendMessageAsync($"Rank: {mmr.RankString}  Progress: {mmr.Progress}/100");
                return;
            }
            // We can also access the channel from the Command Context.
            await Context.Channel.SendMessageAsync("Could not login");

        }
    }
}