using System.IO;
using Newtonsoft.Json.Linq;

namespace Wyrobot.Core
{
    public static class Token
    {
        static Token()
        {
            var json = JObject.Parse(File.ReadAllText("tokens.json"));

            Discord = json["Discord"]?["Production"]?.ToString();
            DiscordDev = json["Discord"]?["Development"]?.ToString();
            UseProduction = (bool) json["Discord"]?["UseProduction"];
            
            TheCatApi = json["TheCatApi"]?.ToString();
            TheDogApi = json["TheDogApi"]?.ToString();
            
            ConnectionString = json["ConnectionString"]?.ToString();

            TwitchKey = json["TwitchKey"]?.ToString();

            TwitterApiKey = json["Twitter"]?["ApiKey"]?.ToString();
            TwitterApiSecret = json["Twitter"]?["ApiSecret"]?.ToString();
            TwitterBearerToken = json["Twitter"]?["BearerToken"]?.ToString();

            ImgurClientId = json["Imgur"]?["ClientId"]?.ToString();
            ImgurClientSecret = json["Imgur"]?["ClientSecret"]?.ToString();
        }
        
        public static string Discord;
        public static string DiscordDev;
        public static bool UseProduction;

        public static string TheCatApi;
        public static string TheDogApi;
        
        public static string ConnectionString;
        
        public static string TwitchKey;
        
        public static string TwitterApiKey;
        public static string TwitterApiSecret;
        public static string TwitterBearerToken;
        
        public static string ImgurClientId;
        public static string ImgurClientSecret;
    }
}