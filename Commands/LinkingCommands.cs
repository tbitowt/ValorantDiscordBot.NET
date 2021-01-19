using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Models.Database;
using DiscordBot.Services;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands
{
    public class LinkingCommands: ModuleBase<SocketCommandContext>
    {
        public ValorantApiService ValorantApiService { get; set; }

        [Command("link")]
        public async Task LinkCommand(IUser user, string subject)
        {
            var socketUser = user as SocketUser;
            await LinkAccount(socketUser, subject);
        }

        [Command("link")]
        public async Task LinkCommand(string subject)
        {
            await LinkAccount(Context.User, subject);
        }

        private async Task LinkAccount(SocketUser discordUser, string subject)
        {
            using (var db = new DatabaseDbContext())
            {
                var playerRank = await ValorantApiService.GetPlayerRank(subject);
                if (playerRank == null)
                {
                    await Context.Channel.SendMessageAsync("Could not retrieve Player rank for selected id");
                    return;
                }

                var valorantAccount = new ValorantAccount()
                {
                    Subject = subject,
                    Rank = playerRank.RankInt,
                    RankName = playerRank.RankString,
                    RankProgress = playerRank.Progress
                };
                var playerIDs = await ValorantApiService.GetPlayerIds(subject);
                if (playerIDs == null)
                {
                    await Context.Channel.SendMessageAsync("Could not retrieve Player IDs for selected id");
                    return;
                }

                valorantAccount.DisplayName = $"{playerIDs.Name}#{playerIDs.Tag}";

                var user = db.DiscordUsers.Include(user => user.ValorantAccounts)
                    .SingleOrDefault(user => user.DiscordUserId == discordUser.Id);
                if (user == null)
                {
                    user = new DiscordUser() {Name = discordUser.Username, DiscordUserId = discordUser.Id};
                    user.ValorantAccounts.Add(valorantAccount);
                    await db.DiscordUsers.AddAsync(user);
                }

                else
                {
                    var account = user.ValorantAccounts.FirstOrDefault(account => account.Subject == subject);
                    if (account == null)
                    {
                        user.ValorantAccounts.Add(valorantAccount);
                    }
                    else
                    {
                        account.DisplayName = valorantAccount.DisplayName;
                        account.Rank = valorantAccount.Rank;
                        account.RankName = valorantAccount.RankName;
                        account.RankProgress = valorantAccount.RankProgress;
                        db.Update(account);
                    }
                }


                await db.SaveChangesAsync();
            }
        }
    }
}