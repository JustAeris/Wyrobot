using System.Collections.Generic;
using MySqlConnector;
using Wyrobot.Core.Models;

namespace Wyrobot.Core.Database
{
    public static class LevelRewardsDatabase
    {
        public static void InsertLevelReward(ulong guildId, int requiredLevel, ulong roleId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
            
            var query = "insert into leveling_rewards (guild_id, level_required, role_reward) VALUES (@guildId, @requiredLevel, @roleId)";

            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.Parameters.AddWithValue("@requiredLevel", requiredLevel);
            cmd.Parameters.AddWithValue("@roleId", roleId);
            cmd.ExecuteNonQuery();
            
            connection.Close();
        }
        
        public static void DeleteLevelReward(ulong guildId, int requiredLevel, ulong roleId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
            
            var query = "delete from leveling_rewards where guild_id = @guildId and level_required = @requiredLevel and role_reward = @roleId";

            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.Parameters.AddWithValue("@requiredLevel", requiredLevel);
            cmd.Parameters.AddWithValue("@roleId", roleId);
            cmd.ExecuteNonQuery();
            
            connection.Close();
        }
        
        public static IEnumerable<LevelReward> GetLevelReward(ulong guildId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();

            var list = new List<LevelReward>();
            
            var query = "select * from leveling_rewards where guild_id = @guildId";

            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            var dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                var requiredLevel = dataReader.GetInt32("level_required");
                var roleId = dataReader["role_reward"].ToUInt64();
                list.Add(new LevelReward(requiredLevel, roleId));
            }
            
            connection.Close();

            return list;
        }

        public static void DeleteRole(ulong guildId, ulong roleId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
            
            var query = "delete from leveling_rewards where guild_id = @guildId and role_reward = @roleId";

            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.Parameters.AddWithValue("@roleId", roleId);
            cmd.ExecuteNonQuery();
            
            connection.Close();
        }
    }
}