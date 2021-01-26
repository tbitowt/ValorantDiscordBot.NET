using System;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DiscordBot.Services
{
    public class BotService
    {
        private readonly EnvCheckerService _envCheckerService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IServiceProvider _serviceProvider;

        public BotService(EnvCheckerService envCheckerService, IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
        {
            _envCheckerService = envCheckerService;
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
            Logger = loggerFactory.CreateLogger<BotService>();


            var logFactory = new LoggerFactory().AddSerilog();

            var discordConfiguration = new DiscordConfiguration
            {
                Token = _envCheckerService.Discord_Token,
                TokenType = TokenType.Bot,
                LoggerFactory = logFactory
            };

            DiscordClient = new DiscordClient(discordConfiguration);

            var commandsNextConfiguration = new CommandsNextConfiguration
            {
                StringPrefixes = new[] {"!!"},
                Services = _serviceProvider
            };

            var commands = DiscordClient.UseCommandsNext(commandsNextConfiguration);

            commands.RegisterCommands(Assembly.GetExecutingAssembly());

            DiscordClient.ClientErrored += (sender, args) =>
            {
                Logger.LogError(args.ToString());
                return Task.CompletedTask;
            };

            DiscordClient.Ready += (sender, args) =>
            {
                Logger.LogInformation("Ready");
                return Task.CompletedTask;
            };
        }

        public ILogger<BotService> Logger { get; set; }
        public DiscordClient DiscordClient { get; }


        public async Task StartAsync()
        {
            await DiscordClient.ConnectAsync();
        }
    }
}