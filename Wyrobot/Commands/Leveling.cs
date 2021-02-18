using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Wyrobot.Core.Database;
using Wyrobot.Core.Models;

// ReSharper disable UnusedMember.Global

namespace Wyrobot.Core.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LevelingCommands : BaseCommandModule
    {
        [Command("setlevel"), RequirePermissions(Permissions.Administrator), Description("Command used to set the level of someone. Requires administrator permissions.")]
        public async Task SetLevel(CommandContext ctx,[Description("User to set the level to.")] DiscordUser user, [Description("Level to set.")]int level)
        {
            var userLevel = new UserLevel
            {
                UserId = user.Id,
                GuildId = ctx.Guild.Id,
                Level = level
            };
            UserLevelDatabase.SetUserLevelInfoLevel(userLevel);

            var iUser = await ctx.Client.GetUserAsync(user.Id);

            await ctx.RespondAsync($"Level correctly set to **{level}** for **{iUser.Username}#{iUser.Discriminator}**");
        }
        
        [Command("addxp"), RequirePermissions(Permissions.Administrator), Description("Command used to set the xp of someone. Requires administrator permissions.")]
        public async Task AddXp(CommandContext ctx,[Description("User to set the xp to.")] DiscordUser user, [Description("Xp to set.")]int xp)
        {
            var userLevel = new UserLevel
            {
                UserId = user.Id,
                GuildId = ctx.Guild.Id,
                Xp = xp
            };
            UserLevelDatabase.SetUserLevelInfoXp(userLevel, ctx.Channel);

            var iUser = await ctx.Client.GetUserAsync(user.Id);

            await ctx.RespondAsync($"Xp correctly set to **{xp}** for **{iUser.Username}#{iUser.Discriminator}**");
        }

        [Command("rank"), Description("Shows the level and the XP of the user or a give user.")]
        public async Task Rank(CommandContext ctx, [Description("User to show the level.")] DiscordUser discordUser = null)
        {
            var userId = discordUser != null ? discordUser.Id : ctx.User.Id;

            var list = UserLevelDatabase.GetUserLevelInfo(ctx.Guild.Id, userId);

            if (list.XpToNextLevel != 0)
            {
                var user = await ctx.Guild.GetMemberAsync(Convert.ToUInt64(userId));
                var embedBuilder = new DiscordEmbedBuilder {Title = "Rank"};
                try
                {
                    var color = user.Roles.First().Color;
                    embedBuilder.Color = color.ToString() != "#000000" ? color : DiscordColor.Grayple;
                }
                catch
                {
                    embedBuilder.Color = DiscordColor.Grayple;
                }
                
                embedBuilder.WithThumbnail(user.AvatarUrl);
                embedBuilder.WithFooter("Wyrobot#7218");

                embedBuilder.AddField(user.DisplayName, $"Level: **{list.Level}**\nCurrent XP: **{list.Xp}**/{list.XpToNextLevel}");

                DiscordEmbed embed = embedBuilder;
                
                await ctx.RespondAsync(null, false, embed);
            }
            else
                await ctx.RespondAsync("User didn't talk yet!");
        }

        [Command("leaderboard"), Aliases("lb"), Description("Shows the leaderboard for this guild.")]
        public async Task Leaderboard(CommandContext ctx, [Description("Page to show. Defaults to 1.")] int page = 1)
        {
            var list = UserLevelDatabase.GetGuildLevelInfo(ctx.Guild.Id).ToList();

            var length = list.Count;

            var pageNumber = length;
            
            while (pageNumber % 10 != 0)
                pageNumber++;

            pageNumber /= 10;
            
            if (page > pageNumber)
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, that page doesn't exist!");
                return;
            }
            
            
            list.RemoveRange(0, (page - 1) * 10);
            
            var embedBuilder = new DiscordEmbedBuilder();

            var place = (page - 1) * 10 + 1;
            foreach (var v in list.Take(10))
            {
                try
                {
                    var userId = v.UserId;
                    var user = await ctx.Guild.GetMemberAsync(userId);

                    switch (place)
                    {
                        case 1:
                            embedBuilder.AddField($"{place}. {user.DisplayName}#{user.Discriminator} <a:DiamondShine:774744659792625685> ", $"Level: {v.Level}, XP: {v.Xp}/{v.XpToNextLevel}");
                            if (user.PremiumSince != null)
                                embedBuilder.Fields.Last().Name += "<a:Nitro_Booster:774956381240950794>";
                            break;
                            
                        case 2:    
                            embedBuilder.AddField($"{place}. {user.DisplayName}#{user.Discriminator} <:gold_ingot:774744395588829185> ", $"Level: {v.Level}, XP: {v.Xp}/{v.XpToNextLevel}");
                            if (user.PremiumSince != null)
                                embedBuilder.Fields.Last().Name += "<a:Nitro_Booster:774956381240950794>";
                            break;
                            
                        case 3:
                            embedBuilder.AddField($"{place}. {user.DisplayName}#{user.Discriminator} <:iron_ingot:774744803657121843> ", $"Level: {v.Level}, XP: {v.Xp}/{v.XpToNextLevel}");
                            if (user.PremiumSince != null)
                                embedBuilder.Fields.Last().Name += "<a:Nitro_Booster:774956381240950794>";
                            break;
                        
                        default:
                            embedBuilder.AddField($"{place}. {user.DisplayName}#{user.Discriminator}", $"Level: {v.Level}, XP: {v.Xp}/{v.XpToNextLevel}");
                            if (user.PremiumSince != null)
                                embedBuilder.Fields.Last().Name += "<a:Nitro_Booster:774956381240950794>";
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                place++;
            }

            embedBuilder.Color = DiscordColor.Grayple;
            embedBuilder.WithTitle("Global ranking of this server");
            embedBuilder.WithFooter($"Page {page}/{pageNumber}\nWyrobot#7218");

            DiscordEmbed embed = embedBuilder;

            await ctx.RespondAsync(null, false, embed);
        }
    }
}