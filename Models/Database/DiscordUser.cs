using System.Collections.Generic;
using DiscordBot.Services;

namespace DiscordBot.Models.Database
{
    public class DiscordUser
    {
        public ulong DiscordUserId { get; set; }
        public string Name { get; set; }

        public List<ValorantAccount> ValorantAccounts { get; } = new List<ValorantAccount>();
    }
}