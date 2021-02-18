using System.Collections.Generic;
using MySqlConnector;
using Wyrobot.Core.Models;

namespace Wyrobot.Core.Database
{
    public static class AutoRoleDatabase
    {
        public static void InsertAutoRole(ulong guildId, ulong roleId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
            
            var query = "insert into auto_role (guild_id, role_id) VALUES (@guildId, @roleId)";

            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.Parameters.AddWithValue("@roleId", roleId);
            cmd.ExecuteNonQuery();
            
            connection.Close();
        }
        
        public static void DeleteAutoRole(ulong guildId, ulong roleId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
            
            var query = "delete from auto_role where guild_id = @guildId and role_id = @roleId";

            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.Parameters.AddWithValue("@roleId", roleId);
            cmd.ExecuteNonQuery();
            
            connection.Close();
        }
        
        public static IEnumerable<AutoRole> GetAutoRoles(ulong guildId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();

            var list = new List<AutoRole>();
            
            var query = "select * from auto_role where guild_id = @guildId";

            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            var dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                var roleId = dataReader["role_id"].ToUInt64();
                list.Add(new AutoRole(roleId));
            }
            
            connection.Close();

            return list;
        }
        
        public static void DeleteRole(ulong guildId, ulong roleId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
            
            var query = "delete from auto_role where guild_id = @guildId and role_id = @roleId";

            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.Parameters.AddWithValue("@roleId", roleId);
            cmd.ExecuteNonQuery();
            
            connection.Close();
        }
    }
}