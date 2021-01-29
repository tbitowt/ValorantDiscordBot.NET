using System;
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
    public class LinkingCommands : LoggerCommandModule
    {
        public ValorantApiService ValorantApiService { get;  }
        public ExternalApiService ExternalApiService { get;  }

        public LinkingCommands(ILoggerFactory loggerFactory, ValorantApiService valorantApiService, ExternalApiService externalApiService) : base(loggerFactory)
        {
            ValorantApiService = valorantApiService;
            ExternalApiService = externalApiService;
        }
        
        [Command("link")]
        public async Task LinkCommand(CommandContext ctx, DiscordUser user, [RemainingText] string subject)
        {
            await ctx.TriggerTypingAsync();

            Logger.LogInformation($"Trying to link account {subject} to {user.Username}");

            if (subject.Contains('#'))
            {
                var strings = subject.Split('#');

                subject = await ExternalApiService.GetPlayerPuuid(strings[0], strings[1]);
                Logger.LogInformation($"User specified valorant id#tag. Found puuid: {subject}");
            }
            
            await LinkAccount(ctx, user, subject);
        }
        
        [Command("link")]
        public async Task LinkCommand(CommandContext ctx, [RemainingText] string subject)
        {
            await LinkCommand(ctx, ctx.User, subject);
        }

        [Command("unlink")]
        public async Task UnlinkCommand(CommandContext ctx, string subject)
        {
            await UnlinkCommand(ctx, ctx.User, subject);
        }

        [Command("unlink")]
        public async Task UnlinkCommand(CommandContext ctx, DiscordUser user, string subject)
        {
            await ctx.Channel.TriggerTypingAsync();
            await UnlinkAccount(ctx, user, subject);
        }

        [Command("unlink")]
        public async Task UnlinkCommand(CommandContext ctx)
        {
            await ctx.Channel.TriggerTypingAsync();
            using (var db = new DatabaseDbContext())
            {
                var user = db.DiscordUsers.Include(user => user.ValorantAccounts)
                    .SingleOrDefault(user => user.ID == ctx.User.Id);
                if (user == null || user.ValorantAccounts.Count == 0)
                {
                    await ctx.Channel.SendMessageAsync("User does not have assigned valorant accounts");
                }
                else
                {
                    var msg = "Assigned accounts:\n";
                    foreach (var valorantAccount in user.ValorantAccounts)
                        msg += $"`{valorantAccount.Subject}\t:\t{valorantAccount.DisplayName}`\n";
                    await ctx.Channel.SendMessageAsync(msg);
                }
            }
        }

        private async Task UnlinkAccount(CommandContext ctx, DiscordUser dbDiscordUser, string subject)
        {
            using (var db = new DatabaseDbContext())
            {
                var user = db.DiscordUsers.Include(user => user.ValorantAccounts)
                    .SingleOrDefault(user => user.ID == dbDiscordUser.Id);
                var acc = user?.ValorantAccounts.FirstOrDefault(acc => acc.Subject == subject || acc.DisplayName == subject);
                if (acc != null)
                {
                    user.ValorantAccounts.Remove(acc);
                    await db.SaveChangesAsync();
                    await ctx.Channel.SendMessageAsync($"Account {acc.DisplayName} unlinked from the user");
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("The user does not have assigned requested account");
                }
            }
        }

        private async Task LinkAccount(CommandContext ctx, DiscordUser discordUser, string subject)
        {
            using (var db = new DatabaseDbContext())
            {
                var playerRank = await ValorantApiService.GetPlayerRank(subject);
                if (playerRank == null)
                {
                    await ctx.Channel.SendMessageAsync("Could not retrieve Player rank for selected id");
                    Logger.LogError($"Could not retrieve Player rank for {subject}");
                    return;
                }

                

                var playerIDs = await ValorantApiService.GetPlayerIds(subject);
                if (playerIDs == null)
                {
                    await ctx.Channel.SendMessageAsync("Could not retrieve Player IDs for selected id");
                    Logger.LogError($"Could not retrieve Player IDs for {subject}");
                    return;
                }

                var newValorantAccount = new ValorantAccount
                {
                    Subject = subject,
                    Rank = playerRank.RankInt,
                    RankName = playerRank.RankString,
                    RankProgress = playerRank.Progress,
                    DisplayName = $"{playerIDs.Name}#{playerIDs.Tag}"
                };

                var user = db.DiscordUsers.Include(user => user.ValorantAccounts)
                    .SingleOrDefault(user => user.ID == discordUser.Id);
                if (user == null)
                {
                    user = new DbDiscordUser {Name = discordUser.Username, ID = discordUser.Id};
                    user.ValorantAccounts.Add(newValorantAccount);
                    await db.DiscordUsers.AddAsync(user);
                    await ctx.Channel.SendMessageAsync(
                        $"Account {newValorantAccount.DisplayName} ( {newValorantAccount.RankName} )  lined to user {user.Name}");
                }

                else
                {
                    var existingAccount = user.ValorantAccounts.FirstOrDefault(account => account.Subject == subject);
                    if (existingAccount == null)
                    {
                        user.ValorantAccounts.Add(newValorantAccount);
                        await ctx.Channel.SendMessageAsync(
                            $"Account {newValorantAccount.DisplayName} ( {newValorantAccount.RankName} ) lined to user {user.Name}");
                    }
                    else
                    {
                        
                        existingAccount.DisplayName = newValorantAccount.DisplayName;
                        existingAccount.Rank = newValorantAccount.Rank;
                        existingAccount.RankName = newValorantAccount.RankName;
                        existingAccount.RankProgress = newValorantAccount.RankProgress;
                        db.Update(existingAccount);
                        await ctx.Channel.SendMessageAsync(
                            $"Account {existingAccount.DisplayName} ( {existingAccount.RankName} ) already linked to user {user.Name}");
                        newValorantAccount = existingAccount;
                    }
                }

                if (newValorantAccount.RegisteredGuilds.Any(guild => guild.GuildID == ctx.Guild.Id) == false)
                {
                    newValorantAccount.RegisteredGuilds.Add(new RegisteredGuild
                        {GuildID = ctx.Guild.Id, ValorantAccount = newValorantAccount });
                    db.Update(newValorantAccount);
                }

                await db.SaveChangesAsync();
                var playerRankHistoty =
                    await ValorantApiService.GetPlayerRankHistory(newValorantAccount, DateTime.Today.AddDays(-50));

                foreach (var rankInfo in playerRankHistoty)
                {
                    if (newValorantAccount.RankInfos.Any(info => info.DateTime == rankInfo.DateTime) == false)
                        newValorantAccount.RankInfos.Add(rankInfo);

                    db.Update(newValorantAccount);
                }

                await db.SaveChangesAsync();
            }
        }
    }
}