using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Models.Database;
using DiscordBot.Services;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Commands
{
    public class GetRanks : LoggerCommandModule
    {
        public ValorantApiService ValorantApiService { get; }

        public PlotService PlotService { get; }

        public GetRanks(ILoggerFactory loggerFactory, ValorantApiService valorantApiService, PlotService plotService) : base(loggerFactory)
        {
            ValorantApiService = valorantApiService;
            PlotService = plotService;
        }

        [Command("rank")]
        public async Task RankCommand(CommandContext ctx, DiscordUser discordUser = null)
        {
            LogCommandExecuted(ctx);
            discordUser ??= ctx.User;

            using (var db = new DatabaseDbContext())
            {
                var user = await db.DiscordUsers.Include(user => user.ValorantAccounts)
                    .FirstOrDefaultAsync(user => user.ID == discordUser.Id);

                if (user == null || user.ValorantAccounts.Count == 0)
                {
                    await ctx.Channel.SendMessageAsync("You have no connected accounts");
                    Logger.LogInformation($"User {discordUser.Username} has no ValorantAccounts connected");
                    return;
                }

                var embed = new DiscordEmbedBuilder().WithTitle($"List of {user.Name} accounts");
                foreach (var valorantAccount in user.ValorantAccounts.OrderByDescending(val => val.Rank)
                    .ThenByDescending(val => val.RankProgress))
                {
                    var playerRank = await ValorantApiService.GetPlayerRank(valorantAccount.Subject);
                    valorantAccount.UpdateRank(playerRank);

                    var playerIDs = await ValorantApiService.GetPlayerIds(valorantAccount.Subject);
                    if (playerIDs != null) valorantAccount.DisplayName = $"{playerIDs.Name}#{playerIDs.Tag}";

                    var guildEmote = ctx.Guild.Emojis.FirstOrDefault(emote =>
                        emote.Value.Name == valorantAccount.RankName.Replace(" ", ""));
                    embed.AddField("Name", valorantAccount.DisplayName, true);
                    embed.AddField("Rank", $"{guildEmote.Value}{valorantAccount.RankName}", true); //todo: add emoji
                    embed.AddField("Progress", $"{valorantAccount.RankProgress} / 100", true);


                    db.Update(valorantAccount);
                }

                await ctx.Channel.SendMessageAsync(embed: embed.Build());

                await db.SaveChangesAsync();
            }
        }

        [Command("history")]
        public async Task HistoryCommand(CommandContext ctx, string accountName)
        {
            LogCommandExecuted(ctx);
            using (var db = new DatabaseDbContext())
            {
                var account = await db.ValorantAccount.Include(user => user.RankInfos)
                    .FirstOrDefaultAsync(acc => acc.DisplayName == accountName);

                if (account == null)
                {
                    await ctx.Channel.SendMessageAsync(
                        "No account with specified ID found. You must specify valorant account name");
                    Logger.LogInformation($"Cannot find Valorant Account {accountName}");
                }

                if (account.RankInfos.Count != 0)
                {
                    var historyPlot = PlotService.GetHistoryPlot(account);
                    // await ctx.Channel.SendFileAsync(historyPlot, "history.png"); // todo: Add sending plot
                    await ctx.Channel.SendFileAsync("history.png", historyPlot);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync($"No history entries for {accountName}");
                }
            }
        }
    }
}