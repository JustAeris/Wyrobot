using MySqlConnector;
using Wyrobot.Core.Models;

namespace Wyrobot.Core.Database
{
    public static class ReactionMessageDatabase
    {
        public static ReactionMessage GetReactionMessage(ulong guildId, ulong messageId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var reactionMessage = new ReactionMessage();
            var query = "select * from reaction_messages where guild_id=@guildId and message_id=@messageId";
                
                
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.Parameters.AddWithValue("@messageId", messageId);
            var dataReader = cmd.ExecuteReader();
    
            while (dataReader.Read())
            {
                reactionMessage.GuildId = dataReader[1].ToUInt64();
                reactionMessage.ChannelId = dataReader[2].ToUInt64();
                reactionMessage.Id = dataReader[3].ToUInt64();
                reactionMessage.Action = (dataReader[4] + "") switch
                {
                    "0" => Action.Grant,
                    "1" => Action.Revoke,
                    "2" => Action.Suggestion,
                    _ => reactionMessage.Action
                };
                reactionMessage.Role = dataReader[5].ToUInt64();
            }
    
            dataReader.Close();
            connection.Close();
    
            return reactionMessage;
        }

        public static void InsertReactionRoleMessage(ReactionMessage reactionMessage)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            connection.Open();
                
            var query = "insert into reaction_messages (guild_id, channel_id, message_id, action, role) VALUES (@guildId, @channelId, @messageId, @action, @role)";
                
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", reactionMessage.GuildId);
            cmd.Parameters.AddWithValue("@channelId", reactionMessage.ChannelId);
            cmd.Parameters.AddWithValue("@messageId", reactionMessage.Id);
            cmd.Parameters.AddWithValue("@action", reactionMessage.Action);
            cmd.Parameters.AddWithValue("@role", reactionMessage.Role);

            cmd.ExecuteNonQuery();   
                
            connection.Close();
        }

        public static void DeleteReactionRoleMessage(ulong guildId, ulong messageId)
        {
            using var connection = new MySqlConnection(Token.ConnectionString);
            var query = "DELETE from reaction_messages where guild_id=@guildId and message_id=@messageId";

            connection.Open();
                
            var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@guildId", guildId);
            cmd.Parameters.AddWithValue("@messageId", messageId);

            cmd.ExecuteNonQuery();

            connection.Close();
        }
    }
}