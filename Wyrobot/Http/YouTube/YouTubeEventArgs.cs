using System;
using YoutubeExplode.Channels;
using YoutubeExplode.Videos;

namespace Wyrobot.Core.Http.YouTube
{
    public class YouTubeEventArgs : EventArgs
    {
        public ulong DiscordGuildId { get; set; }
        public ulong DiscordChannelId { get; set; }

        public string Broadcast { get; set; }

        public Channel Channel { get; set; }
        public Video Video { get; set; }

        public YouTubeEventArgs(ulong discordGuildId, ulong discordChannelId, string broadcast, Channel channel, Video video)
        {
            DiscordGuildId = discordGuildId;
            DiscordChannelId = discordChannelId;

            Broadcast = broadcast;

            Channel = channel;
            Video = video;
        }
    }
}
