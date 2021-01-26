using System.ComponentModel.DataAnnotations;

namespace DiscordBot.Models.Database
{
    public class CustomHeader
    {
        [Key] public string Name { get; set; }

        public string Value { get; set; }
    }
}