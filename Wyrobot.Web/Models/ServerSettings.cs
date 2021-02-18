using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Wyrobot.Web.Models
{
    [Table("server_info")]   
    public class ServerSettings
    {
        [Column("guild_id"), Key]
        public ulong GuildId { get; set; }
        [Column("guild_name"), Display(Name = "Guild Name")]
        public string GuildName { get; set; }
        
        [Column("mute_role_id"), RegularExpression("^[0-9]*$"), Display(Name = "Mute Role Id")]
        public ulong MuteRoleId { get; set; }
        [Column("moderation_roles"), Display(Name = "Moderation Roles")]
        public string ModerationRoles { get; set; }
        [Column("banned_words"), Display(Name = "Banned Words")]
        public string BannedWords { get; set; }
        [Column("caps_percentage"), Display(Name = "Caps Percentage")]
        public float CapsPercentage { get; set; }
        [Column("automoderation_enabled"), Display(Name = "Auto-Moderation")]
        public bool AutoModerationEnabled { get; set; }
        [Column("mute_after_3_warns"), Display(Name = "Mute After 3 Warns")]
        public bool MuteAfter3Warn { get; set; }
        
        // Welcome Settings
        [Column("welcome_message_enabled"), Display(Name = "Welcoming Message")]
        public bool WelcomeEnabled { get; set; }
        [Column("welcome_channel_id"), RegularExpression("^[0-9]*$"), Display(Name = "Welcoming Message Channel Id")]
        public ulong WelcomeChannelId { get; set; }
        [Column("welcome_message"), Display(Name = "Welcoming Message Value")]
        public string WelcomeMessage { get; set; }

        // Logging Settings
        [Column("logging_enabled"), Display(Name = "Logging")]
        public bool LogsEnabled { get; set; }
        [Column("logging_channel_id"), RegularExpression("^[0-9]*$"), Display(Name = "Logging Channel Id")]
        public ulong LogsChannelId { get; set; }
        [Column("log_messages"), Display(Name = "Log Messages")]
        public bool LogMessages { get; set; }
        [Column("log_punishments"), Display(Name = "Log Punishments")]
        public bool LogPunishments { get; set; }
        [Column("log_invites"), Display(Name = "Log Invites")]
        public bool LogInvites { get; set; }
        [Column("log_voicestate"), Display(Name = "Log Voice State")]
        public bool LogVoiceState { get; set; }
        [Column("log_channels"), Display(Name = "Log Channels")]
        public bool LogChannels { get; set; }
        [Column("log_users"), Display(Name = "Log Users")]
        public bool LogUsers { get; set; }
        [Column("log_roles"), Display(Name = "Log Roles")]
        public bool LogRoles { get; set; }
        [Column("log_server"), Display(Name = "Log Server")]
        public bool LogServer { get; set; }

        // Leveling Settings
        [Column("leveling_enabled"), Display(Name = "Leveling")]
        public bool LevelingEnabled { get; set; }
        [Column("leveling_multiplier"), Display(Name = "XP Multiplier")]
        public float Multiplier { get; set; }
        [Column("leveling_message"), Display(Name = "Level-Up Message")]
        public string LevelingMessage { get; set; }

        // Other settings
        [Column("lounge_enabled"), Display(Name = "Lounges")]
        public bool LoungesEnabled { get; set; }
        [Column("cat_cmd_enabled"), Display(Name = "Cat Command")]
        public bool CatCmdEnabled { get; set; }
        [Column("dog_cmd_enabled"), Display(Name = "Dog Command")]
        public bool DogCmdEnabled { get; set; }
    }
}