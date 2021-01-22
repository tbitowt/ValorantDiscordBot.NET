using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBot.Models.Database;
using DiscordBot.Services;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Commands
{
    public class GetRanks : ModuleBase<SocketCommandContext>
    {
        public ValorantApiService ValorantApiService { get; set; }
        
        [Command("rank")]
        public async Task RankCommand()
        {
            await RankCommand(Context.User);
        }


        [Command("rank")]
        public async Task RankCommand(IUser discordUser)
        {
            using (var db = new DatabaseDbContext())
            {
                var user = await db.DiscordUsers.Include(user => user.ValorantAccounts)
                    .FirstOrDefaultAsync(user => user.DiscordUserId == discordUser.Id);

                if (user == null || user.ValorantAccounts.Count == 0)
                {
                    await Context.Channel.SendMessageAsync("You have no connected accounts");
                    return;
                }

                var embed = new EmbedBuilder().WithTitle($"List of {user.Name} accounts");
                foreach (var valorantAccount in user.ValorantAccounts.OrderByDescending(val => val.Rank).ThenByDescending(val => val.RankProgress))
                {
                    var playerRank = await ValorantApiService.GetPlayerRank(valorantAccount.Subject);
                    valorantAccount.UpdateRank(playerRank);

                    var playerIDs = await ValorantApiService.GetPlayerIds(valorantAccount.Subject);
                    if (playerIDs != null)
                    {
                        valorantAccount.DisplayName = $"{playerIDs.Name}#{playerIDs.Tag}";
                    }

                    var guildEmote = Context.Guild.Emotes.FirstOrDefault(emote => emote.Name == valorantAccount.RankName.Replace(" ", ""));
                    embed.AddField("Name", valorantAccount.DisplayName, true);
                    embed.AddField("Rank", $"{guildEmote?.ToString() ?? ""}{valorantAccount.RankName}", true);
                    embed.AddField("Progress", $"{valorantAccount.RankProgress} / 100", true);


                    db.Update(valorantAccount);
                }

                await Context.Channel.SendMessageAsync(embed:embed.Build());

                await db.SaveChangesAsync();
            }

        }
    }
}