using System.Runtime.CompilerServices;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DiscordBot.Commands
{
    public class LoggerCommandModule : BaseCommandModule
    {
        public ILogger Logger { get; }

        public LoggerCommandModule(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(GetType());
        }

        protected void LogCommandExecuted(CommandContext ctx)
        {
            Logger.LogInformation($"command {ctx.Command.Name} executed by {ctx.User.Username}#{ctx.User.Discriminator}");
        }
    }
}