using System;
using Newtonsoft.Json;

namespace Wyrobot.Core.Http.Twitch.Models
{
    public class TwitchStream
    {
        [JsonProperty("_id")]
        public ulong Id { get; set; }

        [JsonProperty("game")]
        public string Game { get; set; }

        [JsonProperty("viewers")]
        public int Viewers { get; set; }

        [JsonProperty("video_height")]
        public int VideoHeight { get; set; }

        [JsonProperty("average_fps")]
        public int AverageFps { get; set; }

        [JsonProperty("delay")]
        public int Delay { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("is_playlist")]
        public bool IsPlaylist { get; set; }

        [JsonProperty("preview")]
        public TwitchStreamPreview Preview { get; set; }

        [JsonProperty("channel")]
        public TwitchChannel Channel { get; set; }
    }
}
