using System.Threading.Tasks;
using DiscordBot.Models.Database;
using DiscordBot.Services;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Commands
{
    public class AdminCommands : LoggerCommandModule
    {
        public ValorantApiService ValorantApiService { get; }

        public AdminCommands(ILoggerFactory loggerFactory, ValorantApiService valorantApiService) : base(loggerFactory)
        {
            ValorantApiService = valorantApiService;
        }

        [Hidden]
        [RequireOwner]
        [Command("tokens")]
        public async Task TokensCommand(CommandContext ctx)
        {
            LogCommandExecuted(ctx);
            // We can also access the channel from the Command Context.
            await ctx.Channel.SendMessageAsync(
                $"AccessToken: \n{ValorantApiService.AccessToken}\n\n\nEntitlementToken: \n{ValorantApiService.EntitlementToken}");
        }

        [Hidden]
        [RequireOwner]
        [Command("setUpdatesChannel")]
        public async Task SetUpdateChannelCommand(CommandContext ctx, DiscordChannel channel)
        {
            Logger.LogInformation($"Update channel for guild {ctx.Guild.Name} set to {channel.Name}");
            // We can also access the channel from the Command Context.
            using (var db = new DatabaseDbContext())
            {
                var guildConfig = new GuildConfig {Guild = ctx.Guild.Id, UpdatesChannel = channel.Id};
                db.AddOrUpdate(guildConfig);
                await db.SaveChangesAsync();
            }
        }

        [Hidden]
        [RequireOwner]
        [Command("setHeader")]
        public async Task SetHeaderCommand(CommandContext ctx, string headerName, [RemainingText] string headerValue)
        {
            Logger.LogInformation($"Added header {headerName} = {headerValue}");
            using (var db = new DatabaseDbContext())
            {
                var customHeader = new CustomHeader {Name = headerName, Value = headerValue};
                db.AddOrUpdate(customHeader);
                await db.SaveChangesAsync();
            }
        }
    }
}