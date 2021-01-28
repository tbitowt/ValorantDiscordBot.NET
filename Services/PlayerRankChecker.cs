using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscordBot.Models.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services
{
    public class PlayerRankChecker
    {
        private readonly BotService _botService;
        private readonly ValorantApiService _valorantApiService;

        private Timer _timer;
        private int _currentAccount = 0;

        public PlayerRankChecker(ValorantApiService valorantApiService, BotService botService,
            ILogger<PlayerRankChecker> logger)
        {
            Logger = logger;
            _valorantApiService = valorantApiService;
            _botService = botService;
        }

        public ILogger<PlayerRankChecker> Logger { get; }

        public void Start()
        {
            _timer = new Timer(
                async e => { await Update(); },
                null,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(30));
        }

        private async Task Update()
        {
            using (var db = new DatabaseDbContext())
            {
                var accounts = db.ValorantAccount.Include(acc => acc.DbDiscordUser)
                    .Include(acc => acc.RegisteredGuilds).Include(acc => acc.RankInfos).ToList();

                var valorantAccount = accounts[_currentAccount];
                Logger.LogInformation($"Running RankChecker on player {valorantAccount.DisplayName}");

                _currentAccount++;
                if (_currentAccount >= accounts.Count)
                    _currentAccount = 0;

                var playerRank = await _valorantApiService.GetPlayerRank(valorantAccount.Subject);
                if (playerRank == null)
                {
                    Logger.LogError("Could not update playerRank");
                    return;
                }

                foreach (var valorantAccountRegisteredGuild in valorantAccount.RegisteredGuilds)
                {
                    var guildConfig = db.GuildConfigs.FirstOrDefault(guild =>
                        guild.Guild == valorantAccountRegisteredGuild.GuildID && guild.UpdatesChannel != null);
                    if (guildConfig != null)
                    {
                        var channel = await _botService.DiscordClient.GetChannelAsync(guildConfig.UpdatesChannel.Value);
                        if (channel != null)
                        {
                            if (playerRank.RankInt < valorantAccount.Rank)
                                await channel.SendMessageAsync(
                                    $"Account {valorantAccount.DisplayName} has been downgraded to {playerRank.RankString} . Current progress: {playerRank.Progress} / 100");

                            if (playerRank.RankInt > valorantAccount.Rank)
                                await channel.SendMessageAsync(
                                    $"Account {valorantAccount.DisplayName} has been promoted to {playerRank.RankString} ! Current progress: {playerRank.Progress} / 100");

                            var info = new RankInfo
                            {
                                DateTime = DateTimeOffset.FromUnixTimeMilliseconds(playerRank.LastMatch.MatchStartTime)
                                    .DateTime,
                                Progress = (int) playerRank.LastMatch.RankedRatingAfterUpdate,
                                RankInt = (int) playerRank.LastMatch.TierAfterUpdate,
                                ValorantAccount = valorantAccount
                            };

                            if (info.Progress != 0 || info.RankInt != 0)
                            {
                                if (valorantAccount.RankInfos.Any(rankInfo => rankInfo.DateTime == info.DateTime) ==
                                    false)
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