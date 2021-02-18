using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Wyrobot.Core.Database;

// ReSharper disable UnusedMember.Global

namespace Wyrobot.Core.Commands
{
    [Group("levelreward")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LevelRewardsCommands : BaseCommandModule
    {
        [Command("add"), RequirePermissions(Permissions.Administrator)]
        public async Task AddReward(CommandContext ctx, DiscordRole role, int requiredLevel)
        {
            LevelRewardsDatabase.InsertLevelReward(ctx.Guild.Id, requiredLevel, role.Id);
            await ctx.RespondAsync("Successfully added the level reward!");
        }
        
        [Command("remove"), Aliases("del", "delete"), RequirePermissions(Permissions.Administrator)]
        public async Task RemoveReward(CommandContext ctx, DiscordRole role, int requiredLevel)
        {
            LevelRewardsDatabase.DeleteLevelReward(ctx.Guild.Id, requiredLevel, role.Id);
            await ctx.RespondAsync("Successfully removed the level reward!");
        }
        
        [Command("list")]
        public async Task ListReward(CommandContext ctx)
        {
            var list = LevelRewardsDatabase.GetLevelReward(ctx.Guild.Id).ToList();

            if (!list.Any())
            {
                await ctx.RespondAsync(":x: No level rewards have been set for this server.");
                return;
            }
            
            var interactivity = ctx.Client.GetInteractivity();

            var s = "";
            foreach (var vReward in list.OrderBy(x => x.RequiredLevel))
            {
                try
                {
                    var role = ctx.Guild.GetRole(vReward.RoleId);
                    s += $"Role : {role.Mention} - Required level : {vReward.RequiredLevel}\n";
                }
                catch
                {
                    // ignored
                }
            }
            
            await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, interactivity.GeneratePagesInEmbed(s.Remove(s.Length - 1), SplitType.Line, new DiscordEmbedBuilder
                {
                    Title = "Level rewards :",
                    Color = DiscordColor.Grayple
                }
                .WithFooter($"ID : {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)));
        }
    }
}