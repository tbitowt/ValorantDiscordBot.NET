using System.ComponentModel.DataAnnotations;

namespace DiscordBot.Models.Database
{
    public class ValorantAccount
    {
        [Key]
        public string Subject { get; set; }
        public string DisplayName { get; set; }

        public int Rank { get; set; }
        public string RankName { get; set; }
        public int RankProgress { get; set; }
    }
}