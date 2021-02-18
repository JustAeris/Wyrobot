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
    [Group("autorole")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AutoRoleCommands : BaseCommandModule
    {
        [Command("add"), RequirePermissions(Permissions.Administrator)]
        public async Task AddAutoRole(CommandContext ctx, DiscordRole role)
        {
            AutoRoleDatabase.InsertAutoRole(ctx.Guild.Id, role.Id);
            await ctx.RespondAsync("Successfully added the auto-role!");
        }
        
        [Command("remove"), Aliases("del", "delete"), RequirePermissions(Permissions.Administrator)]
        public async Task RemoveAutoRole(CommandContext ctx, DiscordRole role)
        {
            AutoRoleDatabase.DeleteAutoRole(ctx.Guild.Id, role.Id);
            await ctx.RespondAsync("Successfully removed the auto-role!");
        }
        
        [Command("list")]
        public async Task ListAutoRoles(CommandContext ctx)
        {
            var list = AutoRoleDatabase.GetAutoRoles(ctx.Guild.Id).ToList();
            
            if (!list.Any())
            {
                await ctx.RespondAsync(":x: No level rewards have been set for this server.");
                return;
            }
            
            var interactivity = ctx.Client.GetInteractivity();

            var s = list.Select(vReward => ctx.Guild.GetRole(vReward.RoleId)).Aggregate("", (current, role) => current + $"Role : {role.Mention}\n");

            await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, interactivity.GeneratePagesInEmbed(s.Remove(s.Length - 1), SplitType.Line, new DiscordEmbedBuilder
                {
                    Title = "Auto-roles list :",
                    Color = DiscordColor.Grayple
                }
                .WithFooter($"ID : {ctx.User.Id}")
                .WithTimestamp(DateTime.UtcNow)));
        }
    }
}