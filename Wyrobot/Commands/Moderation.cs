using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Wyrobot.Core.Database;
using Wyrobot.Core.Models;

#pragma warning disable 1998

// ReSharper disable UnusedMember.Global

namespace Wyrobot.Core.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModerationCommands : BaseCommandModule
    {
        [Command("nuke"), Cooldown(2, 30, CooldownBucketType.Channel), RequirePermissions(Permissions.ManageChannels), Description("Deletes a given number of messages from the last one in the current channel.")]
        public async Task Nuke(CommandContext ctx, [Description("Number of messages to delete. Can't be higher than 20.")] int num)
        {
            if (num == 0)
            {
                await ctx.Message.DeleteAsync();
                var autoDeleteMessage = await ctx.RespondAsync($"No number of messages to delete given ! {ctx.User.Mention}");
                await Task.Delay(5000);
                await autoDeleteMessage.DeleteAsync();
                return;
            }

            if (num > 20)
            {
                await ctx.Message.DeleteAsync();
                var autoDeleteMessage = await ctx.RespondAsync($"Too much messages to delete at once! {ctx.User.Mention}");
                await Task.Delay(5000);
                await autoDeleteMessage.DeleteAsync();
                return;
            }
            
            var messages = ctx.Channel.GetMessagesAsync(num + 1).Result.Where(x => x.Timestamp > DateTimeOffset.Now - TimeSpan.FromDays(14));
            await ctx.Channel.DeleteMessagesAsync(messages, $"Nuked by {ctx.User.Tag()}");
            var autoDeleteMes = await ctx.RespondAsync($"Nuked {num} messages! {ctx.User.Mention}");
            await Task.Delay(5000);
            await autoDeleteMes.DeleteAsync();

            var settings = ServerSettingsDatabase.GetServerSettings(ctx.Guild.Id).Logging;
            if (settings.LogPunishments && settings.Enabled && settings.ChannelId != 0)
            {
                var embed = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Goldenrod
                    }
                    .WithTitle($"{ctx.Member.Tag()} has nuked {num} messages. :wastebasket:")
                    .WithFooter($"Member ID : {ctx.Member.Id}")
                    .WithTimestamp(DateTime.Now);
                        
                var channel = ctx.Guild.GetChannel(settings.ChannelId);
                await channel.SendMessageAsync(embed: embed);
            }
        }

        
        [Command("ban"), Description("Perma-ban a user."), RequirePermissions(Permissions.BanMembers)]
        public async Task Ban(CommandContext ctx,[Description("User to ban.")]DiscordMember member, [RemainingText, Description("(Optional) Reason for the ban.")]string reason = null)
        {
            if (!ctx.Member.CanPunish(member))
            {
                await ctx.RespondAsync("You cannot ban someone that has higher roles than you!");
                return;
            }
            
            if (BanDatabase.GetBans(ctx.Guild.Id).Any(x => x.UserId == member.Id) || BanDatabase.GetActiveBans(ctx.Guild.Id).Any(x => x.UserId == member.Id))
            {
                await ctx.RespondAsync(":no_mouth: User is already banned!");
                return;
            }
            
            var embedBuilder = new DiscordEmbedBuilder().WithTimestamp(DateTime.UtcNow);

            embedBuilder.AddField("Success !", $"{member.DisplayName}#{member.Discriminator} has been successfully banned ! :hammer:");
            if (reason != null)
                embedBuilder.AddField("Reason ...", reason);

            embedBuilder.WithColor(DiscordColor.Red);
            embedBuilder.WithThumbnail(member.AvatarUrl);
            embedBuilder.WithTitle("Ban !");
            embedBuilder.WithFooter($"ID : {member.Id}");
            
            await member.SendMessageAsync($"You have been **banned** from *{ctx.Guild.Name}*.");

            DiscordEmbed embed = embedBuilder;
            await ctx.RespondAsync(null, false, embed);

            var ban = new Ban
            {
                UserId = member.Id,
                GuildId = ctx.Guild.Id,
                IssuedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss").ToDateTime(),
                Username = $"{member.DisplayName}#{member.Discriminator}",
                Reason = reason
            };
            BanDatabase.InsertBan(ban);
            
            await member.BanAsync(0, reason + $" Banned by {ctx.Member.DisplayName}#{ctx.Member.Discriminator}");
            
            var settings = ServerSettingsDatabase.GetServerSettings(ctx.Guild.Id);

            if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogPunishments)
            {
                var logEmbed = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Red,
                        Description = $"Reason : {reason ?? "No reason provided"}"
                    }
                    .WithTitle($"{ctx.Member.Tag()} banned {member.Tag()}. :lock:")
                    .WithFooter($"Member ID : {ctx.Member.Id}")
                    .WithTimestamp(DateTime.Now);
                        
                var channel = ctx.Guild.GetChannel(settings.Logging.ChannelId);
                await channel.SendMessageAsync(embed: logEmbed);
            }
        }

        [Command("tempban"), Description("Temporarily ban a user."), RequirePermissions(Permissions.BanMembers), Aliases("tban")]
        public async Task TempBan(CommandContext ctx, [Description("User to ban.")] DiscordMember member, [Description("Ban duration.")] TimeSpan duration, [RemainingText, Description("(Optional) Reason for the ban.")] string reason = null)
        {
            if (!ctx.Member.CanPunish(member))
            {
                await ctx.RespondAsync("You cannot ban someone that has higher roles than you!");
                return;
            }
            
            if (BanDatabase.GetBans(ctx.Guild.Id).Any(x => x.UserId == member.Id) || BanDatabase.GetActiveBans(ctx.Guild.Id).Any(x => x.UserId == member.Id))
            {
                await ctx.RespondAsync(":no_mouth: User is already banned!");
                return;
            }
            
            var embedBuilder = new DiscordEmbedBuilder().WithTimestamp(DateTime.UtcNow);

            embedBuilder.AddField("Success !", $"{member.DisplayName}#{member.Discriminator} has been successfully banned for **{duration.TotalDays}** days ! :hammer:");
            if (reason != null)
                embedBuilder.AddField("Reason ...", reason);

            embedBuilder.WithColor(DiscordColor.Red);
            embedBuilder.WithThumbnail(member.AvatarUrl);
            embedBuilder.WithTitle("Ban !");
            embedBuilder.WithFooter($"ID : {member.Id}");

            await member.SendMessageAsync($"You have been **banned** from *{ctx.Guild.Name}* for {duration.TotalDays} days.");

            DiscordEmbed embed = embedBuilder;
            await ctx.RespondAsync(null, false, embed);

            var ban = new Ban
            {
                UserId = member.Id,
                GuildId = ctx.Guild.Id,
                IssuedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss").ToDateTime(),
                ExpiresAt = (DateTime.UtcNow + duration).ToString("yyyy-MM-dd HH:mm:ss").ToDateTime(),
                Username = $"{member.DisplayName}#{member.Discriminator}",
                Reason = reason
            };
            BanDatabase.InsertTempBan(ban);
            
            await member.BanAsync(0, reason + $" Banned by {ctx.Member.DisplayName}#{ctx.Member.Discriminator}");
            
            var settings = ServerSettingsDatabase.GetServerSettings(ctx.Guild.Id);

            if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogPunishments)
            {
                var logEmbed = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Red,
                        Description = $"Duration : {duration.ToString()}\nReason : {reason ?? "No reason provided"}"
                    }
                    .WithTitle($"{ctx.Member.Tag()} temp-banned {member.Tag()}. :lock:")
                    .WithFooter($"Member ID : {ctx.Member.Id}")
                    .WithTimestamp(DateTime.Now);
                        
                var channel = ctx.Guild.GetChannel(settings.Logging.ChannelId);
                await channel.SendMessageAsync(embed: logEmbed);
            }
        }

        [Command("banlist"), Description("Get all of the actives bans for the current server."), RequirePermissions(Permissions.Administrator)]
        public async Task Banlist(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var embedBuilder = new DiscordEmbedBuilder().WithTimestamp(DateTime.UtcNow);

            foreach (var v in BanDatabase.GetBans(ctx.Guild.Id).Where(v => v != null))
                embedBuilder.AddField($"User: {v.Username}", $"Reason {v.Reason}\nIssued at: {v.IssuedAt}");

            foreach (var v in BanDatabase.GetActiveBans(ctx.Guild.Id).Where(v => v != null))
                embedBuilder.AddField($"User: {v.Username}", $"Reason: {v.Reason}\nIssued at: {v.IssuedAt} and expires at: {v.ExpiresAt}\nDuration: {Convert.ToDateTime(v.ExpiresAt) - Convert.ToDateTime(v.IssuedAt)}, time left: {(Convert.ToDateTime(v.ExpiresAt) - DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm:ss").ToDateTime()}");

            embedBuilder.WithFooter($"ID : {ctx.Member.Id}");
            embedBuilder.Color = DiscordColor.Grayple;
            DiscordEmbed embed = embedBuilder;

            if (embed.Fields.Count > 0)
                await ctx.RespondAsync(null, false, embed);
            else
                await ctx.RespondAsync("No bans have been found ! :ok_hand:");
        }

        [Command("kick"), Description("Kick a user."), RequirePermissions(Permissions.KickMembers)]
        public async Task Kick(CommandContext ctx, [Description("User to kick.")] DiscordMember member, [RemainingText, Description("(Optional) Reason for the kick.")] string reason = null)
        {
            if (!ctx.Member.CanPunish(member))
            {
                await ctx.RespondAsync("You cannot kick someone that has higher roles than you!");
                return;
            }
            
            var embedBuilder = new DiscordEmbedBuilder().WithTimestamp(DateTime.UtcNow);

            embedBuilder.AddField("Success !", $"{member.DisplayName}#{member.Discriminator} has been successfully kicked ! :boom:");
            if (reason != null)
                embedBuilder.AddField("Reason ...", reason);

            embedBuilder.WithColor(DiscordColor.Orange);
            embedBuilder.WithThumbnail(member.AvatarUrl);
            embedBuilder.WithTitle("Kick !");
            embedBuilder.WithFooter($"ID : {member.Id}");

            if (!member.IsBot)
                await member.SendMessageAsync($"You have been **kicked** from *{ctx.Guild.Name}*.");

            DiscordEmbed embed = embedBuilder;
            await ctx.RespondAsync(null, false, embed);

            await member.RemoveAsync(reason + $" Kicked by {ctx.Member.DisplayName}#{ctx.Member.Discriminator}");
            
            var settings = ServerSettingsDatabase.GetServerSettings(ctx.Guild.Id);

            if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogPunishments)
            {
                var logEmbed = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Orange,
                        Description = $"Reason : {reason ?? "No reason provided"}"
                    }
                    .WithTitle($"{ctx.Member.Tag()} kicked {member.Tag()}. :door:")
                    .WithFooter($"Member ID : {ctx.Member.Id}")
                    .WithTimestamp(DateTime.Now);
                        
                var channel = ctx.Guild.GetChannel(settings.Logging.ChannelId);
                await channel.SendMessageAsync(embed: logEmbed);
            }
        }

        [Command("mute"), Description("Perma-mute a user."), RequirePermissions(Permissions.ManageRoles)]
        public async Task Mute(CommandContext ctx, [Description("User to mute.")] DiscordMember member, [RemainingText, Description("(Optional) Reason for the mute.")] string reason = null)
        {
            if (!ctx.Member.CanPunish(member))
            {
                await ctx.RespondAsync("You cannot mute someone that has higher roles than you!");
                return;
            }
            
            if (MuteDatabase.GetMutes(ctx.Guild.Id).Any(x => x.UserId == member.Id) || MuteDatabase.GetActiveMutes(ctx.Guild.Id).Any(x => x.UserId == member.Id))
            {
                await ctx.RespondAsync(":no_mouth: User is already muted!");
                return;
            }
            
            var embedBuilder = new DiscordEmbedBuilder().WithTimestamp(DateTime.UtcNow);

            embedBuilder.AddField("Success !", $"{member.DisplayName}#{member.Discriminator} has been successfully muted ! :mute:");
            if (reason != null)
                embedBuilder.AddField("Reason ...", reason);

            embedBuilder.WithColor(DiscordColor.Yellow);
            embedBuilder.WithThumbnail(member.AvatarUrl);
            embedBuilder.WithTitle("Mute !");
            embedBuilder.WithFooter($"ID : {member.Id}");

            await member.SendMessageAsync($"You have been **muted** from *{ctx.Guild.Name}*.");

            DiscordEmbed embed = embedBuilder;
            await ctx.RespondAsync(null, false, embed);
            
            var mute = new Mute
            {
                UserId = member.Id,
                GuildId = ctx.Guild.Id,
                IssuedAt = DateTime.UtcNow,
                Username = $"{member.DisplayName}#{member.Discriminator}",
                Reason = reason
            };

            MuteDatabase.InsertMute(mute);

            var muteRole = ctx.Guild.GetRole(ServerSettingsDatabase.GetServerSettings(ctx.Guild.Id).Moderation.MuteRoleId);
            await member.GrantRoleAsync(muteRole, reason + $" Muted by {ctx.Member.DisplayName}#{ctx.Member.Discriminator}");
            
            var settings = ServerSettingsDatabase.GetServerSettings(ctx.Guild.Id);

            if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogPunishments)
            {
                var logEmbed = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Yellow,
                        Description = $"Reason : {reason ?? "No reason provided"}"
                    }
                    .WithTitle($"{ctx.Member.Tag()} muted {member.Tag()}. :mute:")
                    .WithFooter($"Member ID : {ctx.Member.Id}")
                    .WithTimestamp(DateTime.Now);
                        
                var channel = ctx.Guild.GetChannel(settings.Logging.ChannelId);
                await channel.SendMessageAsync(embed: logEmbed);
            }
        }

        [Command("tempmute"), Description("Temporarily mutes a user."), RequireUserPermissions(Permissions.ManageRoles), Aliases("tmute")]
        public async Task TempMute(CommandContext ctx, [Description("User to mute.")] DiscordMember member, [Description("Mute duration.")] TimeSpan duration, [RemainingText, Description("(Optional) Reason for the ban.")] string reason = null)
        {
            if (!ctx.Member.CanPunish(member))
            {
                await ctx.RespondAsync("You cannot mute someone that has higher roles than you!");
                return;
            }
            
            if (MuteDatabase.GetMutes(ctx.Guild.Id).Any(x => x.UserId == member.Id) || MuteDatabase.GetActiveMutes(ctx.Guild.Id).Any(x => x.UserId == member.Id))
            {
                await ctx.RespondAsync(":no_mouth: User is already muted!");
                return;
            }
            
            var embedBuilder = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Yellow)
                .WithThumbnail(member.AvatarUrl)
                .WithTitle("Mute !")
                .WithTimestamp(DateTime.UtcNow);

            embedBuilder.AddField("Success !", $"{member.DisplayName}#{member.Discriminator} has been successfully muted for **{duration.TotalHours}** hours ! :mute:");
            if (reason != null)
                embedBuilder.AddField("Reason ...", reason);

            await member.SendMessageAsync($"You have been **muted** from *{ctx.Guild.Name}* for {duration.TotalHours} hours.");

            embedBuilder.WithFooter($"ID : {member.Id}");
            DiscordEmbed embed = embedBuilder;
            await ctx.RespondAsync(null, false, embed);

            var mute = new Mute
            {
                UserId = member.Id,
                GuildId = ctx.Guild.Id,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = (DateTime.UtcNow + duration).ToString("yyyy-MM-dd HH:mm:ss").ToDateTime(),
                Username = $"{member.DisplayName}#{member.Discriminator}",
                Reason = reason
            };
            
            MuteDatabase.InsertTempMute(mute);

            var muteRole = ctx.Guild.GetRole(ServerSettingsDatabase.GetServerSettings(ctx.Guild.Id).Moderation.MuteRoleId);
            await member.GrantRoleAsync(muteRole, reason + $" Muted by {ctx.Member.DisplayName}#{ctx.Member.Discriminator}");  
            
            var settings = ServerSettingsDatabase.GetServerSettings(ctx.Guild.Id);

            if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogPunishments)
            {
                var logEmbed = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Yellow,
                        Description = $"Duration : {duration.ToString()}\nReason : {reason ?? "No reason provided"}"
                    }
                    .WithTitle($"{ctx.Member.Tag()} temp-muted {member.Tag()}. :mute:")
                    .WithFooter($"Member ID : {ctx.Member.Id}")
                    .WithTimestamp(DateTime.Now);
                    
                var channel = ctx.Guild.GetChannel(settings.Logging.ChannelId);
                await channel.SendMessageAsync(embed: logEmbed);
            }
        }

        [Command("unmute"), RequirePermissions(Permissions.ManageRoles)]
        public async Task Unmute(CommandContext ctx, [Description("User to unmute.")] DiscordMember member)
        {
            if (!ctx.Member.CanPunish(member))
            {
                await ctx.RespondAsync("You cannot unmute someone that has higher roles than you!");
                return;
            }
            
            if (MuteDatabase.GetMutes(ctx.Guild.Id).Any(x => x.UserId != member.Id) || MuteDatabase.GetActiveMutes(ctx.Guild.Id).Any(x => x.UserId != member.Id))
            {
                await ctx.RespondAsync(":no_mouth: User is not muted!");
                return;
            }
            
            var muteRole = ctx.Guild.GetRole(ServerSettingsDatabase.GetServerSettings(ctx.Guild.Id).Moderation.MuteRoleId);
            await member.RevokeRoleAsync(muteRole, $"Unmuted by {ctx.Member.DisplayName}#{ctx.Member.Discriminator}");
            try
            {
                MuteDatabase.DeleteMute(member.Id, ctx.Guild.Id, "mutes");
                MuteDatabase.DeleteMute(member.Id, ctx.Guild.Id);

                _ = Task.Run(() =>
                {
                    foreach (var v in WarnDatabase.GetUserWarn(member.Id, ctx.Guild.Id))
                    {
                        WarnDatabase.DeleteWarn(v.UserId, v.GuildId, v.ExpiresAt);
                    }
                });

            }
            catch
            {
                // ignored
            }
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":done:"));
        } 
        
        [Command("mutelist"), RequirePermissions(Permissions.ManageRoles)]
        public async Task MuteList(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.WithFooter($"ID : {ctx.Member.Id}")
                .WithTimestamp(DateTime.UtcNow)
                .Color = DiscordColor.Grayple;

            foreach (var v in MuteDatabase.GetMutes(ctx.Guild.Id).Where(v => v != null))
                embedBuilder.AddField($"User: {v.Username}", $"Reason {v.Reason}\nIssued at: {v.IssuedAt}");

            foreach (var v in MuteDatabase.GetActiveMutes(ctx.Guild.Id).Where(v => v != null))
                embedBuilder.AddField($"User: {v.Username}", $"Reason: {v.Reason}\nIssued at: {v.IssuedAt}(UTC) and expires at: {v.ExpiresAt}(UTC)\nDuration: {v.ExpiresAt - v.IssuedAt}, time left: {v.ExpiresAt - DateTime.UtcNow}");


            if (embedBuilder.Build().Fields.Count > 0)
                await ctx.RespondAsync(null, false, embedBuilder.Build());
            else
                await ctx.RespondAsync("No mutes have been found ! :ok_hand:");
        }
        [Command("warn"), Aliases("w"), RequirePermissions(Permissions.ManageChannels)]
        public async Task Warn(CommandContext ctx, DiscordMember member, [RemainingText] string reason = null)
        {
            _ = Task.Run(async () =>
            {
                if (!ctx.Member.CanPunish(member))
                {
                    await ctx.RespondAsync("You cannot warn someone that has higher roles than you!");
                    return;
                }
                
                var warn = new Warn
                {
                    UserId = member.Id,
                    GuildId = ctx.Guild.Id,
                    IssuedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss").ToDateTime(),
                    ExpiresAt = (DateTime.UtcNow + TimeSpan.FromDays(1)).ToString("yyyy-MM-dd HH:mm:ss").ToDateTime(),
                    Username = $"{member.Username}#{member.Discriminator}",
                    Reason = reason
                };
                WarnDatabase.InsertWarn(warn);

                if (reason == null)
                    await ctx.RespondAsync($"User {member.Mention} got warned! :warning:");
                else
                    await ctx.RespondAsync($"User {member.Mention} got warned for the following reason : **{reason}** ! :warning:");
                
                if (!member.IsBot)
                {
                    await member.SendMessageAsync(
                        $"You have been warned from **{ctx.Guild.Name}** for ```{reason ?? "No reason has been provided"}```");
                }

                if (WarnDatabase.GetUserWarn(member.Id, ctx.Guild.Id).Count >= 3 && ServerSettingsDatabase.GetServerSettings(ctx.Guild.Id).Moderation.MuteAfter3Warn)
                {
                    var v = Program.Commands.CreateFakeContext(ctx.Client.CurrentUser, ctx.Channel,
                        ctx.Message.Content, ";", Program.Commands.RegisteredCommands["tempmute"],
                        $"{member.Mention} 1d Warned 3 times in less than 24h > 24h Mute ! :no_mouth:");
                    await Program.Commands.ExecuteCommandAsync(v);
                }
            
                var settings = ServerSettingsDatabase.GetServerSettings(ctx.Guild.Id);

                if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogPunishments)
                {
                    var logEmbed = new DiscordEmbedBuilder
                        {
                            Color = DiscordColor.Goldenrod,
                            Description = $"Reason : {reason ?? "No reason provided"}"
                        }
                        .WithTitle($"{ctx.Member.Tag()} warned {member.Tag()}. :mute:")
                        .WithFooter($"Member ID : {ctx.Member.Id}")
                        .WithTimestamp(DateTime.Now);
                        
                    var channel = ctx.Guild.GetChannel(settings.Logging.ChannelId);
                    await channel.SendMessageAsync(embed: logEmbed);
                }
            });
        }

        [Command("warnlist")]
        public async Task WarnList(CommandContext ctx,  DiscordMember member)
        {
            var list = WarnDatabase.GetUserWarn(member.Id, ctx.Guild.Id);

            if (list.Count <= 0)
            {
                await ctx.RespondAsync("User don't have any warns! :ok_hand:");
                return;
            }

            var builder = new DiscordEmbedBuilder
                {
                    Title = $"Warns for {member.Username}#{member.Discriminator}", Color = DiscordColor.Grayple
                }
                .WithFooter("Wyrobot#7218")
                .WithTimestamp(DateTime.UtcNow);

            var i = 1;
            foreach (var v in list)
            {
                builder.AddField($"Warn n°{i}", $"Reason : `{v.Reason}`\nIssued at : {v.IssuedAt:yyyy-MM-dd HH:mm:ss} UTC Time");
                i++;
            }

            await ctx.RespondAsync(null, false, builder.Build());
        }
    }
}