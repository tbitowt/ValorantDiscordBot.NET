namespace DiscordBot.Models.Database
{
    public class RegisteredGuild
    {
        public int RegisteredGuildID { get; set; }
        public ValorantAccount ValorantAccount { get; set; }
        public ulong GuildID { get; set; }
    }
}