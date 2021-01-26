using System;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Models.Database;
using DiscordBot.Services;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands
{
    public class LinkingCommands : BaseCommandModule
    {
        public ValorantApiService ValorantApiService { get; set; }
        public ExternalApiService ExternalApiService { get; set; }

        [Description("aaa")]
        [Command("link")]
        public async Task LinkCommand(CommandContext ctx, DiscordUser user, [RemainingText] string subject)
        {
            await ctx.TriggerTypingAsync();
            if (subject.Contains('#'))
            {
                var strings = subject.Split('#');

                subject = await ExternalApiService.GetPlayerPuuid(strings[0], strings[1]);
            }


            await LinkAccount(ctx, user, subject);
        }

        [Description("bbb")]
        [Command("link")]
        public async Task LinkCommand(CommandContext ctx, [RemainingText] string subject)
        {
            await ctx.Channel.TriggerTypingAsync();
            if (subject.Contains('#'))
            {
                var strings = subject.Split('#');

                subject = await ExternalApiService.GetPlayerPuuid(strings[0], strings[1]);
            }

            await LinkAccount(ctx, ctx.User, subject);
        }

        [Command("unlink")]
        public async Task UnlinkCommand(CommandContext ctx, string subject)
        {
            await ctx.Channel.TriggerTypingAsync();
            await UnlinkAccount(ctx, ctx.User, subject);
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

                var acc = user?.ValorantAccounts.FirstOrDefault(acc => acc.Subject == subject);
                if (acc != null)
                {
                    user.ValorantAccounts.Remove(acc);
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
                    return;
                }

                var valorantAccount = new ValorantAccount
                {
                    Subject = subject,
                    Rank = playerRank.RankInt,
                    RankName = playerRank.RankString,
                    RankProgress = playerRank.Progress
                };

                var playerIDs = await ValorantApiService.GetPlayerIds(subject);
                if (playerIDs == null)
                {
                    await ctx.Channel.SendMessageAsync("Could not retrieve Player IDs for selected id");
                    return;
                }

                valorantAccount.DisplayName = $"{playerIDs.Name}#{playerIDs.Tag}";

                var user = db.DiscordUsers.Include(user => user.ValorantAccounts)
                    .SingleOrDefault(user => user.ID == discordUser.Id);
                if (user == null)
                {
                    user = new DbDiscordUser {Name = discordUser.Username, ID = discordUser.Id};
                    user.ValorantAccounts.Add(valorantAccount);
                    await db.DiscordUsers.AddAsync(user);
                    await ctx.Channel.SendMessageAsync(
                        $"Account {valorantAccount.DisplayName} ( {valorantAccount.RankName} )  lined to user {user.Name}");
                }

                else
                {
                    valorantAccount = user.ValorantAccounts.FirstOrDefault(account => account.Subject == subject);
                    if (valorantAccount == null)
                    {
                        user.ValorantAccounts.Add(valorantAccount);
                        await ctx.Channel.SendMessageAsync(
                            $"Account {valorantAccount.DisplayName} ( {valorantAccount.RankName} ) lined to user {user.Name}");
                    }
                    else
                    {
                        valorantAccount.DisplayName = valorantAccount.DisplayName;
                        valorantAccount.Rank = valorantAccount.Rank;
                        valorantAccount.RankName = valorantAccount.RankName;
                        valorantAccount.RankProgress = valorantAccount.RankProgress;
                        db.Update(valorantAccount);
                        await ctx.Channel.SendMessageAsync(
                            $"Account {valorantAccount.DisplayName} ( {valorantAccount.RankName} ) already linked to user {user.Name}");
                    }
                }

                if (valorantAccount.RegisteredGuilds.Any(guild => guild.GuildID == ctx.Guild.Id) == false)
                {
                    valorantAccount.RegisteredGuilds.Add(new RegisteredGuild
                        {GuildID = ctx.Guild.Id, ValorantAccount = valorantAccount});
                    db.Update(valorantAccount);
                }

                await db.SaveChangesAsync();
                var playerRankHistoty =
                    await ValorantApiService.GetPlayerRankHistory(valorantAccount, DateTime.Today.AddDays(-50));

                foreach (var rankInfo in playerRankHistoty)
                {
                    if (valorantAccount.RankInfos.Any(info => info.DateTime == rankInfo.DateTime) == false)
                        valorantAccount.RankInfos.Add(rankInfo);

                    db.Update(valorantAccount);
                }

                await db.SaveChangesAsync();
            }
        }
    }
}