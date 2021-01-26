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
        public PlotService PlotService { get; set; }
        
        
        [Command("rank")]
        [Summary("Prints rank of all accounts connected to a selected discord user")]
        public async Task RankCommand([Summary("if empty, current user would be selected")][Name("DiscordUser")]IUser discordUser = null)
        {
            discordUser ??= Context.User;

            using (var db = new DatabaseDbContext())
            {
                var user = await db.DiscordUsers.Include(user => user.ValorantAccounts)
                    .FirstOrDefaultAsync(user => user.DiscordUserId == discordUser.Id);

                if (user == null || user.ValorantAccounts.Count == 0)
                {
                    await Context.Channel.SendMessageAsync("You have no connected accounts");
                    return;
                }

                var embed = new EmbedBuilder().WithTitle($"List of {user.Name} accounts");
                foreach (var valorantAccount in user.ValorantAccounts.OrderByDescending(val => val.Rank).ThenByDescending(val => val.RankProgress))
                {
                    var playerRank = await ValorantApiService.GetPlayerRank(valorantAccount.Subject);
                    valorantAccount.UpdateRank(playerRank);

                    var playerIDs = await ValorantApiService.GetPlayerIds(valorantAccount.Subject);
                    if (playerIDs != null)
                    {
                        valorantAccount.DisplayName = $"{playerIDs.Name}#{playerIDs.Tag}";
                    }

                    var guildEmote = Context.Guild.Emotes.FirstOrDefault(emote => emote.Name == valorantAccount.RankName.Replace(" ", ""));
                    embed.AddField("Name", valorantAccount.DisplayName, true);
                    embed.AddField("Rank", $"{guildEmote?.ToString() ?? ""}{valorantAccount.RankName}", true);
                    embed.AddField("Progress", $"{valorantAccount.RankProgress} / 100", true);


                    db.Update(valorantAccount);
                }

                await Context.Channel.SendMessageAsync(embed:embed.Build());

                await db.SaveChangesAsync();
            }
        }

        [Command("history")]
        public async Task HistoryCommand(string accountName)
        {
            using (var db = new DatabaseDbContext())
            {
                var account = await db.ValorantAccount.Include(user => user.RankInfos)
                    .FirstOrDefaultAsync(acc => acc.DisplayName == accountName);

                if (account == null)
                {
                    await Context.Channel.SendMessageAsync(
                        "No account with specified ID found. You must specify valorant account name");
                }

                if (account.RankInfos.Count == 0)
                {
                    
                }
                else
                
                {
                    var historyPlot = PlotService.GetHistoryPlot(account);
                    await Context.Channel.SendFileAsync(historyPlot, "history.png");
                }
            }
        }
    }
}