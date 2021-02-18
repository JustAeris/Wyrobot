using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Wyrobot.Core.Database;
using Wyrobot.Core.Models;
using Action = Wyrobot.Core.Models.Action;
#pragma warning disable 1998

namespace Wyrobot.Core
{
    public static class ClientEvents
    {
        public static Task RegisterEvents(this DiscordClient discord, CommandsNextExtension commands)
        {
            discord.MessageCreated += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    if (eventArgs.Author.IsBot) return;

                    var member = (DiscordMember) eventArgs.Author;
                    var vPerms = member.PermissionsIn(eventArgs.Channel);
                    var mod = vPerms.HasPermission(Permissions.ManageMessages) 
                          || vPerms.HasPermission(Permissions.ManageGuild) 
                          || vPerms.HasPermission(Permissions.ManageRoles) 
                          || vPerms.HasPermission(Permissions.Administrator) 
                          || vPerms.HasPermission(Permissions.KickMembers) 
                          || vPerms.HasPermission(Permissions.BanMembers) 
                          || vPerms.HasPermission(Permissions.ManageChannels);
                    
                    var serverSettings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);
                    
                    if (member.Roles.Any(vRole => serverSettings.Moderation.ModerationRoles.Contains(vRole.Id)))
                        mod = true;
                    
                    float capsCount = 0;
                    foreach (var unused in eventArgs.Message.Content.Where(char.IsUpper))
                        capsCount++;

                    if (!mod &&
                        capsCount / eventArgs.Message.Content.Replace(" ", null).Length * 100 >
                        serverSettings.Moderation.CapsPercentage &&
                        eventArgs.Message.Content.Replace(" ", null).Length > 15 &&
                        serverSettings.Moderation.CapsPercentage != 0 && serverSettings.Moderation.AutoModerationEnabled)
                    {
                        await eventArgs.Channel.AddOverwriteAsync(member, Permissions.None, Permissions.SendMessages);
                        
                        await Task.Delay(2000);
                        var bulk = await eventArgs.Channel.GetMessagesAsync(10);
                        await eventArgs.Channel.DeleteMessagesAsync(bulk
                            .Where(x => x.Content == eventArgs.Message.Content)
                            .Where(x => x.Timestamp > DateTimeOffset.Now - TimeSpan.FromDays(14))
                            .Where(x => x.Author.Id == eventArgs.Author.Id));
                        var ctx = commands.CreateFakeContext(discord.CurrentUser, eventArgs.Channel,
                            eventArgs.Message.Content, ";", commands.RegisteredCommands["warn"],
                            $"{eventArgs.Author.Id} Excessive caps usage");
                        await commands.ExecuteCommandAsync(ctx);
                        await Task.Delay(5000);
                        await eventArgs.Channel.AddOverwriteAsync(member);
                        return;
                    }
                    
                    var bannedWords = serverSettings.Moderation.BannedWords;
                    var words = eventArgs.Message.Content.ToLower().Split(" ");


                    if (serverSettings.Moderation.AutoModerationEnabled)
                    {
                        switch (mod)
                        {
                            case false when words.Any(word => bannedWords.Any(badWord => word == badWord)):
                            {
                                await eventArgs.Channel.AddOverwriteAsync(member, Permissions.None, Permissions.SendMessages);
                            
                                await Task.Delay(2000);
                                var bulk = await eventArgs.Channel.GetMessagesAsync(10);
                                await eventArgs.Channel.DeleteMessagesAsync(bulk
                                    .Where(x => x.Content == eventArgs.Message.Content)
                                    .Where(x => x.Timestamp > DateTimeOffset.Now - TimeSpan.FromDays(14))
                                    .Where(x => x.Author.Id == eventArgs.Author.Id));
                                var ctx = commands.CreateFakeContext(discord.CurrentUser, eventArgs.Channel,
                                    eventArgs.Message.Content, ";", commands.RegisteredCommands["warn"],
                                    $"{eventArgs.Author.Id} Bad word usage");
                                await commands.ExecuteCommandAsync(ctx);
                                await Task.Delay(5000);
                                await eventArgs.Channel.AddOverwriteAsync(member);
                                return;
                            }
                            case false when Regex.Matches(eventArgs.Message.Content,
                                @"([0-9a-zA-Z&é""'(\-è_çà)=~#{[|`^@\]}^¨$£¤*ù%,?;./:§!]{1,})\1{9,}",
                                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace).Count > 0:
                            {
                                await eventArgs.Channel.AddOverwriteAsync(member, Permissions.None, Permissions.SendMessages);
                            
                                await Task.Delay(2000);
                                var bulk = await eventArgs.Channel.GetMessagesAsync(10);
                                await eventArgs.Channel.DeleteMessagesAsync(bulk
                                    .Where(x => x.Content == eventArgs.Message.Content)
                                    .Where(x => x.Timestamp > DateTimeOffset.Now - TimeSpan.FromDays(14))
                                    .Where(x => x.Author.Id == eventArgs.Author.Id));
                                var ctx = commands.CreateFakeContext(discord.CurrentUser, eventArgs.Channel,
                                    eventArgs.Message.Content, ";", commands.RegisteredCommands["warn"],
                                    $"{eventArgs.Author.Id} Spam");
                                await commands.ExecuteCommandAsync(ctx);
                                await Task.Delay(5000);
                                await eventArgs.Channel.AddOverwriteAsync(member);
                                return;
                            }
                        }
                        
                    }

                    if (serverSettings.Leveling.Enabled)
                    {
                        var userLevel = new UserLevel
                        {
                            GuildId = eventArgs.Guild.Id,
                            UserId = eventArgs.Author.Id,
                            // ReSharper disable once PossibleLossOfFraction
                            Xp = (eventArgs.Message.Content.Length / 2 * serverSettings.Leveling.Multiplier).ToInt32()
                        };
                        UserLevelDatabase.SetUserLevelInfoXp(userLevel, eventArgs.Channel);
                    }
                });
            };

            discord.GuildBanAdded += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    var ban = new Ban
                    {
                        ExpiresAt = DateTime.MaxValue,
                        GuildId = eventArgs.Guild.Id,
                        IssuedAt = DateTime.UtcNow,
                        Reason = "Manual ban",
                        Username = eventArgs.Member.Tag(),
                        UserId = eventArgs.Member.Id
                    };

                    BanDatabase.InsertBan(ban);
                    
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogPunishments)
                    {
                        var embed = new DiscordEmbedBuilder
                            {
                                Color = DiscordColor.Red,
                                Description = $"{eventArgs.Member.Mention}"
                            }
                            .WithTitle("A user has been banned. :lock:")
                            .WithFooter($"Member ID : {eventArgs.Member.Id}")
                            .WithTimestamp(DateTime.Now);
                        
                        var channel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                        await channel.SendMessageAsync(embed: embed);
                    }
                });
            };

            discord.GuildBanRemoved += async (sender, eventArgs) =>
            {
                BanDatabase.DeleteBan(eventArgs.Member.Id, eventArgs.Guild.Id, "bans");
                
                _ = Task.Run(async () =>
                {
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogPunishments)
                    {
                        var embed = new DiscordEmbedBuilder
                            {
                                Color = DiscordColor.IndianRed,
                                Description = $"{eventArgs.Member.Tag()}"
                            }
                            .WithTitle("A user has been unbanned. :unlock:")
                            .WithFooter($"Member ID : {eventArgs.Member.Id}")
                            .WithTimestamp(DateTime.Now);
                        
                        var channel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                        await channel.SendMessageAsync(embed: embed);
                    }
                });
            };

            discord.ChannelCreated += async (sender, eventArgs) =>
            {
                var discordRole = eventArgs.Guild.GetRole(ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id).Moderation.MuteRoleId);
                await eventArgs.Channel.AddOverwriteAsync(discordRole, Permissions.None, Permissions.SendMessages, "Automatically set permissions for \"Mute\" role.");
            };

            discord.GuildCreated += async (sender, eventArgs) =>
            {
                var b = false;
                foreach (var (_, value) in eventArgs.Guild.Roles)
                {
                    if (value.Name == "Wyrobot Mute")
                    {
                        b = true;
                    }
                }

                DiscordRole role = null;

                if (!b)
                {
                    role = await eventArgs.Guild.CreateRoleAsync("Wyrobot Mute", Permissions.None, DiscordColor.None,
                        false, false,
                        "Creating Mute role, renaming it or removing it can cause serious problems.");
                }
                else
                {
                    foreach (var (_, value) in eventArgs.Guild.Roles)
                    {
                        if (value.Name == "Wyrobot Mute")
                        {
                            role = value;
                        }
                    }
                }

                // ReSharper disable once PossibleNullReferenceException
                ServerSettingsDatabase.GenerateServerSettings(eventArgs.Guild.Id, role.Id == 0 ? 0 : role.Id, eventArgs.Guild.Name);
            };

            discord.GuildDeleted += async (sender, eventArgs) =>
            {
                ServerSettingsDatabase.DeleteServerSettings(eventArgs.Guild.Id);
            };

            discord.MessageReactionAdded += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    if (eventArgs.User.IsBot) return;

                    var reactionMessage =
                        ReactionMessageDatabase.GetReactionMessage(eventArgs.Guild.Id, eventArgs.Message.Id);

                    if (reactionMessage == null) return;

                    var guild = await discord.GetGuildAsync(reactionMessage.GuildId);
                    var channel = guild.GetChannel(reactionMessage.ChannelId);
                    var message = await channel.GetMessageAsync(reactionMessage.Id);

                    switch (reactionMessage.Action)
                    {
                        case Action.Grant:
                        {
                            var role = guild.GetRole(reactionMessage.Role);
                            var member = (DiscordMember) eventArgs.User;
                            try
                            {
                                await member.GrantRoleAsync(role);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }

                            break;
                        }
                        case Action.Revoke:
                        {
                            var role = guild.GetRole(reactionMessage.Role);
                            var member = (DiscordMember) eventArgs.User;
                            try
                            {
                                await member.RevokeRoleAsync(role);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }

                            break;
                        }
                        case Action.Suggestion:
                        {
                            if (eventArgs.Emoji == DiscordEmoji.FromName(discord, ":x:"))
                                await message.DeleteReactionAsync(DiscordEmoji.FromName(discord, ":white_check_mark:"),
                                    eventArgs.User, "Only one reaction per suggestion");

                            if (eventArgs.Emoji == DiscordEmoji.FromName(discord, ":white_check_mark:"))
                                await message.DeleteReactionAsync(DiscordEmoji.FromName(discord, ":x:"),
                                    eventArgs.User, "Only one reaction per suggestion");
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                });
            };

            discord.MessageReactionRemoved += async (sender, eventArgs) =>
            {
                if (eventArgs.User.IsBot) return;
                
                var reactionMessage = ReactionMessageDatabase.GetReactionMessage(eventArgs.Guild.Id, eventArgs.Message.Id);

                if (reactionMessage == null) return;
                
                var guild = await discord.GetGuildAsync(reactionMessage.GuildId);
                var role = guild.GetRole(reactionMessage.Role);

                switch (reactionMessage.Action)
                {
                    case Action.Grant:
                    {
                        var member = (DiscordMember) eventArgs.User;
                        try {await member.RevokeRoleAsync(role);}
                        catch { /* ignored */  }

                        break;
                    }
                    case Action.Revoke:
                    {
                        var member = (DiscordMember) eventArgs.User;
                        try {await member.GrantRoleAsync(role);}
                        catch { /* ignored */  }

                        break;
                    }
                    case Action.Suggestion:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };

            discord.MessageDeleted += async (sender, eventArgs) =>
            {
                _  = Task.Run( async () =>
                {
                    if (eventArgs.Message.Author == sender.CurrentUser) return;
                
                    var reactionMessage =
                        ReactionMessageDatabase.GetReactionMessage(eventArgs.Guild.Id, eventArgs.Message.Id);

                    if (reactionMessage != null)
                        ReactionMessageDatabase.DeleteReactionRoleMessage(eventArgs.Guild.Id, eventArgs.Message.Id);     
                    
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogMessages)
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = DiscordColor.Azure
                        };
                        embed.WithTitle(
                            $"{eventArgs.Message.Author.Username}#{eventArgs.Message.Author.Discriminator} deleted a message a message. :wastebasket:");
                        embed.WithThumbnail(eventArgs.Message.Author.AvatarUrl);
                        embed.WithTimestamp(DateTime.Now);
                        embed.WithFooter($"ID : {eventArgs.Message.Author.Id}");
                        embed.AddField("Content of the deleted message", eventArgs.Message.Content);
                        var channel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                        await channel.SendMessageAsync(embed: embed);
                    }
                });

            };

            discord.MessageUpdated += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    if (eventArgs.Message.Author == sender.CurrentUser) return;
                    
                    if (eventArgs.MessageBefore == eventArgs.Message) return;
                    
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogMessages)
                    {
                        var embed = new DiscordEmbedBuilder
                            {
                                Color = DiscordColor.Azure,
                                Description = $"[Click to see the message]({eventArgs.Message.JumpLink})"
                            };
                            embed.WithTitle(
                                $"{eventArgs.Author.Username}#{eventArgs.Author.Discriminator} edited a message. :pencil:");
                            embed.WithThumbnail(eventArgs.Author.AvatarUrl);
                            embed.WithTimestamp(DateTime.Now);
                            embed.WithFooter($"ID : {eventArgs.Author.Id}");
                            embed.AddField("Message before", eventArgs.MessageBefore.Content);
                            embed.AddField("New message", eventArgs.Message.Content);
                        var channel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                        await channel.SendMessageAsync(embed: embed);
                    }
                });
            };

            discord.InviteCreated += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogInvites)
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = DiscordColor.CornflowerBlue,
                            Description = $"{eventArgs.Invite}"
                        };
                        embed.WithTitle(
                            $"{eventArgs.Invite.Inviter.Username}#{eventArgs.Invite.Inviter.Discriminator} created an invite. :link:");
                        embed.WithThumbnail(eventArgs.Invite.Inviter.AvatarUrl);
                        embed.WithTimestamp(DateTime.Now);
                        embed.WithFooter($"ID : {eventArgs.Invite.Inviter.Id}");
                        embed.AddField("Created at", eventArgs.Invite.CreatedAt.ToString(), true);
                        embed.AddField("Expires in", Math.Round(TimeSpan.FromSeconds(eventArgs.Invite.MaxAge).TotalDays, 2).ToString(CultureInfo.InvariantCulture) + " days", true);
                        embed.AddField("Max uses",
                            eventArgs.Invite.MaxUses == 0 ? "∞" : eventArgs.Invite.MaxUses.ToString(), true);

                        var channel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                        await channel.SendMessageAsync(embed: embed);
                    }
                });
            };

            discord.InviteDeleted += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogInvites)
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = DiscordColor.CornflowerBlue,
                            Description = ""
                        };
                        embed.WithTitle(
                            "Someone deleted an invite. :wastebasket:");
                        embed.WithThumbnail(sender.CurrentUser.AvatarUrl);
                        embed.WithTimestamp(DateTime.Now);
                        embed.WithFooter($"ID : {eventArgs.Invite.Inviter.Id}");
                        embed.AddField("Was created at", eventArgs.Invite.CreatedAt.ToString(), true);
                        embed.AddField("Expired in", Math.Round(TimeSpan.FromSeconds(eventArgs.Invite.MaxAge).TotalDays, 2).ToString(CultureInfo.InvariantCulture) + " days", true);
                        embed.AddField("Max uses",
                            eventArgs.Invite.MaxUses == 0 ? "∞" : eventArgs.Invite.MaxUses.ToString(), true);

                        var channel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                        await channel.SendMessageAsync(embed: embed);
                    }
                });
            };

            discord.VoiceStateUpdated += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogVoiceState)
                    {
                        var action = eventArgs.GetVoiceStateAction();

                        var embed = new DiscordEmbedBuilder
                        {
                            Color = DiscordColor.SpringGreen
                        }
                            .WithFooter($"ID : {eventArgs.User.Id}")
                            .WithTimestamp(DateTime.Now)
                            .WithThumbnail(eventArgs.User.AvatarUrl);

                        if (eventArgs.Guild.Id == 747884856024760401 && (eventArgs.After.Channel.Id == 789130017983823944 || eventArgs.After.Channel.Name == "Moove -->"))
                        {
                            var pingChannel = eventArgs.Guild.GetChannel(788485840409591830);
                            await pingChannel.SendMessageAsync(
                                $"{eventArgs.Guild.Owner.Mention}, {eventArgs.User.Mention} souhaite être mouve!");
                        }
                        
                        switch (action)
                        {
                            case Extensions.VoiceStateAction.Joined:
                                embed.WithTitle($"{eventArgs.User.Username}#{eventArgs.User.Discriminator} joined a vocal channel. :inbox_tray:");
                                embed.AddField("Channel joined", eventArgs.After.Channel.Name);
                                break;
                            case Extensions.VoiceStateAction.Left:
                                embed.WithTitle($"{eventArgs.User.Username}#{eventArgs.User.Discriminator} left a vocal channel. :outbox_tray:");
                                embed.AddField("Channel left", eventArgs.Before.Channel.Name);
                                break;
                            case Extensions.VoiceStateAction.Unknown:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        var channel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                        await channel.SendMessageAsync(embed: embed);
                    }
                });
            };

            discord.ChannelCreated += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogChannels)
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = DiscordColor.Purple
                        }
                            .WithTitle("A new channel has been created. :new:")
                            .WithFooter($"Channel ID : {eventArgs.Channel.Id}")
                            .WithTimestamp(DateTime.Now)
                            .AddField("Name", eventArgs.Channel.Name, true)
                            .AddField("Type", eventArgs.Channel.Type.ToString(), true)
                            .AddField("Category", eventArgs.Channel.Parent == null ? "None" : eventArgs.Channel.Parent.Name, true);

                        var channel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                        await channel.SendMessageAsync(embed: embed);
                    }
                });
            };
            
            discord.ChannelDeleted += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogChannels)
                    {
                        try
                        {
                            var embed = new DiscordEmbedBuilder
                                {
                                    Color = DiscordColor.Purple
                                }
                                .WithTitle("A channel has been deleted. :x:")
                                .WithFooter($"Channel ID : {eventArgs.Channel.Id}")
                                .WithTimestamp(DateTime.Now)
                                .AddField("Name", eventArgs.Channel.Name, true)
                                .AddField("Type", eventArgs.Channel.Type.ToString(), true)
                                .AddField("Category", eventArgs.Channel.Parent == null ? "None" : eventArgs.Channel.Parent.Name, true)
                                .AddField("Topic", string.IsNullOrWhiteSpace(eventArgs.Channel.Topic) ? "None" : eventArgs.Channel.Topic, true)
                                .AddField("Is NSFW ?", eventArgs.Channel.IsNSFW.ToString().Capitalize(), true);

                            var channel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                            await channel.SendMessageAsync(embed: embed);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }

                    }
                });
            };
            
            discord.ChannelUpdated += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogChannels)
                    {
                        try
                        {
                            var embed = new DiscordEmbedBuilder
                            {
                                Color = DiscordColor.Purple,
                                Description = $"{eventArgs.ChannelAfter.Mention}\nBelow are the new and old values"
                            }
                                .WithTitle("A channel has been updated. :pencil2:")
                                .WithFooter($"Channel ID : {eventArgs.ChannelAfter.Id}")
                                .WithTimestamp(DateTime.Now);
                            //if (eventArgs.ChannelBefore != eventArgs.ChannelAfter) return;

                            if (eventArgs.ChannelBefore.Name != eventArgs.ChannelAfter.Name)
                            {
                                embed.AddField("Name",
                                    $"Old : {eventArgs.ChannelBefore.Name}\nNew : {eventArgs.ChannelAfter.Name}");
                            }
                            
                            if (eventArgs.ChannelBefore.Topic != eventArgs.ChannelAfter.Topic)
                            {
                                embed.AddField("Topic",
                                    $"Old : {eventArgs.ChannelBefore.Topic}\nNew : {eventArgs.ChannelAfter.Topic}");
                            }
                            
                            if (eventArgs.ChannelBefore.IsNSFW != eventArgs.ChannelAfter.IsNSFW)
                            {
                                embed.AddField("Is NSFW ?",
                                    $"Old : {eventArgs.ChannelBefore.IsNSFW.ToString().Capitalize()}\nNew : {eventArgs.ChannelAfter.IsNSFW.ToString().Capitalize()}");
                            }
                            
                            if (eventArgs.ChannelBefore.Bitrate != eventArgs.ChannelAfter.Bitrate)
                            {
                                embed.AddField("Bitrate in kbps",
                                    $"Old : {eventArgs.ChannelBefore.Bitrate}\nNew : {eventArgs.ChannelAfter.Bitrate}");
                            }
                            
                            if (eventArgs.ChannelBefore.PerUserRateLimit != eventArgs.ChannelAfter.PerUserRateLimit)
                            {
                                if (eventArgs.ChannelBefore.PerUserRateLimit != null && eventArgs.ChannelAfter.PerUserRateLimit != null)
                                    embed.AddField("Slowmode",
                                        $"Old : {TimeSpan.FromSeconds((double) eventArgs.ChannelBefore.PerUserRateLimit)}\nNew : {TimeSpan.FromSeconds((double) eventArgs.ChannelAfter.PerUserRateLimit)}");
                            }
                            
                            if (eventArgs.ChannelBefore.UserLimit != eventArgs.ChannelAfter.UserLimit)
                            {
                                embed.AddField("User limit",
                                    $"Old : {eventArgs.ChannelBefore.UserLimit}\nNew : {eventArgs.ChannelAfter.UserLimit}");
                            }

                            var channel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                            await channel.SendMessageAsync(embed: embed);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                });
            };

            discord.UserUpdated += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    DiscordMember member = null;
                    foreach (var vGuild in sender.Guilds)
                    {
                        try
                        {
                            member = await vGuild.Value.GetMemberAsync(eventArgs.UserAfter.Id);
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    if (member is { })
                    {
                        var settings = ServerSettingsDatabase.GetServerSettings(member.Guild.Id);

                        if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogUsers)
                        {
                            try
                            {
                                var embed = new DiscordEmbedBuilder
                                    {
                                        Color = DiscordColor.Brown,
                                        Description = $"{eventArgs.UserAfter.Mention}\nBelow are the new and old values"
                                    }
                                    .WithTitle($"{member.Tag()} updated his profile. :pencil2:")
                                    .WithFooter($"Channel ID : {eventArgs.UserAfter.Id}")
                                    .WithTimestamp(DateTime.Now)
                                    .WithThumbnail(eventArgs.UserAfter.AvatarUrl);
                                if (eventArgs.UserBefore != eventArgs.UserAfter) return;

                                if (eventArgs.UserBefore.Username != eventArgs.UserAfter.Username)
                                {
                                    embed.AddField("Name",
                                        $"Old : {eventArgs.UserBefore.Username}\nNew : {eventArgs.UserAfter.Username}");
                                }
                            
                                if (eventArgs.UserBefore.Discriminator != eventArgs.UserAfter.Discriminator)
                                {
                                    embed.AddField("Type",
                                        $"Old : #{eventArgs.UserBefore.Discriminator}\nNew : #{eventArgs.UserAfter.Discriminator}");
                                }

                                if (eventArgs.UserBefore.AvatarUrl != eventArgs.UserAfter.AvatarUrl)
                                {
                                    embed.AddField("Topic",
                                        $"Old : {eventArgs.UserBefore.AvatarUrl}\nNew : {eventArgs.UserAfter.AvatarUrl}");
                                }

                                var channel = member.Guild.GetChannel(settings.Logging.ChannelId);
                                await channel.SendMessageAsync(embed: embed);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }
                        }
                    }
                });
            };

            discord.GuildRoleCreated += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogRoles)
                    {
                        var embed = new DiscordEmbedBuilder
                            {
                                Color = DiscordColor.Wheat
                            }
                            .WithTitle("A new role has been created. :new:")
                            .WithFooter($"Role ID : {eventArgs.Role.Id}")
                            .WithTimestamp(DateTime.Now)
                            .AddField("Created at", eventArgs.Role.CreationTimestamp.ToString());

                        var channel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                        await channel.SendMessageAsync(embed: embed);
                    }
                });
            };
            
            discord.GuildRoleUpdated += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogRoles)
                    {
                        try
                        {
                            var embed = new DiscordEmbedBuilder
                            {
                                Color = DiscordColor.Wheat,
                                Description = $"{eventArgs.RoleAfter.Mention}\nBelow are the new and old values"
                            }
                                .WithTitle("A role has been updated. :pencil2:")
                                .WithFooter($"Channel ID : {eventArgs.RoleAfter.Id}")
                                .WithTimestamp(DateTime.Now);

                            if (eventArgs.RoleBefore.Name != eventArgs.RoleAfter.Name)
                            {
                                embed.AddField("Name",
                                    $"Old : {eventArgs.RoleBefore.Name}\nNew : {eventArgs.RoleAfter.Mention}");
                            }
                            
                            if (eventArgs.RoleBefore.Color.ToString() != eventArgs.RoleAfter.Color.ToString())
                            {
                                embed.AddField("Color",
                                    $"Old : {eventArgs.RoleBefore.Color}\nNew : {eventArgs.RoleAfter.Color}");
                            }
                            
                            if (eventArgs.RoleBefore.Permissions != eventArgs.RoleAfter.Permissions)
                            {
                                embed.AddField("Permissions", 
                                    $"Old : {eventArgs.RoleBefore.Permissions.ToPermissionString()}\nNew : {eventArgs.RoleAfter.Permissions.ToPermissionString() ?? "None"}");
                            }
                            
                            if (eventArgs.RoleBefore.IsMentionable != eventArgs.RoleAfter.IsMentionable)
                            {
                                embed.AddField("Is mentionable ?",
                                    $"Old : {eventArgs.RoleBefore.IsMentionable.ToString().Capitalize()}\nNew : {eventArgs.RoleAfter.ToString().Capitalize()}");
                            }
                            
                            if (eventArgs.RoleBefore.Position != eventArgs.RoleAfter.Position)
                            {
                                return;
                            }
                            
                            var channel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                            await channel.SendMessageAsync(embed: embed);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                });
            };
            
            discord.GuildRoleDeleted += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    AutoRoleDatabase.DeleteRole(eventArgs.Guild.Id, eventArgs.Role.Id);
                    LevelRewardsDatabase.DeleteRole(eventArgs.Guild.Id, eventArgs.Role.Id);
                    
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogRoles)
                    {
                        try
                        {
                            var embed = new DiscordEmbedBuilder
                                {
                                    Color = DiscordColor.Wheat
                                }
                                .WithTitle("A role has been deleted. :x:")
                                .WithFooter($"Channel ID : {eventArgs.Role.Id}")
                                .WithTimestamp(DateTime.Now)
                                .AddField("Name", eventArgs.Role.Name, true)
                                .AddField("Color", eventArgs.Role.Color.ToString(), true)
                                .AddField("Permissions", eventArgs.Role.Permissions.ToPermissionString(), true)
                                .AddField("Is mentionable ?", eventArgs.Role.IsMentionable.ToString().Capitalize(), true)
                                .AddField("Position", eventArgs.Role.Position.ToString(), true);

                            var channel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                            await channel.SendMessageAsync(embed: embed);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }

                    }
                });
            };

            discord.GuildUpdated += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.GuildAfter.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogServer)
                    {
                        try
                        {
                            var embed = new DiscordEmbedBuilder
                            {
                                Color = DiscordColor.Lilac,
                                Description = $"`{eventArgs.GuildAfter.Name}`\nBelow are the new and old values"
                            }
                                .WithTitle("The guild has been updated. :pencil2:")
                                .WithFooter($"Guild ID : {eventArgs.GuildAfter.Id}")
                                .WithTimestamp(DateTime.Now);

                            if (eventArgs.GuildBefore.Name != eventArgs.GuildAfter.Name)
                            {
                                embed.AddField("Name",
                                    $"Old : {eventArgs.GuildBefore.Name}\nNew : {eventArgs.GuildAfter.Name}", true);
                            }
                            
                            if (eventArgs.GuildBefore.Description != eventArgs.GuildAfter.Description)
                            {
                                embed.AddField("Description",
                                    $"Old : {eventArgs.GuildBefore.Description}\nNew : {eventArgs.GuildAfter.Description}", true);
                            }
                            
                            if (eventArgs.GuildBefore.Emojis.Count != eventArgs.GuildAfter.Emojis.Count)
                            {
                                var s = eventArgs.GuildBefore.Emojis.Aggregate("", (current, v) => current + v.Value.GetDiscordName() + " ");
                                var ss = eventArgs.GuildBefore.Emojis.Aggregate("", (current, v) => current + v.Value.GetDiscordName() + " ");
                                embed.AddField("Emojis", $"Old : {s}\nNew : {ss}", true);
                            }
                            
                            if (eventArgs.GuildBefore.PreferredLocale != eventArgs.GuildAfter.PreferredLocale)
                            {
                                embed.AddField("Preferred language",
                                    $"Old : {eventArgs.GuildBefore.PreferredLocale.Capitalize()}\nNew : {eventArgs.GuildAfter.PreferredLocale.Capitalize()}", true);
                            }
                            
                            if (eventArgs.GuildBefore.Owner != eventArgs.GuildAfter.Owner)
                            {
                                embed.AddField("Owner",
                                    $"Old : {eventArgs.GuildBefore.Owner.Tag()}\nNew : {eventArgs.GuildAfter.Owner.Tag()}", true);
                            }
                            
                            if (eventArgs.GuildBefore.AfkChannel != eventArgs.GuildAfter.AfkChannel)
                            {
                                embed.AddField("AFK Channel",
                                    $"Old : {eventArgs.GuildBefore.AfkChannel.Name ?? "None"}\nNew : {eventArgs.GuildAfter.AfkChannel.Name ?? "None"}", true);
                            }
                            
                            if (eventArgs.GuildBefore.AfkTimeout != eventArgs.GuildAfter.AfkTimeout)
                            {
                                embed.AddField("AFK Timeout",
                                    $"Old : {TimeSpan.FromSeconds(eventArgs.GuildBefore.AfkTimeout)}\nNew : {TimeSpan.FromSeconds(eventArgs.GuildAfter.AfkTimeout)}", true);
                            }
                            
                            if (eventArgs.GuildBefore.Banner != eventArgs.GuildAfter.Banner)
                            {
                                embed.AddField("Banner",
                                    $"New : {eventArgs.GuildAfter.BannerUrl ?? "None"}", true);
                            }
                            
                            if (eventArgs.GuildBefore.IconHash != eventArgs.GuildAfter.IconHash)
                            {
                                embed.AddField("Icon",
                                    $"New : {eventArgs.GuildAfter.IconUrl ?? "None"}", true);
                            }
                            
                            if (eventArgs.GuildBefore.IsLarge != eventArgs.GuildAfter.IsLarge)
                            {
                                embed.AddField("Is large ?",
                                    $"Old : {eventArgs.GuildBefore.IsLarge.ToString().Capitalize()}\nNew : {eventArgs.GuildAfter.IsLarge.ToString().Capitalize()}", true);
                            }
                            
                            if (eventArgs.GuildBefore.PremiumTier != eventArgs.GuildAfter.PremiumTier)
                            {
                                embed.AddField("Premium tier",
                                    $"Old : {eventArgs.GuildBefore.PremiumTier.ToString().Replace("_", " ")}\nNew : {eventArgs.GuildAfter.PremiumTier.ToString().Replace("_", " ")}", true);
                            }
                            
                            if (eventArgs.GuildBefore.RulesChannel != eventArgs.GuildAfter.RulesChannel)
                            {
                                embed.AddField("Rules Channel",
                                    $"Old : {eventArgs.GuildBefore.RulesChannel.Name ?? "None"}\nNew : {eventArgs.GuildAfter.RulesChannel.Mention ?? "None"}", true);
                            }
                            
                            if (eventArgs.GuildBefore.SplashHash != eventArgs.GuildAfter.SplashHash)
                            {
                                embed.AddField("Splash",
                                    $"New : {eventArgs.GuildAfter.SplashUrl ?? "None"}", true);
                            }
                            
                            if (eventArgs.GuildBefore.SystemChannel != eventArgs.GuildAfter.SystemChannel)
                            {
                                embed.AddField("System Channel",
                                    $"Old : {eventArgs.GuildBefore.SystemChannel.Name ?? "None"}\nNew : {eventArgs.GuildAfter.SystemChannel.Mention ?? "None"}", true);
                            }
                            
                            if (eventArgs.GuildBefore.VerificationLevel != eventArgs.GuildAfter.VerificationLevel)
                            {
                                embed.AddField("Verification level",
                                    $"Old : {eventArgs.GuildBefore.VerificationLevel.ToString()}\nNew : {eventArgs.GuildAfter.VerificationLevel.ToString()}", true);
                            }
                            
                            if (eventArgs.GuildBefore.DefaultMessageNotifications != eventArgs.GuildAfter.DefaultMessageNotifications)
                            {
                                embed.AddField("Default messages",
                                    $"Old : {eventArgs.GuildBefore.DefaultMessageNotifications.ToString()}\nNew : {eventArgs.GuildAfter.DefaultMessageNotifications.ToString()}", true);
                            }
                            
                            if (eventArgs.GuildBefore.ExplicitContentFilter != eventArgs.GuildAfter.ExplicitContentFilter)
                            {
                                embed.AddField("Explicit content filter",
                                    $"Old : {eventArgs.GuildBefore.ExplicitContentFilter.ToString()}\nNew : {eventArgs.GuildAfter.ExplicitContentFilter.ToString()}", true);
                            }
                            
                            if (eventArgs.GuildBefore.PremiumSubscriptionCount.HasValue && eventArgs.GuildAfter.PremiumSubscriptionCount.HasValue)
                            {
                                if (eventArgs.GuildBefore.PremiumSubscriptionCount.Value != eventArgs.GuildAfter.PremiumSubscriptionCount.Value)
                                {
                                    embed.AddField("Premium subscription count",
                                        $"Old : {eventArgs.GuildBefore.PremiumSubscriptionCount.ToString() ?? "0"}\nNew : {eventArgs.GuildAfter.PremiumSubscriptionCount.ToString() ?? "0"}", true);

                                }
                            }
                            
                            if (eventArgs.GuildBefore.VanityUrlCode != eventArgs.GuildAfter.VanityUrlCode)
                            {
                                embed.AddField("Vanity url code",
                                    $"Old : {eventArgs.GuildBefore.VanityUrlCode?? "None"}\nNew : {eventArgs.GuildAfter.VanityUrlCode ?? "None"}", true);
                            }
                            
                            if (eventArgs.GuildBefore.MaxVideoChannelUsers != eventArgs.GuildAfter.MaxVideoChannelUsers)
                            {
                                embed.AddField("Vanity url code",
                                    $"Old : {eventArgs.GuildBefore.MaxVideoChannelUsers.ToString() ?? "0"}\nNew : {eventArgs.GuildAfter.MaxVideoChannelUsers.ToString() ?? "0"}", true);
                            }

                            var channel = eventArgs.GuildAfter.GetChannel(settings.Logging.ChannelId);
                            await channel.SendMessageAsync(embed: embed);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                });
            };

            discord.GuildEmojisUpdated += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogServer)
                    {
                        try
                        {
                            var embed = new DiscordEmbedBuilder
                            {
                                Color = DiscordColor.Magenta
                            }
                                .WithTitle("Emojis have been updated. :pencil2:")
                                .WithFooter($"Guild ID : {eventArgs.Guild.Id}")
                                .WithTimestamp(DateTime.Now);

                            if (eventArgs.EmojisBefore.Count != eventArgs.EmojisAfter.Count)
                            {
                                string added = " ", removed = " ";
                                foreach (var vEmoji in eventArgs.EmojisAfter.Values.Except(eventArgs.EmojisBefore.Values).ToList())
                                {
                                    added += vEmoji + " ";
                                }

                                foreach (var vEmoji in eventArgs.EmojisBefore.Values.Except(eventArgs.EmojisAfter.Values).ToList())
                                {
                                    removed += vEmoji.GetDiscordName()+ " ";
                                }

                                if (added != " ")
                                {
                                    embed.AddField("Added", added, true);
                                }
                                if (removed != " ")
                                {
                                    embed.AddField("Removed", removed, true);
                                }

                                var channel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                                await channel.SendMessageAsync(embed: embed);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                });
            };

            discord.GuildMemberAdded += async (sender, eventArgs) =>
            {
                _ = Task.Run(() =>
                {
                    var list = AutoRoleDatabase.GetAutoRoles(eventArgs.Guild.Id);

                    foreach (var role in list.Select(vRole => eventArgs.Guild.GetRole(vRole.RoleId)))
                    {
                        eventArgs.Member.GrantRoleAsync(role, "Auto-role");
                    }
                    
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Welcome.Enabled)
                    {
                        var channel = eventArgs.Guild.GetChannel(settings.Welcome.ChannelId);
                        channel.SendMessageAsync(settings.Welcome.Message.Replace("@user", eventArgs.Member.Mention).Replace("@server", eventArgs.Guild.Name));
                    }
                    
                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogUsers)
                    {
                        try
                        {
                            var embed = new DiscordEmbedBuilder
                                {
                                    Color = DiscordColor.Goldenrod
                                }
                                .WithTitle($"{eventArgs.Member.Tag()} joined. :clap:")
                                .WithFooter($"Member ID : {eventArgs.Member.Id}")
                                .WithTimestamp(DateTime.Now);
                            
                            var logChannel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                            logChannel.SendMessageAsync(embed: embed);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                });
            };

            discord.GuildMemberRemoved += async (sender, eventArgs) =>
            {
                _ = Task.Run(async () =>
                {
                    var settings = ServerSettingsDatabase.GetServerSettings(eventArgs.Guild.Id);

                    if (settings.Logging.Enabled && settings.Logging.ChannelId != 0 && settings.Logging.LogUsers)
                    {
                        try
                        {
                            var embed = new DiscordEmbedBuilder
                            {
                                Color = DiscordColor.Goldenrod
                            }
                                .WithTitle($"{eventArgs.Member.Tag()} left. :door:")
                                .WithFooter($"Member ID : {eventArgs.Member.Id}")
                                .WithTimestamp(DateTime.Now);
                            
                            var channel = eventArgs.Guild.GetChannel(settings.Logging.ChannelId);
                            await channel.SendMessageAsync(embed: embed);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                });
            };

            return Task.CompletedTask;
        }
    }
}