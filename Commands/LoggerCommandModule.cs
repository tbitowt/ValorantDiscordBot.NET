using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Commands
{
    public class LoggerCommandModule : BaseCommandModule
    {
        public LoggerCommandModule(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(GetType());
        }

        public ILogger Logger { get; }
    }
}