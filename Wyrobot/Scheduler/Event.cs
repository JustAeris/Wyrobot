using System;
using System.Linq;
using System.Timers;
using Wyrobot.Core.Database;

namespace Wyrobot.Core.Scheduler
{
    public static class Event
    {
        public static async void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var v in BanDatabase.GetExpiredBans().Where(x => x.ExpiresAt < DateTime.UtcNow))
            {
                var guild = await Program.Discord.GetGuildAsync(v.GuildId);
                try
                {
                    await guild.UnbanMemberAsync(v.UserId, "Ban expired");
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
                BanDatabase.DeleteBan(v.UserId, v.GuildId);
            }

            foreach (var v in MuteDatabase.GetExpiredMutes().Where(x => x.ExpiresAt < DateTime.UtcNow))
            {
                var guild = await Program.Discord.GetGuildAsync(v.GuildId);
                var member = await guild.GetMemberAsync(v.UserId);
                try
                {
                    var muteRole = guild.GetRole(ServerSettingsDatabase.GetServerSettings(v.GuildId).Moderation.MuteRoleId);
                    await member.RevokeRoleAsync(muteRole, "Mute expired");
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
                MuteDatabase.DeleteMute(v.UserId, v.GuildId);
            }

            foreach (var v in WarnDatabase.GetExpiredWarns().Where(x => x.ExpiresAt < DateTime.UtcNow).ToList())
            {
                WarnDatabase.DeleteWarn(v.UserId, v.GuildId, v.ExpiresAt);
            }

            Console.WriteLine($"[{DateTimeOffset.Now.ToString()}] Scheduled task executed");
        }
    }
}