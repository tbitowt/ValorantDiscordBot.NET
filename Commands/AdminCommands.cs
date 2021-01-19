using System.Threading.Tasks;
using Discord.Commands;
using DiscordBot.Services;

namespace DiscordBot.Commands
{
    public class AdminCommands : ModuleBase<SocketCommandContext>
    {
        public ValorantApiService ValorantApiService { get; set; }
        [RequireOwner]
        [Command("tokens")]
        public async Task Command()
        {
            // We can also access the channel from the Command Context.
            await Context.Channel.SendMessageAsync($"AccessToken: \n{ValorantApiService.AccessToken}\n\n\nEntitlementToken: \n{ValorantApiService.EntitlementToken}");
        }
    }
}