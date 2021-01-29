using System;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Models.Database;
using DiscordBot.Services;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Commands
{
    public class RankHistory : LoggerCommandModule
    {
        public ValorantApiService ValorantApiService { get;  }

        public RankHistory(ILoggerFactory loggerFactory, ValorantApiService valorantApiService) : base(loggerFactory)
        {
            ValorantApiService = valorantApiService;
        }

        [Hidden]
        [RequireOwner]
        [Command("getRankHistory")]
        public async Task GetRankHistoryCommand(CommandContext ctx, [RemainingText]string accountName)
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

                var playerRankHistory =
                    await ValorantApiService.GetPlayerRankHistory(account, DateTime.Today.AddDays(50 * -1));

                var numberAdded = 0;
                foreach (var rankInfo in playerRankHistory)
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