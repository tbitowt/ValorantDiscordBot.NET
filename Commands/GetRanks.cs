using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBot.Models.Database;
using DiscordBot.Services;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands
{
    public class GetRanks : ModuleBase<SocketCommandContext>
    {
        public ValorantApiService ValorantApiService { get; set; }
        
        [Command("rank")]
        public async Task RankCommand()
        {
            await RankCommand(Context.User);
        }


        [Command("rank")]
        public async Task RankCommand(IUser discordUser)
        {
            using (var db = new DatabaseDbContext())
            {
                var user = await db.DiscordUsers.Include(user => user.ValorantAccounts)
                    .FirstOrDefaultAsync(user => user.DiscordUserId == discordUser.Id);

                if (user == null || user.ValorantAccounts.Count == 0)
                {
                    await Context.Channel.SendMessageAsync("You have no connected accounts");
                    return;
                }

                var msg = "List of valorant accounts:\n";
                foreach (var valorantAccount in user.ValorantAccounts)
                {
                    var playerRank = await ValorantApiService.GetPlayerRank(valorantAccount.Subject);
                    valorantAccount.UpdateRank(playerRank);

                    var playerIDs = await ValorantApiService.GetPlayerIds(valorantAccount.Subject);
                    if (playerIDs != null)
                    {
                        valorantAccount.DisplayName = $"{playerIDs.Name}#{playerIDs.Tag}";
                    }

                    msg +=
                        $"{valorantAccount.DisplayName}: {valorantAccount.RankName} ({valorantAccount.RankProgress}/100)\n";

                    db.Update(valorantAccount);
                }

                await Context.Channel.SendMessageAsync(msg);

                await db.SaveChangesAsync();
            }

        }
    }
}