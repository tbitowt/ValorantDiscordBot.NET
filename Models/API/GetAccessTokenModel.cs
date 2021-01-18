namespace DiscordBot.Models.API
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Parameters    {
        public string uri { get; set; } 
    }

    public class Response    {
        public string mode { get; set; } 
        public Parameters parameters { get; set; } 
    }

    public class GetAccessTokenModel    {
        public string type { get; set; } 
        public Response response { get; set; } 
        public string country { get; set; } 
    }


}