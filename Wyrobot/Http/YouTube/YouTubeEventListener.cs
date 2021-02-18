using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace Wyrobot.Core.Http.YouTube
{
    public class YouTubeEventListener
    {
        private YoutubeClient _client;
        
        public List<ChannelSubscription> Subscriptions { get; private set; }

        public event YouTubeEventHandler OnVideoUploaded;

        private YouTubeEventListener()
        {
            Subscriptions = new List<ChannelSubscription>();
        }

        public YouTubeEventListener(YoutubeClient client)
            : this()
        {
            _client = client;
        }

        public async Task SubscribeChannels(ulong guildId, ulong channelId, string broadcast, List<string> channels)
        {
            if (channels == null)
                throw new ArgumentNullException(nameof(channels));

            foreach (var url in channels)
            {
                var uploads = await _client.Channels.GetUploadsAsync(url);
                Subscriptions.Add(new ChannelSubscription(guildId, channelId, broadcast, url, uploads.Count > 0 ? uploads[0] : null));
            }
        }

        public async Task SubscribeChannels(ulong guildId, ulong channelId, string broadcast, params string[] channels)
        {
            var channelsList = new List<string>();

            for (var index = 0; index < channels.Length; ++index)
                channelsList.Add(channels[index]);

            await SubscribeChannels(guildId, channelId, broadcast, channelsList);
        }

        public void UnsubscribeChannel(ChannelSubscription subscription)
        {
            if (!Subscriptions.Contains(subscription))
                return;

            Subscriptions.Remove(subscription);
        }

        public async Task PollEvents()
        {
            foreach (var subscription in Subscriptions)
            {
                var uploads = await _client.Channels.GetUploadsAsync(subscription.Url);
                var lastVideo = uploads.Count > 0 ? uploads[0] : null;

                if (lastVideo == null)
                    continue;

                if (subscription.LastVideo != lastVideo)
                {
                    if (OnVideoUploaded == null)
                        continue;

                    var channel = await _client.Channels.GetAsync(lastVideo.ChannelId);
                    OnVideoUploaded(this, new YouTubeEventArgs(subscription.GuildId, subscription.ChannelId, subscription.Broadcast, channel, lastVideo ));

                    subscription.LastVideo = lastVideo;
                }
            }
        }

        public class ChannelSubscription
        {
            public ulong GuildId { get; set; }
            public ulong ChannelId { get; set; }
            public string Broadcast { get; set; }

            public string Url { get; set; }
            public Video LastVideo { get; set; }

            public ChannelSubscription(ulong guildId, ulong channelId, string broadcast, string url, Video lastVideo = null)
            {
                GuildId = guildId;
                ChannelId = channelId;
                Broadcast = broadcast;

                Url = url;
                LastVideo = lastVideo;
            }
        }

        public delegate void YouTubeEventHandler(object sender, YouTubeEventArgs e);
    }
}
