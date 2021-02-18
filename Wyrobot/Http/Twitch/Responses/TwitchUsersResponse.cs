using System.Collections.Generic;
using Newtonsoft.Json;
using Wyrobot.Core.Http.Twitch.Models;

namespace Wyrobot.Core.Http.Twitch.Responses
{
    public class TwitchUsersResponse
    {
        [JsonProperty("_total")]
        public int Total { get; set; }

        [JsonProperty("users")]
        public List<TwitchUser> Users { get; set; }
    }
}
