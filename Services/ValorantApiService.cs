using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DiscordBot.Services
{
    public class ValorantApiService
    {
        private RestClient restClient = new();
        private CookieContainer cookieContainer = new();
        private string accessToken;
        private string entitlementToken;
        public ValorantApiService()
        {
            restClient.CookieContainer = cookieContainer;
        }

        private bool GetAuthorization()
        {
            restClient.BaseUrl = new Uri("https://auth.riotgames.com/api/v1/authorization");
            RestRequest request = new RestRequest(Method.POST);
            var body = new
            {
                client_id = "play-valorant-web-prod",
                nonce = "1",
                redirect_uri = "https://playvalorant.com/opt_in",
                response_type = "token id_token"
            };
            request.AddJsonBody(body);
            var response = restClient.Execute(request);
            return response.IsSuccessful;
        }

        private bool GetAccessToken(string username, string password)
        {
            restClient.BaseUrl = new Uri("https://auth.riotgames.com/api/v1/authorization");

            RestRequest request = new RestRequest(Method.PUT);
            string body = "{\"type\":\"auth\",\"username\":\"" + username + "\",\"password\":\"" + password + "\"}";
            request.AddJsonBody(body);

            var result = restClient.Execute(request).Content;
            var authJson = JsonConvert.DeserializeObject(result);
            var authObj = JObject.FromObject(authJson);
            string authURL = authObj["response"]["parameters"]["uri"].Value<string>();
            var tokenMatch  = Regex.Match(authURL, @"access_token=(.+?)&scope=");
            if (tokenMatch.Success)
            {
                var token = tokenMatch.Groups[1].Value;
                accessToken = token;
                restClient.AddDefaultHeader("Authorization", $"Bearer {token}");
                return true;
            }
            else
            {
                return false;
            }
            
        }

        private bool GetEntitlementToken()
        {
            restClient.BaseUrl = new Uri("https://entitlements.auth.riotgames.com/api/token/v1");
            RestRequest request = new RestRequest(Method.POST);
            request.AddJsonBody("{}");
            var response = restClient.Execute(request);
            if (response.IsSuccessful)
            {
                var entitlement_token = JsonConvert.DeserializeObject(response.Content);
                JToken entitlement_tokenObj = JObject.FromObject(entitlement_token);

                entitlementToken = entitlement_tokenObj["entitlements_token"].Value<string>();
                restClient.AddDefaultHeader("X-Riot-Entitlements-JWT", entitlementToken);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Login(string username, string password)
        {
            if (GetAuthorization() == false)
            {
                return false;
            }
            if (GetAccessToken(username, password) == false)
            {
                return false;
            }

            if (GetEntitlementToken() == false)
            {
                return false;
            }
            return true;
        }
    }
}