using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wyrobot.Core;

namespace Wyrobot.Web
{
    public class Program
    {
        public static DiscordRestClient DiscordClient { get; set; }

        public static void Main(string[] args)
        {
            _ = Task.Run(() =>
            {
                DiscordClient = new DiscordRestClient(new DiscordConfiguration
                {
                    Token = Token.Discord,
                    MinimumLogLevel = LogLevel.Debug,
                    TokenType = TokenType.Bot,
                    Intents = DiscordIntents.All,
                    AutoReconnect = true,
                    ReconnectIndefinitely = true
                });
            });
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}