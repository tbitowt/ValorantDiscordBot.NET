using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Services
{
    public class PlayerRankChecker
    {
        private readonly ValorantApiService _valorantApiService;
        private readonly DiscordSocketClient _client;
        private Timer _timer;
        public PlayerRankChecker(ValorantApiService valorantApiService, DiscordSocketClient client)
        {
            _valorantApiService = valorantApiService;
            _client = client;
        }

        public void Start()
        {
            _timer = new System.Threading.Timer(
                async e =>
                {
                    await Update();
                },
                null,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromMinutes(15));
        }

        private async Task Update()
        {
            using (var db = new DatabaseDbContext())
            {
                foreach (var valorantAccount in db.ValorantAccount.Include(acc=> acc.DiscordUser).Include(acc => acc.RegisteredGuilds).Include(acc => acc.RankInfos))
                {
                    foreach (var valorantAccountRegisteredGuild in valorantAccount.RegisteredGuilds)
                    {
                        var guildConfig = db.GuildConfigs.FirstOrDefault(guild =>
                            (guild.Guild == valorantAccountRegisteredGuild.GuildID) && (guild.UpdatesChannel != null));
                        if (guildConfig != null)
                        {
                            var ch = _client.GetChannel(guildConfig.UpdatesChannel.Value);
                            var channel = _client.GetChannel(guildConfig.UpdatesChannel.Value) as ISocketMessageChannel;
                            if (channel != null)
                            {
                                var playerRank = await _valorantApiService.GetPlayerRank(valorantAccount.Subject);
                                
                                if (playerRank.RankInt < valorantAccount.Rank)
                                {
                                    await channel.SendMessageAsync(
                                        $"Account {valorantAccount.DisplayName} has been downgraded to {playerRank.RankString} . Current progress: {playerRank.Progress} / 100");
                                }

                                if (playerRank.RankInt > valorantAccount.Rank)
                                {
                                    await channel.SendMessageAsync(
                                        $"Account {valorantAccount.DisplayName} has been promoted to {playerRank.RankString} ! Current progress: {playerRank.Progress} / 100");
                                }

                                var info = new RankInfo()
                                {
                                    DateTime = DateTimeOffset.FromUnixTimeMilliseconds(playerRank.LastMatch.MatchStartTime).DateTime,
                                    Progress = (int)playerRank.LastMatch.RankedRatingAfterUpdate,
                                    RankInt = (int)playerRank.LastMatch.TierAfterUpdate,
                                    ValorantAccount = valorantAccount
                                };

                                if (info.Progress != 0 || info.RankInt != 0)
                                {
                                    if(valorantAccount.RankInfos.Any(rankInfo => rankInfo.DateTime == info.DateTime) == false)
                                        valorantAccount.RankInfos.Add(info);
                                    db.Update(valorantAccount);
                                    await db.SaveChangesAsync();
                                }
                                    
                                var playerRankChanged = playerRank.RankInt != valorantAccount.Rank;
                                if (playerRankChanged)
                                {
                                    valorantAccount.UpdateRank(playerRank);
                                    db.Update(valorantAccount);
                                    await db.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}