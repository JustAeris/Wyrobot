using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using MySqlConnector;
using Wyrobot.Core.Models;

namespace Wyrobot.Core.Database
{
    public static class UserLevelDatabase
    {
        /// <summary>
        /// Gets the level infos about a user in a specific guild
        /// </summary>
        /// <param name="guildId">Guild's ID</param>
        /// <param name="userId">User's ID</param>
        /// <returns></returns>
        public static UserLevel GetUserLevelInfo(ulong guildId, ulong userId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var userLevelInfo = new UserLevel();
                
            var query = "SELECT * FROM user_levels where guild_id=@guildId and user_id=@userId";
    
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.Parameters.AddWithValue("@userId", userId);

            var dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                ulong guildInt = 0, userInt = 0;
                int xp = 0, level = 0, xpToNextLevel = 0;
                    
                if (dataReader[1] != DBNull.Value)
                    userInt = dataReader[1].ToUInt64();
                if (dataReader[2] != DBNull.Value)
                    guildInt = dataReader[2].ToUInt64();
                if (dataReader[3] != DBNull.Value)
                    xp = dataReader[3].ToInt32();
                if (dataReader[4] != DBNull.Value)
                    level = dataReader[4].ToInt32();
                if (dataReader[5] != DBNull.Value)
                    xpToNextLevel = dataReader[5].ToInt32();

                userLevelInfo.UserId = userInt;
                userLevelInfo.GuildId = guildInt;
                userLevelInfo.Xp = xp;
                userLevelInfo.Level = level;
                userLevelInfo.XpToNextLevel = xpToNextLevel;
            }
            dataReader.Close();
            connection.Close();

            return userLevelInfo;
        }

        public static IEnumerable<UserLevel> GetGuildLevelInfo(ulong guildId, bool order = true)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var guildLevelInfo = new List<UserLevel>();
                            
            var query = "SELECT * FROM user_levels where guild_id=@guildId order by current_level desc, current_xp DESC";
                
            if (!order)
            {
                query = "SELECT * FROM user_levels where guild_id=@guildId";
            }
    
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);

            var dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                ulong userUInt = 0;
                    
                if (dataReader[1] != DBNull.Value)
                    userUInt = dataReader[1].ToUInt64();
                    
                guildLevelInfo.Add(GetUserLevelInfo(guildId, userUInt));
            }
            dataReader.Close();
            connection.Close();
            return guildLevelInfo;
        }

        public static void SetUserLevelInfoLevel(UserLevel userLevel)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();

            var query = GetUserLevelInfo(userLevel.GuildId, userLevel.UserId).UserId != 0
                ? "UPDATE user_levels set current_level=@level, current_xp=0, xp_to_next_level=@xpToNextLevel where user_id=@userId and guild_id=@guildId"
                : "INSERT into user_levels (user_id, guild_id, current_level) values (@userId, @guildId, @level)";
                
                
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", userLevel.GuildId);
            cmd.Parameters.AddWithValue("@userId", userLevel.UserId);
            cmd.Parameters.AddWithValue("@level", userLevel.Level);
            cmd.Parameters.AddWithValue("@xpToNextLevel", userLevel.Level * 100 + 75);
    
            cmd.ExecuteNonQuery();
                
            connection.Close();
        }
        
        public static async void SetUserLevelInfoXp(UserLevel userLevel, DiscordChannel discordChannel)
        {
            var settings = ServerSettingsDatabase.GetServerSettings(userLevel.GuildId);
            
            await using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();

            var oldUserLevel = GetUserLevelInfo(userLevel.GuildId, userLevel.UserId);
            var currentXp = oldUserLevel.Xp;
            var currentLevel = oldUserLevel.Level;
    
            var query = oldUserLevel.UserId != 0
                ? "UPDATE user_levels set current_xp=@currentXp, xp_to_next_level=@xpToNextLevel where user_id=@userId and guild_id=@guildId"
                : "INSERT into user_levels (user_id, guild_id, current_xp) values (@userId, @guildId, @currentXp)";
                
                
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", userLevel.GuildId);
            cmd.Parameters.AddWithValue("@userId", userLevel.UserId);
            cmd.Parameters.AddWithValue("@currentXp", userLevel.Xp + currentXp);
            cmd.Parameters.AddWithValue("@xpToNextLevel", currentLevel * 100 + 75);
    
            cmd.ExecuteNonQuery();
    
            oldUserLevel = GetUserLevelInfo(userLevel.GuildId, userLevel.UserId);

            currentXp = oldUserLevel.Xp;
            currentLevel = oldUserLevel.Level;
            var xpToNextLevel = oldUserLevel.XpToNextLevel;
    
            if (currentXp > xpToNextLevel)
            {
                var guild = await Program.Discord.GetGuildAsync(userLevel.GuildId);
                var user = await guild.GetMemberAsync(userLevel.UserId);

                userLevel.Level += currentLevel + 1;
                SetUserLevelInfoLevel(userLevel);
                if (discordChannel == null) return;
                await discordChannel.SendMessageAsync(
                    $"{settings.Leveling.Message.Replace("@level", (currentLevel + 1).ToString()).Replace("@user", user.Mention)}");

                var list = LevelRewardsDatabase.GetLevelReward(userLevel.GuildId).ToList();

                foreach (var role in from vReward in list where vReward.RequiredLevel == userLevel.Level select guild.GetRole(vReward.RoleId))
                {
                    await user.GrantRoleAsync(role);
                }
            }
            await connection.CloseAsync();
        }
    }
}