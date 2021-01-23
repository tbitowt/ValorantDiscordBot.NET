using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DiscordBot.Models;
using DiscordBot.Models.API;
using DiscordBot.Models.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using RestSharp;
using RestSharp.Serialization.Json;
using RestSharpPolly;
using RestSharpPolly.PolicyProviders;

namespace DiscordBot.Services
{
    public class ValorantApiService : IValorantApiService
    {
        
        private RestClientFactory<IRestResponse> RestClient => RestClientFactory<IRestResponse>.Create(_retryPolicy);
        private CookieContainer _cookieContainer = new();
        private EnvCheckerService _envCheckerService;
        private string _region;
        public string AccessToken { get; private set; }
        public string EntitlementToken { get; private set; }
        private AsyncRetryPolicy<IRestResponse> _retryPolicy;
        
        private String[] _rank_map = new String[]
        {
            "Unrated", "Unknown", "Unknown", "Iron 1", "Iron 2", "Iron 3", "Bronze 1", "Bronze 2", "Bronze 3",
            "Silver 1", "Silver 2", "Silver 3", "Gold 1", "Gold 2", "Gold 3", "Platinum 1", "Platinum 2",
            "Platinum 3", "Diamond 1", "Diamond 2", "Diamond 3", "Immortal 1", "Immortal 2", "Immortal 3",
            "Radiant"
        };

        public ValorantApiService(EnvCheckerService envCheckerService)
        {
            RestClient.CookieContainer = _cookieContainer;
            RestClient.AddHandler("text/plain", new JsonDeserializer());
            RestClient.AllowMultipleDefaultParametersWithSameName = false;


            _envCheckerService = envCheckerService;
            _retryPolicy = Policy.HandleResult<IRestResponse>(r => r.IsSuccessful == false).RetryAsync(async (result, i) => await Login());
        }

        public void SetRegion(string region)
        {
            _region = region;
        }

        private Uri GetBasePath()
        {
            return new Uri($"https://pd.{_region}.a.pvp.net");
        }

        private async Task<bool> GetAuthorization()
        {
            RestRequest request = new RestRequest("https://auth.riotgames.com/api/v1/authorization", Method.POST);
            var body = new
            {
                client_id = "play-valorant-web-prod",
                nonce = "1",
                redirect_uri = "https://playvalorant.com/opt_in",
                response_type = "token id_token"
            };
            request.AddJsonBody(body);
            var response = await RestClient.ExecuteAsync(request);
            if (response.IsSuccessful == false)
            {
                Console.WriteLine("GetAuthorization result is unsuccessful");
            }
            return response.IsSuccessful;
        }

        private async Task<bool> GetAccessToken(string username, string password)
        {

            RestRequest request = new RestRequest("https://auth.riotgames.com/api/v1/authorization", Method.PUT);
            string body = "{\"type\":\"auth\",\"username\":\"" + username + "\",\"password\":\"" + password + "\"}";
            request.AddJsonBody(body);
            
            var result = await RestClient.ExecuteAsync<GetAccessTokenModel>(request);

            if (result.IsSuccessful)
            {
                var data = result.Data;
                string authURL = data.response.parameters.uri;
                var tokenMatch  = Regex.Match(authURL, @"access_token=(.+?)&scope=");
                if (tokenMatch.Success)
                {
                    var token = tokenMatch.Groups[1].Value;
                    AccessToken = token;
                    RestClient.AddOrUpdateDefaultParameter(new Parameter("Authorization", $"Bearer {AccessToken}", ParameterType.HttpHeader));
                    return true;
                }
            }
            Console.WriteLine("GetAccessToken result is unsuccessful");
            return false;
        }

        private async Task<bool> GetEntitlementToken()
        {
            RestRequest request = new RestRequest("https://entitlements.auth.riotgames.com/api/token/v1", Method.POST);
            request.AddJsonBody("{}");
            var response = await RestClient.ExecuteAsync<GetEntitlementsTokenModel>(request);
            if (response.IsSuccessful)
            {
                EntitlementToken = response.Data.EntitlementsToken;
                RestClient.AddOrUpdateDefaultParameter(new Parameter("X-Riot-Entitlements-JWT", EntitlementToken, ParameterType.HttpHeader));

                return true;
            }
            Console.WriteLine("GetEntitlementToken result is unsuccessful");
            return false;
        }
        
        public async Task<bool> Login()
        {
            using (var db = new DatabaseDbContext())
            {
                foreach (var customHeader in db.CustomHeaders)
                {
                    RestClient.AddOrUpdateDefaultParameter(new Parameter(customHeader.Name, customHeader.Value, ParameterType.HttpHeader));
                }
            }

            _cookieContainer = new CookieContainer();
            RestClient.CookieContainer = _cookieContainer;


            if (await GetAuthorization() == false)
            {
                return false;
            }
            if (await GetAccessToken(_envCheckerService.Valorant_UserName, _envCheckerService.Valorant_Password) == false)
            {
                return false;
            }

            if (await GetEntitlementToken() == false)
            {
                return false;
            }

            
            return true;
        }

        public async Task<PlayerRank> GetPlayerRank(string playerId)
        {
            RestRequest request = new RestRequest($"{GetBasePath()}/mmr/v1/players/{playerId}", Method.GET);

            var response = await RestClient.ExecuteAsync<GetPlayerMmr>(request);
            
            if(response.IsSuccessful)
            {
                var result = new PlayerRank();
                result.RankInt = response.Data.QueueSkills.Competitive.CompetitiveTier;
                result.RankString = _rank_map[result.RankInt];
                result.Progress = response.Data.QueueSkills.Competitive.TierProgress;
                result.MatchesLeftForRank = response.Data.QueueSkills.Competitive.CurrentSeasonGamesNeededForRating;

                var dataLatestCompetitiveUpdate = response.Data.LatestCompetitiveUpdate;
                result.LastMatch = new DiscordBot.Models.API.Match()
                {
                    MatchStartTime = dataLatestCompetitiveUpdate.MatchStartTime,
                    TierAfterUpdate = dataLatestCompetitiveUpdate.TierAfterUpdate,
                    RankedRatingAfterUpdate = dataLatestCompetitiveUpdate.RankedRatingAfterUpdate
                };
                return result;
            }
            Console.WriteLine("GetPlayerRank result is unsuccessful");
            return null;
        }

        public async Task<ValorantPlayerIds> GetPlayerIds(string playerId)
        {
            RestRequest request = new RestRequest($"{GetBasePath()}/name-service/v2/players", Method.PUT);
            var ids = new string[] {playerId};
            request.AddJsonBody(ids);
            var result = await RestClient.ExecuteAsync<List<PlayerIDs>>(request);

            if (result.IsSuccessful)
            {
                var valorantPlayerIds = new ValorantPlayerIds()
                {
                    Name = result.Data[0].GameName,
                    Tag = result.Data[0].TagLine
                };
                return valorantPlayerIds;
            }
            Console.WriteLine("GetPlayerIds result is unsuccessful");
            return null;
        }

        public async Task<List<RankInfo>> GetPlayerRankHistory(ValorantAccount account, DateTime beginDateTime)
        {
            RestRequest request = new RestRequest($"{GetBasePath()}/mmr/v1/players/{account.Subject}/competitiveupdates", Method.GET);

            
            var result = new List<RankInfo>();
            int startIndex = 0;
            do
            {
                request.AddOrUpdateParameter("startIndex", startIndex);
                request.AddOrUpdateParameter("endIndex", startIndex + 20);
                startIndex += 20;
                var response = await RestClient.ExecuteAsync<MatchHistory>(request);
                if (response.IsSuccessful)
                {
                    if (response.Data.Matches.Count == 0)
                    {
                        break;
                    }

                    foreach (var dataMatch in response.Data.Matches)
                    {
                        var info = new RankInfo()
                        {
                            DateTime = DateTimeOffset.FromUnixTimeMilliseconds(dataMatch.MatchStartTime).DateTime,
                            Progress = (int) dataMatch.RankedRatingAfterUpdate,
                            RankInt = (int) dataMatch.TierAfterUpdate,
                            ValorantAccount = account
                        };
                        
                        if (info.DateTime < beginDateTime)
                            break;
                        
                        if(info.Progress == 0 && info.RankInt == 0)
                            continue;

                        result.Add(info);
                        
                    }
                }
                else
                {
                    Console.WriteLine("GetPlayerRankHistory result is unsuccessful");
                    break;
                }
            } while (true);


            return result;
        }
    }
}