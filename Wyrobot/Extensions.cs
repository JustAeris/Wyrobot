using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Wyrobot.Core
{
    public static class Extensions
    {
        public static ulong ToUInt64(this Object o)
        {
            try
            {
                return Convert.ToUInt64(o);
            }
            catch
            {
                return 0;
            }
        }
        
        public static int ToInt32(this Object o)
        {
            return Convert.ToInt32(o);
        }
        
        public static DateTime ToDateTime(this String str)
        {
            return Convert.ToDateTime(str);
        }

        public static bool ToBoolean(this String str)
        {
            switch (str)
            {
                case "true":
                case "1":
                    return true;
                case "false":
                case "0":
                    return false;
                default:
                    return false;
            }
        }

        public static string Capitalize(this String str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }
        
        public enum VoiceStateAction
        {
            Unknown,
            Joined,
            Left,
            Moved
        }
        public static VoiceStateAction GetVoiceStateAction(this VoiceStateUpdateEventArgs arg)
        {
            if (arg.Before.Channel == null && arg.After.Channel != null)
                return VoiceStateAction.Joined;
                
            if (arg.Before.Channel != null && arg.After.Channel == null)
                return VoiceStateAction.Left;

            if (arg.Before.Channel != null && arg.After.Channel != null)
                return VoiceStateAction.Moved;
                
            return VoiceStateAction.Unknown;
        }

        public static string Tag(this DiscordMember member)
        {
            return $"{member.Username}#{member.Discriminator}";
        }
        
        public static string Tag(this DiscordUser member)
        {
            return $"{member.Username}#{member.Discriminator}";
        }

        public static long GetPing(this String s)
        {
            var pingSender = new Ping ();
            var options = new PingOptions {DontFragment = true};

            // ReSharper disable once StringLiteralTypo
            var data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var buffer = Encoding.ASCII.GetBytes (data);
            var timeout = 2000;
            var reply = pingSender.Send (s, timeout, buffer, options);

            if (reply == null || reply.Status != IPStatus.Success) return 0;
            
            return reply.RoundtripTime;
        }

        public static string GetHeartColor(this long l)
        {
            if (l < 100)
            {
                return "green_";
            }
            
            if (l < 500)
            {
                return "yellow_";
            }

            return l < 1000 ? "" : "broken";
        }

        public static string GetHeartColor(this int i)
        {
            return GetHeartColor(Convert.ToInt64(i));
        }

        public static bool CanPunish(this DiscordMember moderator, DiscordMember target)
        {
            var highestMemberRole = moderator.Roles.Aggregate((a, b) => a.Position < b.Position ? a : b);
            var highestTargetRole = target.Roles.Aggregate((a, b) => a.Position < b.Position ? a : b);

            return  highestTargetRole.Position < highestMemberRole.Position;
        }
    }
}