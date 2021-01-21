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
        public ExternalApiService ExternalApiService { get; set; }

        [Command("link")]
        public async Task LinkCommand(IUser user, [Remainder]string subject)
        {
            await Context.Channel.TriggerTypingAsync();
            if (subject.Contains('#'))
            {
                var strings = subject.Split('#');

                subject = await ExternalApiService.GetPlayerPuuid(strings[0], strings[1]);

            }
            var socketUser = user as SocketUser;
            
            await LinkAccount(socketUser, subject);
        }

        [Command("link")]
        public async Task LinkCommand([Remainder] string subject)
        {
            await Context.Channel.TriggerTypingAsync();
            if (subject.Contains('#'))
            {
                var strings = subject.Split('#');

                subject = await ExternalApiService.GetPlayerPuuid(strings[0], strings[1]);

            }
            
            await LinkAccount(Context.User, subject);
        }

        [Command("unlink")]
        public async Task UnlinkCommand(string subject)
        {
            await Context.Channel.TriggerTypingAsync();
            await UnlinkAccount(Context.User, subject);
        }
        
        [Command("unlink")]
        public async Task UnlinkCommand(IUser user, string subject)
        {
            await Context.Channel.TriggerTypingAsync();
            var socketUser = user as SocketUser;
            await UnlinkAccount(socketUser, subject);
        }
        
        [Command("unlink")]
        public async Task UnlinkCommand()
        {
            await Context.Channel.TriggerTypingAsync();
            using (var db = new DatabaseDbContext())
            {
                var user = db.DiscordUsers.Include(user => user.ValorantAccounts).SingleOrDefault(user => user.DiscordUserId == Context.User.Id);
                if (user == null || user.ValorantAccounts.Count == 0)
                {
                    await Context.Channel.SendMessageAsync($"User does not have assigned valorant accounts");
                }
                else
                {
                    var msg = "Assigned accounts:\n";
                    foreach (var valorantAccount in user.ValorantAccounts)
                    {
                        msg += $"`{valorantAccount.Subject}\t:\t{valorantAccount.DisplayName}`\n";
                    }
                    await Context.Channel.SendMessageAsync(msg);
                }
            }
        }

        private async Task UnlinkAccount(SocketUser discordUser, string subject)
        {
            using (var db = new DatabaseDbContext())
            {
                var user = db.DiscordUsers.Include(user => user.ValorantAccounts)
                    .SingleOrDefault(user => user.DiscordUserId == discordUser.Id);

                var acc = user?.ValorantAccounts.FirstOrDefault(acc => acc.Subject == subject);
                if (acc != null)
                {
                    user.ValorantAccounts.Remove(acc);
                    await Context.Channel.SendMessageAsync($"Account {acc.DisplayName} unlinked from the user");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"The user does not have assigned requested account");
                }
            }
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
                    await Context.Channel.SendMessageAsync($"Account {valorantAccount.DisplayName} ( {valorantAccount.RankName} )  lined to user {user.Name}");
                }

                else
                {
                    var account = user.ValorantAccounts.FirstOrDefault(account => account.Subject == subject);
                    if (account == null)
                    {
                        user.ValorantAccounts.Add(valorantAccount);
                        await Context.Channel.SendMessageAsync($"Account {valorantAccount.DisplayName} ( {valorantAccount.RankName} ) lined to user {user.Name}");
                    }
                    else
                    {
                        account.DisplayName = valorantAccount.DisplayName;
                        account.Rank = valorantAccount.Rank;
                        account.RankName = valorantAccount.RankName;
                        account.RankProgress = valorantAccount.RankProgress;
                        db.Update(account);
                        await Context.Channel.SendMessageAsync($"Account {valorantAccount.DisplayName} ( {valorantAccount.RankName} ) already linked to user {user.Name}");
                    }
                }

                await db.SaveChangesAsync();
            }
        }
    }
}