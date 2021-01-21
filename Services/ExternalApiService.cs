using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DiscordBot.Models.API;
using RestSharp;

namespace DiscordBot.Services
{
    public class ExternalApiService
    {
        private readonly RestClient _restClient = new();
        private readonly CookieContainer _cookieContainer = new();

        public ExternalApiService()
        {
            _restClient.CookieContainer = _cookieContainer;
        }

        public async Task<string> GetPlayerPuuid(string name, string tag)
        {
            _restClient.BaseUrl = new Uri("https://api.henrikdev.xyz/valorant/");
            RestRequest request = new RestRequest($"/v1/puuid/{name}/{tag}", Method.GET);
            var result = await _restClient.ExecuteAsync<PlayerPuuidResult>(request);

            if (result.IsSuccessful)
            {
                return result.Data.Data.Puuid;
            }
            Console.WriteLine("GetPlayerPuuid result is unsuccessful");
            return null;
        }
    }

}