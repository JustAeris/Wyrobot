using System;
using System.Collections.Generic;
using MySqlConnector;
using Wyrobot.Core.Models;

namespace Wyrobot.Core.Database
{
    public static class BanDatabase
    {
        /// <summary>
        /// Creates a temp-ban entry in the database
        /// </summary>
        /// <param name="ban">Ban object</param>
        public static void InsertTempBan(Ban ban)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var query = "INSERT INTO temp_bans (user_id, guild_id, issued_at, expires_at, username, reason) VALUES (@userId, @guildId, @issuedAt, @expiresAt, @username,  @reason)";
                
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", ban.UserId);
            cmd.Parameters.AddWithValue("@guildId", ban.GuildId);
            cmd.Parameters.AddWithValue("@issuedAt", ban.IssuedAt);
            cmd.Parameters.AddWithValue("@expiresAt", ban.ExpiresAt);
            cmd.Parameters.AddWithValue("@username", ban.Username);
            cmd.Parameters.AddWithValue("@reason", ban.Reason);

            cmd.ExecuteNonQuery();

            connection.Close();
        }
        
        /// <summary>
        /// Creates a ban entry in the database
        /// </summary>
        /// <param name="ban">Ban object</param>
        public static void InsertBan(Ban ban)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            
            connection.Open();
            
            var query =
                "INSERT INTO bans (user_id, guild_id, issued_at, username, reason) VALUES (@userId, @guildId, @issuedAt, @username,  @reason)";
                
            //create command and assign the query and connection from the constructor
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", ban.UserId);
            cmd.Parameters.AddWithValue("@guildId", ban.GuildId);
            cmd.Parameters.AddWithValue("@issuedAt", ban.IssuedAt);
            cmd.Parameters.AddWithValue("@username", ban.Username);
            cmd.Parameters.AddWithValue("@reason", ban.Reason);

            //Execute command
            cmd.ExecuteNonQuery();

            connection.Close();
        }

        /// <summary>
        /// Gets all the expired bans
        /// </summary>
        /// <returns>List of ExpiredBan object</returns>
        public static List<ExpiredBan> GetExpiredBans()
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var unbanList = new List<ExpiredBan>();
                
            var query = "SELECT * FROM temp_bans WHERE expires_at < @date";
    
            //Create Command
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@date", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            //Create a data reader and Execute the command
            var dataReader = cmd.ExecuteReader();

            //Read the data and store them in the list
            while (dataReader.Read())
            {
                var i = dataReader[1].ToUInt64();
                var ii = dataReader[2].ToUInt64();
                unbanList.Add(new ExpiredBan(i, ii));
            }

            //close Data Reader
            dataReader.Close();

            connection.Close();
            return unbanList;
        }

        /// <summary>
        /// Delete a ban entry from the database
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="guildId">Guild ID</param>
        /// <param name="table">Table's name, either "temp_bans" by default or "bans"</param>
        public static void DeleteBan(ulong userId, ulong guildId, string table = "temp_bans")
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var query = table switch
            {
                "temp_bans" => "DELETE FROM temp_bans WHERE user_id=@userId AND guild_id=@guildId",
                "bans" => "DELETE FROM bans WHERE user_id=@userId AND guild_id=@guildId",
                _ => null
            };
               
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.ExecuteNonQuery();
                    
            connection.Close();
        }

        /// <summary>
        /// Gets all permanent bans from a guild
        /// </summary>
        /// <param name="guildId">Guild's ID</param>
        /// <returns>List of Ban object</returns>
        public static IEnumerable<Ban> GetBans(ulong guildId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var bansList = new List<Ban>();
                
            var query = "SELECT * FROM bans WHERE guild_id=@guildId";
    
            //Create Command
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            //Create a data reader and Execute the command
            var dataReader = cmd.ExecuteReader();

            //Read the data and store them in the list
            while (dataReader.Read())
            {
                var userId = dataReader[1].ToUInt64();
                var uInt64 = dataReader[2].ToUInt64();
                var issuedAt = dataReader.GetDateTime(3);
                var username = dataReader.GetString(4);
                var reason = dataReader.GetString(5);
                    
                bansList.Add(new Ban(userId, uInt64, issuedAt, DateTime.MaxValue, username, reason));
            }

            //close Data Reader
            dataReader.Close();
            connection.Close();
            return bansList;
        }

        /// <summary>
        /// Returns all active temporary bans
        /// </summary>
        /// <param name="guildId">Guild's ID</param>
        /// <returns>List of Ban object</returns>
        public static IEnumerable<Ban> GetActiveBans(ulong guildId)
        {
            var bansList = new List<Ban>();
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var query = "SELECT * FROM temp_bans WHERE guild_id=@guildId";

            //Create Command
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            //Create a data reader and Execute the command
            var dataReader = cmd.ExecuteReader();

            //Read the data and store them in the list
            while (dataReader.Read())
            {
                var userId = dataReader[1].ToUInt64();
                var uInt64 = dataReader[2].ToUInt64();
                var issuedAt = dataReader.GetDateTime(3);
                var expiresAt = dataReader.GetDateTime(4);
                var username = dataReader[5] + "";
                var reason = dataReader[6] + "";
                    
                bansList.Add(new Ban(userId, uInt64, issuedAt, expiresAt, username, reason));
            }

            //close Data Reader
            dataReader.Close();
            connection.Close();
            return bansList;
        }
    }
}