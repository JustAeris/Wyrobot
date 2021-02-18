namespace Wyrobot.Core.Models
{
    public class ReactionMessage
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong Id { get; set; }
        public Action Action { get; set; }
        public ulong Role { get; set; }
    }
    
    public enum Action
    {
        Grant,
        Revoke,
        Suggestion
    }
}