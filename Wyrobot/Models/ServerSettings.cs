using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;

namespace Wyrobot.Core.Models
{
    public class ServerSettings
    {
        public ServerSettings()
        {
            /*YouTube = new YouTubeBroadcast();
            Reddit = new RedditBroadcast();
            Twitch = new TwitchBroadcast();*/
            Moderation = new ModerationSettings();
            Welcome = new WelcomeSettings();
            Logging = new LoggingSettings();
            Leveling = new LevelingSettings();
            Other = new OtherSettings();
            
            /*YouTube.Channels = new List<string>();
            Twitch.Channels = new List<string>();
            Reddit.Subreddits = new List<string>();*/
            Moderation.ModerationRoles = new List<ulong>();
        }

        public LoggingSettings Logging { get; }
        public LevelingSettings Leveling { get; }
        public ModerationSettings Moderation { get; }
        public WelcomeSettings Welcome { get; }
        public OtherSettings Other { get; }
        /*
        public YouTubeBroadcast YouTube { get; }
        public TwitchBroadcast Twitch { get; }
        public RedditBroadcast Reddit { get; }
        */
        private static Task UpdateSetting(ulong guildId, string column, object o)
        {
            using var connection =
                new MySqlConnection(Token.ConnectionString);
            connection.Open();

            var query =
                "update server_info set @column = @object where guild_id = @guildId"
                    .Replace("@column", column);
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.Parameters.AddWithValue("@object", o);
            cmd.ExecuteNonQuery();
                
            connection.Close();
            
            return Task.CompletedTask;
        }
        
/*
        private static void UpdateSocials(ulong guildId, string column, object o)
        {
            using var connection =
                new MySqlConnection(Token.ConnectionString);
            connection.Open();

            var query = "update socials_broadcast set @column = @object where guild_id = @guildId"
                .Replace("@column", column);
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.Parameters.AddWithValue("@object", o);
            cmd.ExecuteNonQuery();
                
            connection.Close();
        }
*/

        /*
        public class YouTubeBroadcast
        {
            public List<string> Channels { get; set; }
            public void UpdateChannels(ulong guildId, List<string> o) => UpdateSocials(guildId, "youtube_channels", o);
            public ulong ChannelId { get; set; }
            public void UpdateChannelId(ulong guildId, ulong o) => UpdateSocials(guildId, "youtube_broadcast_channel_id", o);
            public string Broadcast { get; set; }
            public void UpdateBroadcast(ulong guildId, string o) => UpdateSocials(guildId, "youtube_broadcast_message", o);
        }
        public class TwitchBroadcast
        {
            public List<string> Channels { get; set; }
            public void UpdateChannels(ulong guildId, List<string> o) => UpdateSocials(guildId, "twitch_channels", o);
            public ulong ChannelId { get; set; }
            public void UpdateChannelId(ulong guildId, ulong o) => UpdateSocials(guildId, "twitch_broadcast_channel_id", o);
            public string Broadcast { get; set; }
            public void UpdateBroadcast(ulong guildId, string o) => UpdateSocials(guildId, "twitch_broadcast_message", o);
        }
        public class RedditBroadcast
        {
            public List<string> Subreddits { get; set; }
            public void UpdateChannels(ulong guildId, List<string> o) => UpdateSocials(guildId, "subreddits", o);
            public ulong ChannelId { get; set; }
            public void UpdateChannelId(ulong guildId, ulong o) => UpdateSocials(guildId, "subreddits_broadcast_channel_id", o);
            public string Broadcast { get; set; }
            public void UpdateBroadcast(ulong guildId, string o) => UpdateSocials(guildId, "subreddits_broadcast_message", o);
        }
        */
        
        public class ModerationSettings
        {
            public ulong MuteRoleId { get; set; }
            public async Task UpdateMuteRoleId(ulong guildId, ulong o) => await UpdateSetting(guildId, "mute_role_id", o);
            public List<ulong> ModerationRoles { get; set; }
            public static async Task UpdateModerationRoles(ulong guildId, List<ulong> o)
            {
                var s = o.Aggregate("", (current, v) => current + (o + " "));
                await UpdateSetting(guildId, "moderation_roles", s);
            }
            public string[] BannedWords { get; set; }
            public static async Task UpdateBannedWords(ulong guildId, string[] o)
            {
                var s = o.Aggregate("", (current, v) => current + (v + " "));
                await UpdateSetting(guildId, "banned_words", s);
            }
            public float CapsPercentage { get; set; }
            public static async Task UpdateCapsPercentage(ulong guildId, float o) => await UpdateSetting(guildId, "caps_percentage", o);
            public bool AutoModerationEnabled { get; set; }
            public static async Task UpdateAutoModEnabled(ulong guildId, bool o) => await UpdateSetting(guildId, "automoderation_enabled", o);
            public bool MuteAfter3Warn { get; set; }
            public async Task Update3WarnMute(ulong guildId, bool o) => await UpdateSetting(guildId, "mute_after_3_warns", o);
        }
        
        public class WelcomeSettings
        {
            public bool Enabled { get; set; }
            public static async Task UpdateWelcomeEnabled(ulong guildId, bool o) => await UpdateSetting(guildId, "welcome_message_enabled", o);
            public ulong ChannelId { get; set; }
            public static async Task UpdateChannelId(ulong guildId, ulong o) => await UpdateSetting(guildId, "welcome_channel_id", o);
            public string Message { get; set; }
            public static async Task UpdateMessage(ulong guildId, string o) => await UpdateSetting(guildId, "welcome_message", o);
        }

        public class LoggingSettings
        {
            public bool Enabled { get; set; }
            public static async Task UpdateLoggingEnabled(ulong guildId, bool o) => await UpdateSetting(guildId, "logging_enabled", o);
            public ulong ChannelId { get; set; }
            public static async Task UpdateChannelId(ulong guildId, ulong o) => await UpdateSetting(guildId, "logging_channel_id", o);
            public bool LogMessages { get; set; }
            public async Task UpdateLogMessages(ulong guildId, bool o) => await UpdateSetting(guildId, "log_message", o);
            public bool LogPunishments { get; set; }
            public async Task UpdateLogPunishment(ulong guildId, bool o) => await UpdateSetting(guildId, "log_punishments", o);
            public bool LogInvites { get; set; }
            public async Task UpdateLogInvites(ulong guildId, bool o) => await UpdateSetting(guildId, "log_invites", o);
            public bool LogVoiceState { get; set; }
            public async Task UpdateLogVoiceState(ulong guildId, bool o) => await UpdateSetting(guildId, "log_voicestate", o);
            public bool LogChannels { get; set; }
            public async Task UpdateLogChannels(ulong guildId, bool o) => await UpdateSetting(guildId, "log_channels", o);
            public bool LogUsers { get; set; }
            public async Task UpdateLogUsers(ulong guildId, bool o) => await UpdateSetting(guildId, "log_users", o);
            public bool LogRoles { get; set; }
            public async Task UpdateLogRoles(ulong guildId, bool o) => await UpdateSetting(guildId, "log_roles", o);
            public bool LogServer { get; set; }
            public async Task UpdateLogServer(ulong guildId, bool o) => await UpdateSetting(guildId, "log_server", o);
        }
        
        public class LevelingSettings
        {
            public bool Enabled { get; set; }
            public static async Task UpdateLevelingEnabled(ulong guildId, bool o) => await UpdateSetting(guildId, "leveling_enabled", o);
            public float Multiplier { get; set; }
            public static async Task UpdateMultiplier(ulong guildId, int o) => await UpdateSetting(guildId, "leveling_multiplier", o);
            public string Message { get; set; }
            public static async Task UpdateMessage(ulong guildId, string o) => await UpdateSetting(guildId, "leveling_message", o);
        }

        public class OtherSettings
        {
            public bool LoungesEnabled { get; set; }
            public static async Task UpdateLoungesEnabled(ulong guildId, bool o) => await UpdateSetting(guildId, "lounge_enabled", o);
            public bool CatCmdEnabled { get; set; }
            public async Task UpdateCatCmdEnabled(ulong guildId, bool o) => await UpdateSetting(guildId, "cat_cmd_enabled", o);
            public bool DogCmdEnabled { get; set; }
            public async Task UpdateDogCmdEnabled(ulong guildId, bool o) => await UpdateSetting(guildId, "dog_cmd_enabled", o);
        }
    }
}