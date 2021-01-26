using System;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Models.Database;
using DiscordBot.Services;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands
{
    public class RankHistory : BaseCommandModule
    {
        public ValorantApiService ValorantApiService { get; set; }

        [Hidden]
        [RequireOwner]
        [Command("getRankHistory")]
        public async Task GetRankHistoryCommand(CommandContext ctx, string accountName, int days = 50)
        {
            await ctx.Channel.TriggerTypingAsync();
            using (var db = new DatabaseDbContext())
            {
                var account = db.ValorantAccount.Include(acc => acc.RankInfos)
                    .FirstOrDefault(acc => acc.DisplayName == accountName);
                if (account == null)
                {
                    await ctx.Channel.SendMessageAsync($"Could not find an account with name {accountName}");
                    return;
                }

                var playerRankHistoty =
                    await ValorantApiService.GetPlayerRankHistory(account, DateTime.Today.AddDays(days * -1));

                var numberAdded = 0;
                foreach (var rankInfo in playerRankHistoty)
                    if (account.RankInfos.Any(info => info.DateTime == rankInfo.DateTime) == false)
                    {
                        account.RankInfos.Add(rankInfo);
                        numberAdded++;
                    }

                await db.SaveChangesAsync();

                await ctx.Channel.SendMessageAsync($"Updated {numberAdded} records");
            }
        }
    }
}