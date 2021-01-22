using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using DiscordBot.Models.Database;
using DiscordBot.Services;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands
{
    public class RankHistory : ModuleBase<SocketCommandContext>
    {
        public ValorantApiService ValorantApiService { get; set; }

        [RequireOwner]
        [Command("getRankHistory")]
        public async Task GetRankHistoryCommand(string accountName, int days = 50)
        {
            await Context.Channel.TriggerTypingAsync();
            using (var db = new DatabaseDbContext())
            {
                var account = db.ValorantAccount.Include(acc => acc.RankInfos).FirstOrDefault(acc => acc.DisplayName == accountName);
                if (account == null)
                {
                    await Context.Channel.SendMessageAsync($"Could not find an account with name {accountName}");
                    return;
                }

                var playerRankHistoty = await ValorantApiService.GetPlayerRankHistoty(account, DateTime.Today.AddDays(days * -1));

                int numberAdded = 0;
                foreach (var rankInfo in playerRankHistoty)
                {
                    if (account.RankInfos.Any(info => info.DateTime == rankInfo.DateTime) == false)
                    {
                        account.RankInfos.Add(rankInfo);
                        numberAdded++;
                    }
                }

                await db.SaveChangesAsync();

                await Context.Channel.SendMessageAsync($"Updated {numberAdded} records");
            }
            return;
        }
    }
}