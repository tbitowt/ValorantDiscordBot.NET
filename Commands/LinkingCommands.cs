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

        [Command("setRegion")]
        public async Task SetRegionCommand(CommandContext ctx, string region, [RemainingText]string account)
        {
            Region reg;
            
            var parsed = Enum.TryParse(region, true, out reg);
            if (parsed == false)
            {
                
                await ctx.Channel.SendMessageAsync($"Could not parse region. Possible values: {string.Join(", ", Enum.GetNames(typeof(Region)))}");
                return;
            }
            using (var db = new DatabaseDbContext())
            {
                var acc = db.ValorantAccount.FirstOrDefault(acc => acc.DisplayName == account);
                if (acc == null)
                {
                    await ctx.Channel.SendMessageAsync("Cannot find specified account");
                    return;
                }

                acc.Region = reg;
                db.Update(acc);
                await db.SaveChangesAsync();
                await ctx.Channel.SendMessageAsync("Region updated");
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
                var guild = db.GuildConfigs.FirstOrDefault(g => g.Guild == ctx.Guild.Id);
                var playerRank = await ValorantApiService.GetPlayerRank(guild.Region, subject);
                if (playerRank == null)
                {
                    await ctx.Channel.SendMessageAsync("Could not retrieve Player rank for selected id");
                    Logger.LogError($"Could not retrieve Player rank for {subject}");
                    return;
                }



                var playerIDs = await ValorantApiService.GetPlayerIds(guild.Region, subject);
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

                var valorantDbService = new ValorantDbService();
                var accountLinked = await valorantDbService.LinkAccountToDiscordUser(newValorantAccount, discordUser);

                await valorantDbService.AsignAccountToGuild(newValorantAccount, ctx.Guild.Id);

                var playerRankHistoty =
                    await ValorantApiService.GetPlayerRankHistory(guild.Region, newValorantAccount, DateTime.Today.AddDays(-50));
                await valorantDbService.AddLastAccountMatch(newValorantAccount, playerRankHistoty);

                if (accountLinked)
                {
                    await ctx.Channel.SendMessageAsync(
                        $"Account {newValorantAccount.DisplayName} ( {newValorantAccount.RankName} )  lined to user {discordUser.Username}");
                }
                else
                {
                    await ctx.Channel.SendMessageAsync(
                        $"Account {newValorantAccount.DisplayName} ( {newValorantAccount.RankName} ) already linked to user {discordUser.Username}");
                }
            }
        }
    }
}