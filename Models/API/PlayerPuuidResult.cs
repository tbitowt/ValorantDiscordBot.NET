namespace DiscordBot.Models.API
{

    public partial class PlayerPuuidResult
    {
        public long Status { get; set; }
        public Data Data { get; set; }
    }

    public partial class Data
    {
        public string Puuid { get; set; }
    }
}