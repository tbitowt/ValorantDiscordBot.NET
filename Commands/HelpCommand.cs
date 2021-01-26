using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBot.Services;
using DiscordBot.Extensions.CommandExtensions;

namespace DiscordBot.Commands
{
    public class HelpCommand : ModuleBase<SocketCommandContext>
    {
        public CommandService CommandService { get; set; }
        public EnvCheckerService EnvCheckerService { get; set; }
        [Command("Help")]
        public async Task Help([Remainder]string commandName = null)
        {

            var botPrefix = EnvCheckerService.Bot_Prefix; //replace this with your own prefix.
            var helpEmbed = CommandService.GetDefaultHelpEmbed(commandName, botPrefix);
            
            await Context.Channel.SendMessageAsync(embed: helpEmbed);
        }

    //     public string GetHelp(string commandName = null)
    //     {
    //         string result = "List of server commands:\n";
    //
    //         if (commandName == null)
    //         {
    //             List<CommandInfo> commands = CommandService.Commands.ToList();
    //             foreach (CommandInfo command in commands)
    //             {
    //                 var b = command.GetHelp();
    //                 var nameAttributes = command.Attributes.Where(attr => attr is NameAttribute).Select(attr => attr as NameAttribute);
    //                 var firstOrDefault = nameAttributes.FirstOrDefault();
    //             }
    //         }
    //
    //         return result;
    //     }
    //
    //     private string AddCommandDescription(CommandInfo command)
    //     {
    //         var summary = command.Summary != null ? $" - {command.Summary}":"";
    //
    //         string cmdInfo = $"{EnvCheckerService.Bot_Prefix}{command.Name}{summary}";
    //         if (command.Aliases.Count != 1)
    //         {
    //             cmdInfo += $" (aliases: ";
    //             foreach (var commandAlias in command.Aliases)
    //             {
    //                 if(commandAlias != command.Name)
    //                     cmdInfo += $"{EnvCheckerService.Bot_Prefix}{commandAlias}, ";
    //             }
    //
    //             cmdInfo += ")";
    //         }
    //
    //         
    //         if (command.Parameters.Count != 0)
    //         {
    //
    //             cmdInfo += "\n  Parameters: ";
    //             foreach (var commandParameter in command.Parameters)
    //             {
    //                 if (commandParameter.IsOptional)
    //                 {
    //                     cmdInfo += "[optional] ";
    //                 }
    //
    //                 cmdInfo += $"{commandParameter.Name} ";
    //
    //                 if (commandParameter.Summary != null)
    //                 {
    //                     cmdInfo += $" - {commandParameter.Summary} ";
    //                 }
    //             }
    //         }
    //
    //         
    //
    //
    //         return cmdInfo;
    //     }
    }

    
}