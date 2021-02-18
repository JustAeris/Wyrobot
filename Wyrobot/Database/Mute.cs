using System;
using System.Collections.Generic;
using MySqlConnector;
using Wyrobot.Core.Models;

namespace Wyrobot.Core.Database
{
    public static class MuteDatabase
    {
        /// <summary>
        /// Creates a temp-mute entry in the database
        /// </summary>
        /// <param name="mute">Mute object</param>
        public static void InsertTempMute(Mute mute)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var query =
                "INSERT INTO temp_mutes (user_id, guild_id, issued_at, expires_at, username, reason) VALUES (@userId, @guildId, @issuedAt, @expiresAt, @username,  @reason)";

            //create command and assign the query and connection from the constructor
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", mute.UserId);
            cmd.Parameters.AddWithValue("@guildId", mute.GuildId);
            cmd.Parameters.AddWithValue("@issuedAt", mute.IssuedAt);
            cmd.Parameters.AddWithValue("@expiresAt", mute.ExpiresAt);
            cmd.Parameters.AddWithValue("@username", mute.Username);
            cmd.Parameters.AddWithValue("@reason", mute.Reason);

            //Execute command
            cmd.ExecuteNonQuery();
                
            connection.Close();
        }

        /// <summary>
        /// Creates a mute entry in the database
        /// </summary>
        /// <param name="mute">Mute object</param>
        public static void InsertMute(Mute mute)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var query = "INSERT INTO mutes (user_id, guild_id, issued_at, username, reason) VALUES (@userId, @guildId, @issuedAt, @username,  @reason)";
                
            //create command and assign the query and connection from the constructor
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", mute.UserId);
            cmd.Parameters.AddWithValue("@guildId", mute.GuildId);
            cmd.Parameters.AddWithValue("@issuedAt", mute.IssuedAt);
            cmd.Parameters.AddWithValue("@username", mute.Username);
            cmd.Parameters.AddWithValue("@reason", mute.Reason);

            //Execute command
            cmd.ExecuteNonQuery();
                
            connection.Close();
        }

        /// <summary>
        /// Gets all of the expired mutes
        /// </summary>
        /// <returns>List of ExpiredMute object</returns>
        public static IEnumerable<ExpiredMute> GetExpiredMutes()
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var unbanList = new List<ExpiredMute>();

            var query = "SELECT * FROM temp_mutes WHERE expires_at < @date";

            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@date", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            var dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                var userId = dataReader[1].ToUInt64();
                var guildId = dataReader[2].ToUInt64();
                unbanList.Add(new ExpiredMute(userId, guildId));
            }

            dataReader.Close();
                
            connection.Close();

            return unbanList;
        }

        /// <summary>
        /// Delete a Mute entry from the database
        /// </summary>
        /// <param name="userId">User's ID</param>
        /// <param name="guildId">Guild's ID</param>
        /// <param name="table">Table's name, either "temp_bans" by default or "bans"</param>
        public static void DeleteMute(ulong userId, ulong guildId, string table = "temp_mutes")
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var query = table switch
            {
                "temp_mutes" => "DELETE FROM temp_mutes WHERE user_id = @userId AND guild_id = @guildId",
                "mutes" => "DELETE FROM mutes WHERE user_id = @userId AND guild_id = @guildId",
                _ => null
            };
    
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.ExecuteNonQuery();
                
            connection.Close();
        }

        /// <summary>
        /// Gets permanent mutes
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns>List of Mute objects</returns>
        public static IEnumerable<Mute> GetMutes(ulong guildId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var bansList = new List<Mute>();
                
            var query = "SELECT * FROM mutes WHERE guild_id=@guildId";
    
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            var dataReader = cmd.ExecuteReader();
    
            while (dataReader.Read())
            {
                var userId = dataReader[1].ToUInt64();
                var uInt64 = dataReader[2].ToUInt64();
                var issuedAt = dataReader.GetDateTime(3);
                var username = dataReader[4] + "";
                var reason = dataReader[5] + "";
                    
                bansList.Add(new Mute(userId, uInt64, issuedAt, DateTime.MaxValue, username, reason));
            }
    
            dataReader.Close();
                
            connection.Close();

            return bansList;
        }

        public static IEnumerable<Mute> GetActiveMutes(ulong guildId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var bansList = new List<Mute>();

            var query = "SELECT * FROM temp_mutes WHERE guild_id=@guildId";

            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            var dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                var userId = dataReader[1].ToUInt64();
                var uInt64 = dataReader[2].ToUInt64();
                var issuedAt = dataReader.GetDateTime(3);
                var expiresAt = dataReader.GetDateTime(4);
                var username = dataReader[5] + "";
                var reason = dataReader[6] + "";
                    
                bansList.Add(new Mute(userId, uInt64, issuedAt, expiresAt, username, reason));
            }

            dataReader.Close();
                
            connection.Close();

            return bansList;
        }
    }
}