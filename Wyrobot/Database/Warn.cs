using System;
using System.Collections.Generic;
using MySqlConnector;
using Wyrobot.Core.Models;

namespace Wyrobot.Core.Database
{
    public static class WarnDatabase
    {
        public static void InsertWarn(Warn warn)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var query = "INSERT INTO warns (user_id, guild_id, issued_at, expires_at, username, reason) VALUES (@userId, @guildId, @issuedAt, @expiresAt, @username,  @reason)";
                
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", warn.UserId);
            cmd.Parameters.AddWithValue("@guildId", warn.GuildId);
            cmd.Parameters.AddWithValue("@issuedAt", warn.IssuedAt);
            cmd.Parameters.AddWithValue("@expiresAt", warn.ExpiresAt);
            cmd.Parameters.AddWithValue("@username", warn.Username);
            cmd.Parameters.AddWithValue("@reason", warn.Reason);

            cmd.ExecuteNonQuery();

            connection.Close();
        }
        
        public static IEnumerable<ExpiredWarn> GetExpiredWarns()
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var unbanList = new List<ExpiredWarn>();
                
            var query = "SELECT * FROM warns where expires_at < @date";
    
            //Create Command
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@date", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss").ToDateTime());
            //Create a data reader and Execute the command
            var dataReader = cmd.ExecuteReader();

            //Read the data and store them in the list
            while (dataReader.Read())
            {
                var userId = dataReader[1].ToUInt64();
                var uInt64 = dataReader[2].ToUInt64();
                var issuedAt = dataReader.GetDateTime(3).ToString("yyyy-MM-dd HH:mm:ss").ToDateTime();
                var expiresAt = dataReader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss").ToDateTime();
                var username = dataReader[5] + "";
                var reason = dataReader[6] + "";
                    
                if (expiresAt < DateTime.UtcNow)
                    unbanList.Add(new ExpiredWarn(userId, uInt64, issuedAt, expiresAt, username, reason));

                Console.WriteLine(dataReader[3]);
            }

            //close Data Reader
            dataReader.Close();

            connection.Close();
            return unbanList;
        }

        public static void DeleteWarn(ulong userId, ulong guildId, DateTime expiresAt)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
            
            var query = "DELETE FROM warns WHERE user_id=@userId AND guild_id=@guildId and expires_at=@expiresAt";
           
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.Parameters.AddWithValue("@expiresAt", expiresAt);
            cmd.ExecuteNonQuery();
                
            connection.Close();
        }

        public static List<Warn> GetUserWarn(ulong userId, ulong guildId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();

            var list = new List<Warn>();

            var query = "select * from warns where user_id=@userId and guild_id=@guildId";
                
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@guildId", guildId);

            var dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                var userUInt = dataReader[1].ToUInt64();
                var guildUInt = dataReader[2].ToUInt64();
                var issuedAt = dataReader.GetDateTime(3);
                var expiresAt = dataReader.GetDateTime(4);
                var username = dataReader[5] + "";
                var reason = dataReader[6] + "";
                list.Add(new Warn(userUInt, guildUInt, issuedAt, expiresAt, username, reason));
            }
                
            connection.Close();

            return list;
        }
    }
}