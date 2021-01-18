using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using DiscordBot.Models;
using DiscordBot.Models.API;
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
            "Unknown", "Unknown", "Unknown 2", "Iron 1", "Iron 2", "Iron 3", "Bronze 1", "Bronze 2", "Bronze 3",
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
        
        private bool GetAuthorization()
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
            var response = _restClient.Execute(request);
            return response.IsSuccessful;
        }

        private bool GetAccessToken(string username, string password)
        {
            _restClient.BaseUrl = new Uri("https://auth.riotgames.com/api/v1/authorization");

            RestRequest request = new RestRequest(Method.PUT);
            string body = "{\"type\":\"auth\",\"username\":\"" + username + "\",\"password\":\"" + password + "\"}";
            request.AddJsonBody(body);

            var result = _restClient.Execute<GetAccessTokenModel>(request);
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

            return false;
        }

        private bool GetEntitlementToken()
        {
            _restClient.BaseUrl = new Uri("https://entitlements.auth.riotgames.com/api/token/v1");
            RestRequest request = new RestRequest(Method.POST);
            request.AddJsonBody("{}");
            var response = _restClient.Execute<GetEntitlementsTokenModel>(request);
            if (response.IsSuccessful)
            {
                EntitlementToken = response.Data.EntitlementsToken;
                _restClient.AddDefaultHeader("X-Riot-Entitlements-JWT", EntitlementToken);
                return true;
            }

            return false;
        }
        
        public bool Login()
        {
            if (GetAuthorization() == false)
            {
                return false;
            }
            if (GetAccessToken(_envCheckerService.Valorant_UserName, _envCheckerService.Valorant_Password) == false)
            {
                return false;
            }

            if (GetEntitlementToken() == false)
            {
                return false;
            }

            _restClient.AddDefaultHeader("X-Riot-ClientVersion", "release-02.00-shipping-16-508517");
            return true;
        }

        public PlayerRank GetPlayerRank(string playerId)
        {
            _restClient.BaseUrl = GetBasePath();
            RestRequest request = new RestRequest($"/mmr/v1/players/{playerId}", Method.GET);
            var response = _restClient.Execute<GetPlayerMmr>(request);
            if(response.IsSuccessful)
            {
                var result = new PlayerRank();
                result.RankInt = response.Data.QueueSkills.Competitive.CompetitiveTier;
                result.RankString = _rank_map[result.RankInt];
                result.Progress = response.Data.QueueSkills.Competitive.TierProgress;
                result.MatchesLeftForRank = response.Data.QueueSkills.Competitive.TotalGamesNeededForRating;
                return result;
            }
            return null;
        }
    }
}