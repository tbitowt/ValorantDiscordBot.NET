using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Models.Database;
using DiscordBot.Services;
using dotenv.net;
using dotenv.net.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;

namespace DiscordBot.Commands
{
}

namespace DiscordBot
{
    class Program
    {
        private DiscordSocketClient _client;
        
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            DotEnv.AutoConfig();
            using (var db = new DatabaseDbContext())
            {
                await db.Database.MigrateAsync();
            }
            using (var services = ConfigureServices())
            {
                var envCheckerService = services.GetService<EnvCheckerService>();
                
                var envAvailable = envCheckerService.CheckEnvironmentValues();
                if (envAvailable == false)
                {
                    return;
                }

                var valorantApiService = services.GetService<ValorantApiService>();
                valorantApiService.SetRegion("eu");
                var loginResult = await valorantApiService.Login();
                


                var client = services.GetRequiredService<DiscordSocketClient>();

                client.Log += Log;
                services.GetRequiredService<CommandService>().Log += Log;

                // Tokens should be considered secret data and never hard-coded.
                // We can read from the environment variable to avoid hardcoding.
                await client.LoginAsync(TokenType.Bot, envCheckerService.Discord_Token);
                await client.StartAsync();

                // Here we initialize the logic required to register our commands.
                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

                await Task.Delay(Timeout.Infinite);
            }
        }
        
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        
        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<EnvReader>()
                .AddSingleton<EnvCheckerService>()
                .AddSingleton<ValorantApiService>()
                .BuildServiceProvider();
        }
    }


    // Keep in mind your module **must** be public and inherit ModuleBase.
// If it isn't, it will not be discovered by AddModulesAsync!
}