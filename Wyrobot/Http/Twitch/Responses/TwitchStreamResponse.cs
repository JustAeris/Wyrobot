using Newtonsoft.Json;
using Wyrobot.Core.Http.Twitch.Models;

namespace Wyrobot.Core.Http.Twitch.Responses
{
    public class TwitchStreamResponse
    {
        [JsonProperty("stream")]
        public TwitchStream Stream { get; set; }
    }
}
