namespace DiscordBot.Models.Database
{
    public class RegisteredGuild
    {
        public int ID { get; set; }
        public ValorantAccount ValorantAccount { get; set; }
        public ulong GuildID { get; set; }
    }
}