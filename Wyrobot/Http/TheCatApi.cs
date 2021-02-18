using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Wyrobot.Core.Http
{
    public static class TheCatApi
    {
        public static async Task<string> Get()
        {
            var request = (HttpWebRequest)WebRequest.Create("https://api.thecatapi.com/v1/images/search");

            request.Headers["X-Api-Key"] = Token.TheCatApi;
            
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            string s;

            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            await using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream!))
            {
               s = await reader.ReadToEndAsync();
            }

            var value = (string)JArray.Parse(s).Children()["url"].First();

            return value;
        }
    }
}