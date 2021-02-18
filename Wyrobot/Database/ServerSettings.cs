using System;
using MySqlConnector;
using Wyrobot.Core.Models;

namespace Wyrobot.Core.Database
{
    public static class ServerSettingsDatabase
    {
        /// <summary>
        /// Gets all the infos and settings about a server
        /// </summary>
        /// <param name="guildId">Guild's ID</param>
        /// <returns>ServerInfo object</returns>
        public static ServerSettings GetServerSettings(ulong guildId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var serverInfos = new ServerSettings();

            var query = "SELECT * FROM server_info WHERE guild_id = @guildId";

            //Create Command
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            //Create a data reader and Execute the command
            var dataReader = cmd.ExecuteReader();

            //Read the data and store them in the list
            while (dataReader.Read())
            {
                if (dataReader["guild_id"] != DBNull.Value) dataReader["guild_id"].ToUInt64();

                if (dataReader["welcome_message_enabled"] != DBNull.Value) 
                    serverInfos.Welcome.Enabled = dataReader.GetBoolean("welcome_message_enabled");
                if (dataReader["welcome_channel_id"] != DBNull.Value) 
                    serverInfos.Welcome.ChannelId = dataReader["welcome_channel_id"].ToUInt64();
                if (dataReader["welcome_message"] != DBNull.Value) 
                    serverInfos.Welcome.Message = dataReader.GetString("welcome_message");
                if (dataReader["leveling_enabled"] != DBNull.Value) 
                    serverInfos.Leveling.Enabled = dataReader.GetBoolean("leveling_enabled");
                if (dataReader["leveling_multiplier"] != DBNull.Value) 
                    serverInfos.Leveling.Multiplier = dataReader.GetFloat("leveling_multiplier");
                if (dataReader["leveling_message"] != DBNull.Value) 
                    serverInfos.Leveling.Message = dataReader.GetString("leveling_message");

                if (dataReader["logging_enabled"] != DBNull.Value) 
                    serverInfos.Logging.Enabled = dataReader.GetBoolean("logging_enabled");
                if (dataReader["logging_channel_id"] != DBNull.Value) 
                    serverInfos.Logging.ChannelId = dataReader["logging_channel_id"].ToUInt64();
                if (dataReader["log_messages"] != DBNull.Value) 
                    serverInfos.Logging.LogMessages = dataReader.GetBoolean("log_messages");
                if (dataReader["log_punishments"] != DBNull.Value) 
                    serverInfos.Logging.LogPunishments = dataReader.GetBoolean("log_punishments");
                if (dataReader["log_invites"] != DBNull.Value) 
                    serverInfos.Logging.LogInvites = dataReader.GetBoolean("log_invites");
                if (dataReader["log_voicestate"] != DBNull.Value) 
                    serverInfos.Logging.LogVoiceState = dataReader.GetBoolean("log_voicestate");
                if (dataReader["log_channels"] != DBNull.Value) 
                    serverInfos.Logging.LogChannels = dataReader.GetBoolean("log_channels");
                if (dataReader["log_users"] != DBNull.Value) 
                    serverInfos.Logging.LogUsers = dataReader.GetBoolean("log_users");
                if (dataReader["log_roles"] != DBNull.Value) 
                    serverInfos.Logging.LogRoles = dataReader.GetBoolean("log_roles");
                if (dataReader["log_server"] != DBNull.Value) 
                    serverInfos.Logging.LogServer = dataReader.GetBoolean("log_server");
                
                        
                if (dataReader["moderation_roles"] != DBNull.Value)
                {
                    var s = dataReader.GetString("moderation_roles").Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var v in s)
                        serverInfos.Moderation.ModerationRoles.Add(v.ToUInt64());
                }
                if (dataReader["banned_words"] != DBNull.Value) 
                    serverInfos.Moderation.BannedWords = dataReader.GetString("banned_words").Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (dataReader["caps_percentage"] != DBNull.Value) 
                    serverInfos.Moderation.CapsPercentage = dataReader.GetFloat("caps_percentage");
                if (dataReader["mute_role_id"] != DBNull.Value) 
                    serverInfos.Moderation.MuteRoleId = dataReader["mute_role_id"].ToUInt64();
                if (dataReader["automoderation_enabled"] != DBNull.Value) 
                    serverInfos.Moderation.AutoModerationEnabled = dataReader.GetBoolean("automoderation_enabled");
                if (dataReader["mute_after_3_warns"] != DBNull.Value) 
                    serverInfos.Moderation.MuteAfter3Warn = dataReader.GetBoolean("mute_after_3_warns");

                if (dataReader["lounge_enabled"] != DBNull.Value)
                    serverInfos.Other.LoungesEnabled = dataReader.GetBoolean("lounge_enabled");
                if (dataReader["cat_cmd_enabled"] != DBNull.Value)
                    serverInfos.Other.CatCmdEnabled = dataReader.GetBoolean("cat_cmd_enabled");
                if (dataReader["dog_cmd_enabled"] != DBNull.Value)
                    serverInfos.Other.DogCmdEnabled = dataReader.GetBoolean("dog_cmd_enabled");
            }

            dataReader.Close();
            /*    
            var query2 = "SELECT * FROM socials_broadcast WHERE guild_id=@guildId";

            //Create Command
            var cmd2 = new MySqlCommand(query2, connection);
            cmd2.Parameters.AddWithValue("@guildId", guildId);
            //Create a data reader and Execute the command
            var dataReader2 = cmd2.ExecuteReader();

            while (dataReader2.Read())
            {
                serverInfos.YouTube.Channels = dataReader2.GetString(2)
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                serverInfos.YouTube.ChannelId = dataReader2.GetUInt64(3);
                serverInfos.YouTube.Broadcast = dataReader2.GetString(4);

                serverInfos.Twitch.Channels = dataReader2.GetString(5)
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                serverInfos.Twitch.ChannelId = dataReader2.GetUInt64(6);
                serverInfos.Twitch.Broadcast = dataReader2.GetString(7);

                serverInfos.Reddit.Subreddits = dataReader2.GetString(8)
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                serverInfos.Reddit.ChannelId = dataReader2.GetUInt64(9);
                serverInfos.Reddit.Broadcast = dataReader2.GetString(10);
            }
                
            dataReader2.Close();
            */    
            connection.Close();

            return serverInfos;
        }

        /// <summary>
        /// Delete server settings
        /// </summary>
        /// <param name="guildId">Guild's ID</param>
        public static void DeleteServerSettings(ulong guildId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var query = "DELETE FROM server_info WHERE guild_id=@guildId";
                
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.ExecuteNonQuery();
                
            connection.Close();
        }
        
        public static void GenerateServerSettings(ulong guildId, ulong muteId, string guildName)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var query = "insert into server_info (guild_id, mute_role_id, guild_name) values (@guildId, @muteId, @guildName)";
                
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.Parameters.AddWithValue("@guildName", guildName);
            cmd.Parameters.AddWithValue("@muteId", muteId);
            cmd.ExecuteNonQuery();
                
            connection.Close();
        }
    }
}