using Newtonsoft.Json;

namespace Wyrobot.Core.Http.Twitch.Models
{
    public class TwitchStreamPreview
    {
        [JsonProperty("small")]
        public string Small { get; set; }

        [JsonProperty("medium")]
        public string Medium { get; set; }

        [JsonProperty("large")]
        public string Large { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }
    }
}
