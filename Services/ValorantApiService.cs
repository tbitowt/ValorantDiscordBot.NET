using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DiscordBot.Models.API;
using DiscordBot.Models.Database;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RestSharp;
using RestSharp.Serialization.Json;
using RestSharpPolly;
using Match = DiscordBot.Models.API.Match;

namespace DiscordBot.Services
{
    public class ValorantApiService : IValorantApiService
    {
        public ILogger<ValorantApiService> Logger { get; }
        private CookieContainer _cookieContainer = new();
        private readonly EnvCheckerService _envCheckerService;

        private readonly string[] _rank_map =
        {
            "Unrated", "Unknown", "Unknown", "Iron 1", "Iron 2", "Iron 3", "Bronze 1", "Bronze 2", "Bronze 3",
            "Silver 1", "Silver 2", "Silver 3", "Gold 1", "Gold 2", "Gold 3", "Platinum 1", "Platinum 2",
            "Platinum 3", "Diamond 1", "Diamond 2", "Diamond 3", "Immortal 1", "Immortal 2", "Immortal 3",
            "Radiant"
        };

        private string _region;
        private readonly IAsyncPolicy<IRestResponse> _retryPolicy;

        public ValorantApiService(EnvCheckerService envCheckerService, ILogger<ValorantApiService> logger)
        {
            Logger = logger;
            RestClient.CookieContainer = _cookieContainer;
            RestClient.AddHandler("text/plain", new JsonDeserializer());
            RestClient.AllowMultipleDefaultParametersWithSameName = false;


            _envCheckerService = envCheckerService;
            var rateLimitedPolicy = Policy.HandleResult<IRestResponse>(p => p.StatusCode == HttpStatusCode.TooManyRequests)
                .FallbackAsync(FallbackAction, OnFallback);

            var loginRetryPolicy = Policy.HandleResult<IRestResponse>(r => r.IsSuccessful == false && r.StatusCode != HttpStatusCode.TooManyRequests)
                .RetryAsync(async (result, i) => await Login());

            _retryPolicy  = rateLimitedPolicy.WrapAsync(loginRetryPolicy);
            

        }

        private async Task OnFallback(DelegateResult<IRestResponse> response, Context arg2)
        {
            var retryHeader = response.Result.Headers.SingleOrDefault(h => h.Name == "Retry-After");

            Logger.LogError($"Valorant API request was rate-limited. Should retry in: {retryHeader?.Value}");
            return;
        }

        private async Task<IRestResponse> FallbackAction(DelegateResult<IRestResponse> response, Context arg2, CancellationToken arg3)
        {
            return response.Result;
        }

        private RestClientFactory<IRestResponse> RestClient => RestClientFactory<IRestResponse>.Create(_retryPolicy);
        public string AccessToken { get; private set; }
        public string EntitlementToken { get; private set; }

        public async Task<bool> Login()
        {
            using (var db = new DatabaseDbContext())
            {
                foreach (var customHeader in db.CustomHeaders)
                    RestClient.AddOrUpdateDefaultParameter(new Parameter(customHeader.Name, customHeader.Value,
                        ParameterType.HttpHeader));
            }

            _cookieContainer = new CookieContainer();
            RestClient.CookieContainer = _cookieContainer;


            if (await GetAuthorization() == false) return false;
            if (await GetAccessToken(_envCheckerService.Valorant_UserName, _envCheckerService.Valorant_Password) ==
                false) return false;

            if (await GetEntitlementToken() == false) return false;


            return true;
        }

        public async Task<PlayerRank> GetPlayerRank(string playerId)
        {
            var request = new RestRequest($"{GetBasePath()}/mmr/v1/players/{playerId}", Method.GET);

            var response = await RestClient.ExecuteAsync<GetPlayerMmr>(request);
            if (response.IsSuccessful)
            {
                var result = new PlayerRank();
                result.RankInt = response.Data.QueueSkills.Competitive.CompetitiveTier;
                result.RankString = _rank_map[result.RankInt];
                result.Progress = response.Data.QueueSkills.Competitive.TierProgress;
                result.MatchesLeftForRank = response.Data.QueueSkills.Competitive.CurrentSeasonGamesNeededForRating;
                if (response.Data.LatestCompetitiveUpdate != null)
                {
                    var dataLatestCompetitiveUpdate = response.Data.LatestCompetitiveUpdate;
                    result.LastMatch = new Match
                    {
                        MatchStartTime = dataLatestCompetitiveUpdate.MatchStartTime,
                        TierAfterUpdate = dataLatestCompetitiveUpdate.TierAfterUpdate,
                        RankedRatingAfterUpdate = dataLatestCompetitiveUpdate.RankedRatingAfterUpdate
                    };
                }

                return result;
            }

            Console.WriteLine("GetPlayerRank result is unsuccessful");
            return null;
        }

        public async Task<ValorantPlayerIds> GetPlayerIds(string playerId)
        {
            var request = new RestRequest($"{GetBasePath()}/name-service/v2/players", Method.PUT);
            var ids = new[] {playerId};
            request.AddJsonBody(ids);
            var result = await RestClient.ExecuteAsync<List<PlayerIDs>>(request);

            if (result.IsSuccessful)
            {
                var valorantPlayerIds = new ValorantPlayerIds
                {
                    Name = result.Data[0].GameName,
                    Tag = result.Data[0].TagLine
                };
                return valorantPlayerIds;
            }

            Console.WriteLine("GetPlayerIds result is unsuccessful");
            return null;
        }

        public void SetRegion(string region)
        {
            _region = region;
        }

        private Uri GetBasePath()
        {
            return new($"https://pd.{_region}.a.pvp.net");
        }

        private async Task<bool> GetAuthorization()
        {
            var request = new RestRequest("https://auth.riotgames.com/api/v1/authorization", Method.POST);
            var body = new
            {
                client_id = "play-valorant-web-prod",
                nonce = "1",
                redirect_uri = "https://playvalorant.com/opt_in",
                response_type = "token id_token"
            };
            request.AddJsonBody(body);
            var response = await RestClient.ExecuteAsync(request);
            if (response.IsSuccessful == false) Console.WriteLine("GetAuthorization result is unsuccessful");
            return response.IsSuccessful;
        }

        private async Task<bool> GetAccessToken(string username, string password)
        {
            var request = new RestRequest("https://auth.riotgames.com/api/v1/authorization", Method.PUT);
            var body = "{\"type\":\"auth\",\"username\":\"" + username + "\",\"password\":\"" + password + "\"}";
            request.AddJsonBody(body);

            var result = await RestClient.ExecuteAsync<GetAccessTokenModel>(request);

            if (result.IsSuccessful)
            {
                var data = result.Data;
                var authURL = data.response.parameters.uri;
                var tokenMatch = Regex.Match(authURL, @"access_token=(.+?)&scope=");
                if (tokenMatch.Success)
                {
                    var token = tokenMatch.Groups[1].Value;
                    AccessToken = token;
                    RestClient.AddOrUpdateDefaultParameter(new Parameter("Authorization", $"Bearer {AccessToken}",
                        ParameterType.HttpHeader));
                    return true;
                }
            }

            Console.WriteLine("GetAccessToken result is unsuccessful");
            return false;
        }

        private async Task<bool> GetEntitlementToken()
        {
            var request = new RestRequest("https://entitlements.auth.riotgames.com/api/token/v1", Method.POST);
            request.AddJsonBody("{}");
            var response = await RestClient.ExecuteAsync<GetEntitlementsTokenModel>(request);
            if (response.IsSuccessful)
            {
                EntitlementToken = response.Data.EntitlementsToken;
                RestClient.AddOrUpdateDefaultParameter(new Parameter("X-Riot-Entitlements-JWT", EntitlementToken,
                    ParameterType.HttpHeader));

                return true;
            }

            Console.WriteLine("GetEntitlementToken result is unsuccessful");
            return false;
        }

        public async Task<List<RankInfo>> GetPlayerRankHistory(ValorantAccount account, DateTime beginDateTime)
        {
            var request = new RestRequest($"{GetBasePath()}/mmr/v1/players/{account.Subject}/competitiveupdates",
                Method.GET);


            var result = new List<RankInfo>();
            var startIndex = 0;
            do
            {
                request.AddOrUpdateParameter("startIndex", startIndex);
                request.AddOrUpdateParameter("endIndex", startIndex + 20);
                startIndex += 20;
                var response = await RestClient.ExecuteAsync<MatchHistory>(request);
                if (response.IsSuccessful)
                {
                    if (response.Data.Matches.Count == 0) break;

                    foreach (var dataMatch in response.Data.Matches)
                    {
                        var info = new RankInfo
                        {
                            DateTime = DateTimeOffset.FromUnixTimeMilliseconds(dataMatch.MatchStartTime).DateTime,
                            Progress = (int) dataMatch.RankedRatingAfterUpdate,
                            RankInt = (int) dataMatch.TierAfterUpdate,
                            ValorantAccount = account
                        };

                        if (info.DateTime < beginDateTime)
                            return result;

                        if (info.Progress == 0 && info.RankInt == 0)
                            continue;

                        result.Add(info);
                    }
                }
                else
                {
                    Console.WriteLine("GetPlayerRankHistory result is unsuccessful or match history ended");
                    break;
                }
            } while (true);


            return result;
        }
    }
}