using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using CoreHtmlToImage;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Extensions.CommandExtensions;
using DiscordBot.Models.Database;
using DiscordBot.Services;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands
{
    public class AdminCommands : ModuleBase<SocketCommandContext>
    {
        public ValorantApiService ValorantApiService { get; set; }
        private IDiscordClient _client;
        [RequireOwner]
        [Summary("Prints API tokens")]
        [Command("tokens")]
        [Hidden]
        public async Task TokensCommand()
        {
            // We can also access the channel from the Command Context.
            await Context.Channel.SendMessageAsync($"AccessToken: \n{ValorantApiService.AccessToken}\n\n\nEntitlementToken: \n{ValorantApiService.EntitlementToken}");
        }

        [RequireOwner]
        [Summary("Sets channel for rank updates")]
        [Command("setUpdatesChannel")]
        public async Task SetUpdateChannelCommand([Summary("Channel to be set")]IChannel channel)
        {
            // We can also access the channel from the Command Context.
            using (var db = new DatabaseDbContext())
            {
                var guildConfig = new GuildConfig() { Guild = Context.Guild.Id, UpdatesChannel = channel.Id };
                db.AddOrUpdate(guildConfig);
                await db.SaveChangesAsync();
            }
        }

        [RequireOwner]
        [Summary("Sets a required headers to Valorant API calls")]
        [Command("setHeader")]
        public async Task SetHeaderCommand(string headerName, [Remainder] string headerValue)
        {
            using (var db = new DatabaseDbContext())
            {
                var customHeader = new CustomHeader() {Name = headerName, Value = headerValue};
                db.AddOrUpdate(customHeader);
                await db.SaveChangesAsync();
            }
        }
    }
}