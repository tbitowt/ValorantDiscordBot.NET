using System;
using System.Threading.Tasks;
using DiscordBot.Models.Database;
using DiscordBot.Services;
using dotenv.net;
using dotenv.net.Utilities;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DiscordBot.Commands
{
}

namespace DiscordBot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] <{SourceContext}>\t{Message:j} {NewLine}{Exception}")
                .CreateLogger();
            MainAsync().GetAwaiter().GetResult();
        }


        private static async Task MainAsync()
        {
            DotEnv.AutoConfig();
            using (var services = ConfigureServices())
            {
                var envCheckerService = services.GetRequiredService<EnvCheckerService>();
                var checkEnvironmentValues = envCheckerService.CheckEnvironmentValues();
                if (checkEnvironmentValues == false)
                {
                    Console.WriteLine("Error: CheckEnvironmentValues ");
                    return;
                }

                var valorantApiService = services.GetRequiredService<ValorantApiService>();
                valorantApiService.SetRegion("eu");

                await using (var db = new DatabaseDbContext())
                {
                    await db.Database.MigrateAsync();
                }

                var discord = services.GetRequiredService<BotService>();
                await discord.StartAsync();


                services.GetRequiredService<PlayerRankChecker>().Start();


                await Task.Delay(-1);
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<EnvReader>()
                .AddSingleton<DiscordClient>()
                .AddSingleton<EnvCheckerService>()
                .AddSingleton<ValorantApiService>()
                .AddSingleton<ExternalApiService>()
                .AddSingleton<PlayerRankChecker>()
                .AddSingleton<PlotService>()
                .AddSingleton<BotService>()
                .AddLogging(c => c.AddSerilog())
                .BuildServiceProvider();
        }
    }


    // Keep in mind your module **must** be public and inherit ModuleBase.
// If it isn't, it will not be discovered by AddModulesAsync!
}