namespace Wyrobot.Core.Models
{
    public class LevelReward
    {
        public LevelReward(int requiredLevel = default, ulong roleId = default)
        {
            RequiredLevel = requiredLevel;
            RoleId = roleId;
        }

        public int RequiredLevel { get; }
        public ulong RoleId { get; }
    }
}