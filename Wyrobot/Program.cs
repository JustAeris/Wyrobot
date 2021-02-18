using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;
using Wyrobot.Core.Commands;
using Wyrobot.Core.Scheduler;

#pragma warning disable 4014
#pragma warning disable 1998

namespace Wyrobot.Core
{
    public static class Program
    {
        public static DiscordClient Discord;
        public static CommandsNextExtension Commands;

        private static async Task Main()
        {
            Task.Run(Setup.SetupAsync);
            MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            Discord = new DiscordClient(new DiscordConfiguration
            {
                Token = Token.UseProduction ? Token.Discord : Token.DiscordDev,
                MinimumLogLevel = LogLevel.Debug,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                AutoReconnect = true,
                ReconnectIndefinitely = true
            });

            Commands = Discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] {";", "w!", "ww!"}, 
                EnableDms = false,
                IgnoreExtraArguments = true
            });

            Commands.RegisterCommands<LevelingCommands>();
            Commands.RegisterCommands<ModerationCommands>();
            Commands.RegisterCommands<MusicCommands>();
            Commands.RegisterCommands<OtherCommands>();
            Commands.RegisterCommands<ServerSettingsCommands>();
            Commands.RegisterCommands<LevelRewardsCommands>();
            Commands.RegisterCommands<AutoRoleCommands>();

            Commands.CommandErrored += async (sender, eventArgs) =>
            {
                Console.WriteLine($"Ze command went kaput: \n{eventArgs.Exception}");

                switch (eventArgs.Exception.Message)
                {
                    case "Specified command was not found.":
                        return;
                    case "No matching subcommands were found, and this group is not executable.":
                        await eventArgs.Context.RespondAsync(
                            $"**{eventArgs.Command.QualifiedName}** is a group-command. Use `;help {eventArgs.Command.QualifiedName}` to get all of the sub commands available.");
                        return;
                    case "Could not find a suitable overload for the command.":
                        await eventArgs.Context.RespondAsync(
                            $"You used invalid arguments. Use `;help {eventArgs.Command.QualifiedName}` to get help.");
                        return;
                    case "One or more pre-execution checks failed.":
                        await eventArgs.Context.RespondAsync(
                            $"You are either under cooldown or you have insufficient permissions. Use `;help {eventArgs.Command.QualifiedName}` to get help.");
                        return;
                }

                var builder = new DiscordEmbedBuilder
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    Title = "A internal exception has occured!",
                    Description = $"`{eventArgs.Exception.Message}`",
                    Color = DiscordColor.DarkRed
                };
                await eventArgs.Context.RespondAsync(embed: builder.Build());
                
                if (!File.Exists("logs.txt"))
                    File.Create("logs.txt");

                await using var f = new FileStream("logs.txt", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                f.Position = f.Length;
                await f.WriteAsync(Encoding.UTF8.GetBytes(Environment.NewLine + DateTimeOffset.UtcNow + eventArgs.Exception + Environment.NewLine));
            };

            Discord.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2), 
                PaginationDeletion = PaginationDeletion.DeleteEmojis
            });
            
            var endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1", // From your server configuration.
                Port = 2333 // From your server configuration
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "2Y9jHmwro!f*e4G%", // From your server configuration.
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };
            
            var lavalink = Discord.UseLavalink();
            
            Discord.UseVoiceNext(new VoiceNextConfiguration
            {
                EnableIncoming = true
            });
            
            await Discord.RegisterEvents(Commands);
            
            await Discord.ConnectAsync(new DiscordActivity("Developing itself", ActivityType.Playing), UserStatus.Online);
            await lavalink.ConnectAsync(lavalinkConfig);

            await Task.Delay(-1);
        }
    }
}


