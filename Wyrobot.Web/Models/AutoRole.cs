using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace Wyrobot.Web.Models
{
    [Table("auto_role")]
    public class AutoRole
    {
        [Column("pk")]
        public int Id { get; set; }
        [Column("guild_id")]
        public ulong GuildId { get; set; }
        [Column("role_id")]
        public ulong RoleId { get; set; }
        [NotMapped]
        public string RoleName { get; set; }
    }
}