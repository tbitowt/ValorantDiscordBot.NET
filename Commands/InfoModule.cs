using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using DiscordBot.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands
{
    public class TestCommand : ModuleBase<SocketCommandContext>
    {
        [Command("test")]
        public async Task Command()
        {
            using (var db = new DatabaseDbContext())
            {
                var valorantAccounts = db.ValorantAccount.Include(acc => acc.DiscordUser).ToList();
            }
            // We can also access the channel from the Command Context.
            await Context.Channel.SendMessageAsync($"Test2");
        }
    }
}