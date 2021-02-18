using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Wyrobot.Core.Http.Twitch.Responses;

namespace Wyrobot.Core.Http.Twitch
{
    public class TwitchClient : IDisposable
    {
        public readonly Uri BaseUrl = new Uri("https://api.twitch.tv/");

        public string ClientId { get; private set; }

        public TwitchClient(string clientId)
        {
            ClientId = clientId;
        }

        public async Task<TwitchStreamResponse> GetStreamAsync(string channelId)
        {
            var output = await SendRequestAsync($"/kraken/streams/{channelId}");
            return JsonConvert.DeserializeObject<TwitchStreamResponse>(output);
        }

        public async Task<TwitchStreamResponse> GetStreamAsync(ulong channelId)
        {
            return await GetStreamAsync(channelId.ToString());
        }

        public async Task<TwitchUsersResponse> GetUsersAsync(params string[] names)
        {
            var builder = new StringBuilder();

            for (var index = 0; index < names.Length; ++index)
            {
                builder.Append(names[index]);
                builder.Append(',');
            }

            var output = await SendRequestAsync($"/kraken/users?login={builder.ToString().TrimEnd(',')}");
            return JsonConvert.DeserializeObject<TwitchUsersResponse>(output);
        }

        public async Task<string> SendRequestAsync(string endpoint)
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(BaseUrl, endpoint));

            request.Headers["Accept"] = "application/vnd.twitchtv.v5+json";
            request.Headers["Client-ID"] = ClientId;
            
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using var response = (HttpWebResponse)await request.GetResponseAsync();
            await using var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream!);
            return await reader.ReadToEndAsync();
        }

        public void Dispose()
        {
        }
    }
}
