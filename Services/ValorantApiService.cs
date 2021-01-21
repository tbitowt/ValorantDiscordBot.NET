using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordBot.Models;
using DiscordBot.Models.API;
using DiscordBot.Models.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serialization.Json;

namespace DiscordBot.Services
{
    public class ValorantApiService : IValorantApiService
    {
        
        private readonly RestClient _restClient = new();
        private readonly CookieContainer _cookieContainer = new();
        private EnvCheckerService _envCheckerService;
        private string _region;
        public string AccessToken { get; private set; }
        public string EntitlementToken { get; private set; }
        
        private String[] _rank_map = new String[]
        {
            "Unrated", "Unknown", "Unknown", "Iron 1", "Iron 2", "Iron 3", "Bronze 1", "Bronze 2", "Bronze 3",
            "Silver 1", "Silver 2", "Silver 3", "Gold 1", "Gold 2", "Gold 3", "Platinum 1", "Platinum 2",
            "Platinum 3", "Diamond 1", "Diamond 2", "Diamond 3", "Immortal 1", "Immortal 2", "Immortal 3",
            "Radiant"
        };
        
        public ValorantApiService(EnvCheckerService envCheckerService)
        {
            _restClient.CookieContainer = _cookieContainer;
            _restClient.AddHandler("text/plain", new JsonDeserializer());
            _envCheckerService = envCheckerService;
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
            _restClient.BaseUrl = new Uri("https://auth.riotgames.com/api/v1/authorization");
            RestRequest request = new RestRequest(Method.POST);
            var body = new
            {
                client_id = "play-valorant-web-prod",
                nonce = "1",
                redirect_uri = "https://playvalorant.com/opt_in",
                response_type = "token id_token"
            };
            request.AddJsonBody(body);
            var response = await _restClient.ExecuteAsync(request);
            if (response.IsSuccessful == false)
            {
                Console.WriteLine("GetAuthorization result is unsuccessful");
            }
            return response.IsSuccessful;
        }

        private async Task<bool> GetAccessToken(string username, string password)
        {
            _restClient.BaseUrl = new Uri("https://auth.riotgames.com/api/v1/authorization");

            RestRequest request = new RestRequest(Method.PUT);
            string body = "{\"type\":\"auth\",\"username\":\"" + username + "\",\"password\":\"" + password + "\"}";
            request.AddJsonBody(body);

            var result = await _restClient.ExecuteAsync<GetAccessTokenModel>(request);
            if (result.IsSuccessful)
            {
                var data = result.Data;
                string authURL = data.response.parameters.uri;
                var tokenMatch  = Regex.Match(authURL, @"access_token=(.+?)&scope=");
                if (tokenMatch.Success)
                {
                    var token = tokenMatch.Groups[1].Value;
                    AccessToken = token;
                    _restClient.AddDefaultHeader("Authorization", $"Bearer {AccessToken}");
                    return true;
                }
            }
            Console.WriteLine("GetAccessToken result is unsuccessful");
            return false;
        }

        private async Task<bool> GetEntitlementToken()
        {
            _restClient.BaseUrl = new Uri("https://entitlements.auth.riotgames.com/api/token/v1");
            RestRequest request = new RestRequest(Method.POST);
            request.AddJsonBody("{}");
            var response = await _restClient.ExecuteAsync<GetEntitlementsTokenModel>(request);
            if (response.IsSuccessful)
            {
                EntitlementToken = response.Data.EntitlementsToken;
                _restClient.AddDefaultHeader("X-Riot-Entitlements-JWT", EntitlementToken);
                return true;
            }
            Console.WriteLine("GetEntitlementToken result is unsuccessful");
            return false;
        }
        
        public async Task<bool> Login()
        {
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

            using (var db = new DatabaseDbContext())
            {
                foreach (var customHeader in db.CustomHeaders)
                {
                    _restClient.AddDefaultHeader(customHeader.Name, customHeader.Value);
                }
            }
            return true;
        }

        public async Task<PlayerRank> GetPlayerRank(string playerId)
        {
            _restClient.BaseUrl = GetBasePath();
            RestRequest request = new RestRequest($"/mmr/v1/players/{playerId}", Method.GET);
            var response = await _restClient.ExecuteAsync<GetPlayerMmr>(request);
            if(response.IsSuccessful)
            {
                var result = new PlayerRank();
                result.RankInt = response.Data.QueueSkills.Competitive.CompetitiveTier;
                result.RankString = _rank_map[result.RankInt];
                result.Progress = response.Data.QueueSkills.Competitive.TierProgress;
                result.MatchesLeftForRank = response.Data.QueueSkills.Competitive.CurrentSeasonGamesNeededForRating;
                return result;
            }
            Console.WriteLine("GetPlayerRank result is unsuccessful");
            return null;
        }

        public async Task<ValorantPlayerIds> GetPlayerIds(string playerId)
        {
            _restClient.BaseUrl = GetBasePath();
            RestRequest request = new RestRequest("/name-service/v2/players", Method.PUT);
            var ids = new string[] {playerId};
            request.AddJsonBody(ids);
            var result = await _restClient.ExecuteAsync<List<PlayerIDs>>(request);

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
    }
}