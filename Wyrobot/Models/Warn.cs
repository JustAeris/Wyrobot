using System;

namespace Wyrobot.Core.Models
{
    public class Warn
    {
        /// <summary>
        /// The Ban object is used to represent a banned user
        /// </summary>
        /// <param name="userId">User's ID</param>
        /// <param name="guildId">Guild's ID the user is banned from</param>
        /// <param name="issuedAt">DateTime the user has been banned at</param>
        /// <param name="expiresAt">DateTime the ban expires at, null for permanent bans</param>
        /// <param name="username">User username including the 4-digits tag</param>
        /// <param name="reason">Reason for the ban</param>
        public Warn(ulong userId = default, ulong guildId = default, DateTime issuedAt = default, DateTime expiresAt = default, string username = default, string reason = default)
        {
            UserId = userId;
            GuildId = guildId;
            IssuedAt = issuedAt;
            ExpiresAt = expiresAt;
            Username = username;
            Reason = reason;
        }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Username { get; set; }
        public string Reason { get; set; }
    }
    
    public class ExpiredWarn : Warn
    {
        public ExpiredWarn(ulong userId = default, ulong guildId = default, DateTime issuedAt = default, DateTime expiresAt = default, string username = default, string reason = default)
        {
            UserId = userId;
            GuildId = guildId;
            IssuedAt = issuedAt;
            ExpiresAt = expiresAt;
            Username = username;
            Reason = reason;
        }
    }
}