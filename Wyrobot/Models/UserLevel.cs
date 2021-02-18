namespace Wyrobot.Core.Models
{
    public class UserLevel
    {
        public UserLevel(ulong userId = default, ulong guildId = default, int xp = default, int level = default, int xpToNextLevel = default)
        {
            UserId = userId;
            GuildId = guildId;
            Xp = xp;
            Level = level;
            XpToNextLevel = xpToNextLevel;
        }
        
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public int Xp { get; set; }
        public int Level { get; set; }
        public int XpToNextLevel { get; set; }
    }
}