using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using MySqlConnector;
using PokeAPI;
using Wyrobot.Core.Database;
using Wyrobot.Core.Http;
using Wyrobot.Core.Models;
using Action = Wyrobot.Core.Models.Action;
#pragma warning disable 1998

// ReSharper disable UnusedMember.Global

namespace Wyrobot.Core.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class OtherCommands : BaseCommandModule
    {
        [Command("suggestion"), Aliases("idea"), Description("Make a suggestion")]
        public async Task Suggestion(CommandContext ctx, [RemainingText, Description("Suggestion to make")]string suggestion)
        {
            if (suggestion == null)
            {
                await ctx.Message.DeleteAsync();
                var autoDeleteMessage = await ctx.RespondAsync($"No suggestion has been given! {ctx.User.Mention}");
                await Task.Delay(5000);
                await autoDeleteMessage.DeleteAsync();
                return;
            }

            await ctx.Message.DeleteAsync("Auto delete on command invocation.");

            var embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.AddField($"**{ctx.User.Username}#{ctx.User.Discriminator}**'s suggestion:", suggestion);
            embedBuilder.Color = DiscordColor.Grayple;
            DiscordEmbed embed = embedBuilder;
            var message = await ctx.RespondAsync(null, false, embed);

            await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));

            var reactionMessage = new ReactionMessage
            {
                GuildId = ctx.Guild.Id, Id = message.Id, Action = Action.Suggestion, ChannelId = message.ChannelId
            };

            ReactionMessageDatabase.InsertReactionRoleMessage(reactionMessage);
        }

        [Command("reactionrole"), Aliases("rr"), Description("Creates a message with a reaction which can be used to grant or revoke a role."), RequirePermissions(Permissions.ManageRoles)]
        public async Task ReactionRole(CommandContext ctx, [Description("Role to grant or revoke.")] DiscordRole role, [Description("Emoji use for the event.")] DiscordEmoji emoji, [Description("Use \"grant\" or \"revoke\". This is what will happen on react. on de-react, the opposite action will happen.")] string action, [RemainingText, Description("Text to put in the message")] string text)
        {
            if (action != "grant" && action != "revoke" || role == null || emoji == null)
            {
                await ctx.RespondAsync("Invalid command usage! :x: \nPlease use `;help rr`.");
                return;
            }
            
            var embedBuilder = new DiscordEmbedBuilder
                {Color = role.Color, Description = text};

            var message = await ctx.RespondAsync(null, false, embedBuilder.Build());

            await message.CreateReactionAsync(emoji);

            await ctx.Message.DeleteAsync();

            var reactionMessage = new ReactionMessage
            {
                GuildId = ctx.Guild.Id,
                ChannelId = message.ChannelId,
                Id = message.Id, 
                Action = action switch
                {
                    "grant" => Action.Grant,
                    "revoke" => Action.Revoke,
                    "suggestion" => Action.Suggestion,
                    _ => Action.Grant
                },
                Role = role.Id
            };

            ReactionMessageDatabase.InsertReactionRoleMessage(reactionMessage);
        }

        [Command("rolelist")]
        public async Task RoleList(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            
            var builder = new DiscordEmbedBuilder
                {
                    Title = "Role list :",
                    Color = DiscordColor.Grayple
                }
                .WithFooter($"ID : {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow);

            var i = 1;
            var s = "";
            var list = ctx.Guild.Roles.OrderBy(x => x.Value.Position).Reverse();
            foreach (var (_, v) in list)
            {
                s += $"{i}: {v.Mention}\n";
                i++;
            }

            var pages = interactivity.GeneratePagesInEmbed(s, SplitType.Line, builder);

            await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages);
        }

        [Command("whois")]
        public async Task WhoIs(CommandContext ctx, DiscordMember user)
        {
            await ctx.TriggerTypingAsync();

            var permissions = user.Roles.Select(x => x.Permissions).ToList();

            var s = "";
            foreach (var v in permissions.Where(v => !s.Contains(v.ToPermissionString())))
            {
                if (v != Permissions.Administrator) continue;
                if (Permissions.ViewAuditLog != v) continue;
                if (Permissions.ManageGuild != v) continue;
                if (Permissions.ManageRoles != v) continue;
                if (Permissions.ManageChannels != v) continue;
                if (Permissions.KickMembers != v) continue;
                if (Permissions.BanMembers != v) continue;
                if (Permissions.ManageMessages != v) continue;
                if (Permissions.ManageNicknames != v) continue;
                if (Permissions.ManageWebhooks != v) continue;
                if (Permissions.ManageWebhooks != v) continue;
                
                s += $"{v.ToPermissionString()}, ";
            }

            var ss = user.Roles.Select(x => x.Mention).Aggregate("", (current, v) => current + (v + " "));

            var builder = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Grayple,
                    Title = $"Who is {user.DisplayName}#{user.Discriminator}",
                    Description = $"Account information about {user.DisplayName}#{user.Discriminator}.",
                    Timestamp = DateTimeOffset.UtcNow
                }
                .WithFooter($"ID : {user.Id}")
                .WithThumbnail(user.AvatarUrl);
                builder.AddField("Joined at :", user.JoinedAt.ToString(), true);
                builder.AddField("Account creation :", user.CreationTimestamp.ToString(), true);
                builder.AddField($"Roles [{user.Roles.Count()}]", string.IsNullOrEmpty(ss) ? "-" : ss);
                builder.AddField("Permissions", string.IsNullOrEmpty(s) ? "-" : s);

            await ctx.RespondAsync(embed: builder);
        }

        [Command("cat")]
        public async Task Cat(CommandContext ctx)
        {
            if (!ServerSettingsDatabase.GetServerSettings(ctx.Guild.Id).Other.CatCmdEnabled)
            {
                await ctx.RespondAsync(":x: `;cat` is not enabled on this server!");
                return;
            }
            
            var builder = new DiscordEmbedBuilder
                {
                    Title = "What a lovely cat! :cat:",
                    ImageUrl = TheCatApi.Get().Result,
                    Color = DiscordColor.Grayple
                }
                .WithFooter($"ID : {ctx.Member.Id}")
                .WithTimestamp(DateTime.Now)
                .WithAuthor($"Requested by {ctx.User.Username}#{ctx.User.Discriminator}", iconUrl: ctx.User.AvatarUrl);
            builder.Description = "Link if you don't see it : " + builder.ImageUrl;
            await ctx.RespondAsync(embed: builder);
        }
        
        [Command("dog")]
        public async Task Dog(CommandContext ctx)
        {
            if (!ServerSettingsDatabase.GetServerSettings(ctx.Guild.Id).Other.DogCmdEnabled)
            {
                await ctx.RespondAsync(":x: `;dog` is not enabled on this server!");
                return;
            }
            
            var builder = new DiscordEmbedBuilder
                {
                    Title = "What a lovely dog! :dog:",
                    ImageUrl = TheDogApi.Get().Result,
                    Color = DiscordColor.Grayple
                }
                .WithFooter($"ID : {ctx.Member.Id}")
                .WithTimestamp(DateTime.Now)
                .WithAuthor($"Requested by {ctx.User.Username}#{ctx.User.Discriminator}", iconUrl: ctx.User.AvatarUrl);
            builder.Description = "Link if you don't see it : " + builder.ImageUrl;
            await ctx.RespondAsync(embed: builder);
        }
        
        [Command("pokemon")]
        public async Task Pokemon(CommandContext ctx, string name)
        {
            await ctx.TriggerTypingAsync();

            Pokemon p;
            try
            {
                p = DataFetcher.GetNamedApiObject<Pokemon>(name.ToLower()).Result;
            }
            catch (AggregateException)
            {
                await ctx.RespondAsync(":x: Invalid Pokemon name!");
                return;
            }
            
            var pTypes = p.Types.Aggregate("", (current, v) => current + (v.Type.Name.Capitalize() + ", "));
            var pAbilities = p.Abilities.Aggregate("", (current, v) => current + (v.Ability.Name.Capitalize() + ", \n"));
            var pStats = p.Stats.Aggregate("", (current, v) => current + (v.Stat.Name.Capitalize() + $" [{v.BaseValue}]" + ", \n"));
            var pMoves = p.Moves.Take(5).Aggregate("", (current, v) => current + (v.Move.Name.Capitalize() + ", \n"));

            var builder = new DiscordEmbedBuilder
                {
                    Title = $"<:pokeball:783397731455991829> {p.Name.Capitalize()}",
                    Color = DiscordColor.Grayple
                }
                .WithFooter($"ID : {ctx.Member.Id}")
                .WithTimestamp(DateTime.Now)
                .WithAuthor($"Requested by {ctx.User.Username}#{ctx.User.Discriminator}",
                    iconUrl: ctx.User.AvatarUrl)
                .WithThumbnail(
                    $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{p.ID}.png")
                .AddField("Height", Math.Round((float) p.Height / 10, 1) + "m (" + Math.Round(p.Mass / 2.771, 1) + "ft)", true)
                .AddField("Weight", Math.Round((float) p.Mass / 10, 1) + "kg (" + Math.Round(p.Mass / 4.536, 1) + "lbs)", true)
                .AddField("Types", pTypes.Substring(0, pTypes.Length - 2), true)
                .AddField($"Abilities [{p.Abilities.Length}]", pAbilities.Substring(0, pAbilities.Length - 3), true)
                .AddField("Stats", pStats.Substring(0, pStats.Length - 3), true)
                .AddField($"Moves [{p.Moves.Length}]", pMoves.Substring(0, pMoves.Length - 3), true);
            
            var message = await ctx.RespondAsync(embed: builder);
            
            await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":twisted_rightwards_arrows:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":camera_with_flash:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":sparkles:"));

            var interactivity = ctx.Client.GetInteractivity();
            _ = Task.Run(async () =>
            {
                var shiny = DiscordEmoji.FromName(ctx.Client, ":sparkles:");
                var sprites = DiscordEmoji.FromName(ctx.Client, ":camera_with_flash:");
                var moves = DiscordEmoji.FromName(ctx.Client, ":twisted_rightwards_arrows:");
                
                while (true)
                {
                    var result = await interactivity.WaitForReactionAsync(message, ctx.User, TimeSpan.FromSeconds(30));

                    if (result.TimedOut)
                    {
                        await message.DeleteAllReactionsAsync();
                        return;
                    }

                    if (result.Result.Emoji == moves)
                    {
                        var pMovesReact = p.Moves.Aggregate("", (current, v) => current + (v.Move.Name.Capitalize() + ", "));
                        await ctx.RespondAsync(pMovesReact.Remove(pMovesReact.Length - 3, 3));
                        await result.Result.Message.DeleteReactionAsync(moves, result.Result.User);
                    }
                    
                    else if (result.Result.Emoji == sprites)
                    {
                        await ctx.RespondAsync(p.Sprites.FrontMale);
                        await ctx.RespondAsync(p.Sprites.BackMale);
                        await result.Result.Message.DeleteReactionAsync(sprites, result.Result.User);
                    }
                    
                    else if (result.Result.Emoji == shiny)
                    {
                        await ctx.RespondAsync(p.Sprites.FrontShinyMale);
                        await ctx.RespondAsync(p.Sprites.BackShinyMale);
                        await result.Result.Message.DeleteReactionAsync(shiny, result.Result.User);
                    }
                }
            });
        }

        [Command("lounge"), Description("Creates a temporary voice channel. Has a cooldown of 10 seconds. Use `;loungejoin` to ask to join a private lounge."), Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Lounge(CommandContext ctx, [Description("Can be either `public` or `private`.")] string visibility = "public", [Description("Name of the lounge, be sure to put the name between **quotes**!")] string name = null, [Description("User limit of the lounge.")] int maxUsers = 0)
        {
            if (!ServerSettingsDatabase.GetServerSettings(ctx.Guild.Id).Other.LoungesEnabled)
            {
                await ctx.RespondAsync(":x: Lounges are not enabled on this server!");
                return;
            }
            
            DiscordChannel lounge = null;

            switch (visibility)
            {
                case "public":
                    lounge = await ctx.Guild.CreateChannelAsync(name ?? $"{ctx.User.Username}'s lounge", ChannelType.Voice, ctx.Channel.Parent, userLimit: maxUsers);
                    break;
                case "private":
                    lounge = await ctx.Guild.CreateChannelAsync(name ?? $"{ctx.User.Username}'s private lounge", ChannelType.Voice, ctx.Channel.Parent, userLimit: maxUsers);
                    await lounge.AddOverwriteAsync(ctx.Guild.Roles.First(x => x.Value.Name.ToLower() == "@everyone").Value, Permissions.None, Permissions.UseVoice);
                    await lounge.AddOverwriteAsync(ctx.Member, Permissions.UseVoice);
                    break;
            }

            if (lounge == null) return;
            _ = Task.Run(async () =>
            {
                await Task.Delay(10000);
                while (lounge.Users.Any())
                {
                    await Task.Delay(10000);
                }
                await lounge.DeleteAsync("Lounge expired");
            });
        }
        
        [Command("loungejoin"), Aliases("loungej"), Description("Allows a user to ask to join a private lounge.")]
        public async Task LoungeJoin(CommandContext ctx, [Description("Target member to make the request to.")] DiscordMember member)
        {
            var interactivity = ctx.Client.GetInteractivity();
            _ = Task.Run(async () =>
            {
                if (member.VoiceState == null || !member.VoiceState.Channel.PermissionOverwrites.Any(x =>
                    x.Allowed == Permissions.UseVoice && x.Id == member.Id && x.Type == OverwriteType.Member &&
                    x.Denied == Permissions.None))
                {
                    await ctx.RespondAsync($"{member.Nickname} is not in a lounge!");
                    return;
                }
                
                var request = await ctx.RespondAsync(member.Mention,embed: new DiscordEmbedBuilder
                {
                    Description = $"React with :white_check_mark: to allow **{ctx.Member.Username}** to connect to your lounge." +
                                  "\nThis request will expire in 30 seconds. There is no way to revoke the authorization.",
                    Color = DiscordColor.Gray
                }
                    .WithAuthor($"{ctx.Member.Username}#{ctx.Member.Discriminator} requested to join your private lounge!", iconUrl: ctx.Member.AvatarUrl));

                await request.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));

                var result = await interactivity.WaitForReactionAsync(request, member, TimeSpan.FromSeconds(30));

                if (result.TimedOut)
                {
                    await request.DeleteAllReactionsAsync();
                    await request.ModifyAsync(embed: new DiscordEmbedBuilder
                        {
                            Description = "Request has expired!",
                            Color = DiscordColor.DarkButNotBlack
                        }
                        .WithAuthor(
                            $"[EXPIRED] {member.Username}#{member.Discriminator} requested to join your private lounge!",
                            iconUrl: ctx.Member.AvatarUrl).Build());
                    return;
                }
                if (result.Result.Emoji == DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"))
                {
                    var voice = member.VoiceState;
                    if (voice == null)
                    {
                        await ctx.RespondAsync($"{member.Username} is not connect to the lounge anymore!");
                        return;
                    }

                    await voice.Channel.AddOverwriteAsync(ctx.Member, Permissions.UseVoice);
                    await ctx.RespondAsync($"{member.Mention}, {ctx.User.Mention} can now join your lounge.");
                }
            });
        }

        [Command("ping")]
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public async Task Ping(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Grayple,
                Title = "Pong!"
            }
                .AddField("Discord", "Pinging... <a:loading:788448736527384606>", true)
                .AddField("Database", "Pinging... <a:loading:788448736527384606>", true)
                .AddField("Google", "Pinging... <a:loading:788448736527384606>", true)
                .AddField("TheCatApi", "Pinging... <a:loading:788448736527384606>", true)
                .AddField("TheDogApi", "Pinging... <a:loading:788448736527384606>", true)
                .AddField("PokeApi", "Pinging... <a:loading:788448736527384606>", true);
            var message = await ctx.RespondAsync(embed: embed);

            bool dbPing;
            await using var connection = new MySqlConnection(Token.ConnectionString);
            {
                try
                {
                    await connection.OpenAsync();
                    dbPing = await connection.PingAsync();
                    await connection.CloseAsync();
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
            var dbPingString = dbPing ? ":bar_chart: Alive :green_heart:" : ":bar_chart: Dead :broken_heart:" ;

            var googlePing = "google.com".GetPing();
            var googlePingString = googlePing == 0 ? ":bar_chart: Timeout :broken_heart:" : $":bar_chart: {googlePing}ms :{googlePing.GetHeartColor()}heart:";

            var discordPing = ctx.Client.Ping;
            var discordPingString = discordPing == 0 ? ":bar_chart: Timeout :broken_heart:" : $":bar_chart: {discordPing}ms :{discordPing.GetHeartColor()}heart:";

            var catApiPing = "thecatapi.com".GetPing();
            var catApiPingString = catApiPing == 0 ? ":bar_chart: Timeout :broken_heart:" : $":bar_chart: {catApiPing}ms :{catApiPing.GetHeartColor()}heart:";

            var dogApiPing = "thedogapi.com".GetPing();
            var dogApiPingString = dogApiPing == 0 ? ":bar_chart: Timeout :broken_heart:" : $":bar_chart: {dogApiPing}ms :{dogApiPing.GetHeartColor()}heart:";

            var pokeApiPing = "pokeapi.co".GetPing();
            var pokeApiPingString = pokeApiPing == 0 ? ":bar_chart: Timeout :broken_heart:" : $":bar_chart: {pokeApiPing}ms :{pokeApiPing.GetHeartColor()}heart:";

            var embedPing = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Grayple,
                    Title = "Pong! :ping_pong:"
                }
                .AddField("Discord", discordPingString, true)
                .AddField("Database", dbPingString, true)
                .AddField("Google", googlePingString, true)
                .AddField("TheCatApi", catApiPingString, true)
                .AddField("TheDogApi", dogApiPingString, true)
                .AddField("PokeApi", pokeApiPingString, true);
            await message.ModifyAsync(embed: new Optional<DiscordEmbed>(embedPing));
            
        }
    }
}