using System.Threading.Tasks;
using Discord.Commands;

namespace DiscordBot.Commands
{
    public class TestCommand : ModuleBase<SocketCommandContext>
    {
        [Command("test")]
        public async Task Command([Summary("The number to square.")] int num)
        {
            // We can also access the channel from the Command Context.
            await Context.Channel.SendMessageAsync($"Test2");
        }
    }
}