using System.Collections.Generic;

namespace DiscordBot.Models.Database
{
    public class DbDiscordUser
    {
        public ulong ID { get; set; }
        public string Name { get; set; }

        public List<ValorantAccount> ValorantAccounts { get; } = new();
    }
}