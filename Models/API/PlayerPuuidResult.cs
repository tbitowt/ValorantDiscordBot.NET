namespace DiscordBot.Models.API
{
    public class PlayerPuuidResult
    {
        public long Status { get; set; }
        public Data Data { get; set; }
    }

    public class Data
    {
        public string Puuid { get; set; }
    }
}