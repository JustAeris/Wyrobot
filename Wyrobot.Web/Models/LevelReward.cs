using System.ComponentModel.DataAnnotations.Schema;

namespace Wyrobot.Web.Models
{
    [Table("leveling_rewards")]
    public class LevelReward
    {
        [Column("pk")]
        public int Id { get; set; }
        [Column("guild_id")]
        public ulong GuildId { get; set; }
        [Column("level_required")]
        public int RequiredLevel { get; set; }
        [Column("role_reward")]
        public ulong RoleId { get; set; }
        [NotMapped]
        public string RoleName { get; set; }
    }
}