using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using Wyrobot.Core.Database;
using static Wyrobot.Core.Models.ServerSettings;
#pragma warning disable 1998

// ReSharper disable UnusedMember.Global

namespace Wyrobot.Core.Commands
{
    [Group("config"), RequirePermissions(Permissions.Administrator)]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ServerSettingsCommands : BaseCommandModule
    {
        [Command("setup"), RequireUserPermissions(Permissions.Administrator)]
        public async Task Setup(CommandContext ctx)
        {
            _ = Task.Run(async () =>
            {
                var currentServerSettings = ServerSettingsDatabase.GetServerSettings(ctx.Guild.Id);

                await ctx.RespondAsync(
                    "This command will guide you through the setup of Wyrobot on your server, you can exit the setup at any moment by typing `cancel`, each command will have a time out of 1 minute. You can re-configure each element later.\nAre you ready to begin ? **Yes**|**No**");

                var result = await ctx.Message.GetNextMessageAsync();

                if (result.TimedOut)
                {
                    await ctx.RespondAsync(":x: Timed out! Exiting setup...");
                    return;
                }

                switch (result.Result.Content.ToLower())
                {
                    case "yes":
                        await ctx.RespondAsync(":white_check_mark: Cool, let's continue !");
                        break;

                    case "no":
                        await ctx.RespondAsync(
                            "Oh, okay. :warning: Please note that this highly recommended to run this command when you add Wyrobot to your server.");
                        return;

                    case "cancel":
                        await ctx.RespondAsync(
                            ":x: Cancelled! Exiting setup...  :warning: Please note that this highly recommended to run this command when you add Wyrobot to your server.");
                        return;

                    default:
                        await ctx.RespondAsync(
                            "You might have misspelled! I'll let you try again.\nDo you want to continue ? **Yes**|**No**");
                        var confirmation = await ctx.Message.GetNextMessageAsync();
                        if (confirmation.Result.Content.ToLower() != "yes") return;
                        await ctx.RespondAsync("Cool, let's continue !");
                        break;
                }

                #region Welcome setup

                await ctx.RespondAsync(
                    "Alright, do you want to welcome your new members with a custom message ? **Yes**|**No**");

                result = await ctx.Message.GetNextMessageAsync();

                if (result.TimedOut || result.Result.Content.ToLower() == "cancel")
                {
                    await ctx.RespondAsync(":x: Timed out or cancelled! Exiting setup...");
                    return;
                }

                if (result.Result.Content.ToLower().Contains("yes"))
                {
                    await ctx.RespondAsync(
                        "In which channel do you want to see the welcome message appear ? Use a channel mention.");

                    var welcomeR = await ctx.Message.GetNextMessageAsync();

                    if (welcomeR.TimedOut || welcomeR.Result.Content.ToLower() == "cancel")
                    {
                        await ctx.RespondAsync(":x: Timed out or cancelled! Exiting setup...");
                        return;
                    }

                    var channel = welcomeR.Result.MentionedChannels.First();

                    await ctx.RespondAsync(
                        $"Great, now enter the message that will be used to welcome your new members.\nCurrent message is : {currentServerSettings.Welcome.Message}.\nYou can use the argument `@user` to mention the new member and `@server` to represent the name of your server.");

                    var welcomeRr = await ctx.Message.GetNextMessageAsync();

                    if (welcomeRr.TimedOut || welcomeRr.Result.Content.ToLower() == "cancel")
                    {
                        await ctx.RespondAsync(":x: Timed out or cancelled! Exiting setup...");
                        return;
                    }

                    var welcomeMessage = welcomeRr.Result.Content;

                    await WelcomeSettings.UpdateWelcomeEnabled(ctx.Guild.Id, true);
                    await WelcomeSettings.UpdateChannelId(ctx.Guild.Id, channel.Id);
                    await WelcomeSettings.UpdateMessage(ctx.Guild.Id, welcomeMessage);
                }
                else
                {
                    await WelcomeSettings.UpdateWelcomeEnabled(ctx.Guild.Id, false);
                }

                #endregion

                #region Leveling setup

                await ctx.RespondAsync(
                    "Okay, now, do you want to enable leveling ? It's enabled by default. **Yes**|**No**");

                result = await ctx.Message.GetNextMessageAsync();

                if (result.TimedOut || result.Result.Content.ToLower() == "cancel")
                {
                    await ctx.RespondAsync(":x: Timed out or cancelled! Exiting setup...");
                    return;
                }

                if (result.Result.Content.ToLower().Contains("yes"))
                {
                    await ctx.RespondAsync(
                        "Great, now enter the message that will be used when a member level up." +
                        $"\nCurrent message is : {currentServerSettings.Leveling.Message}.\nYou can use the argument `@user` to mention the new member and `@level` to represent the level the member just reached." +
                        "Answer with `default` to use the current message.");

                    var levelingR = await ctx.Message.GetNextMessageAsync();

                    if (levelingR.TimedOut || levelingR.Result.Content.ToLower() == "cancel")
                    {
                        await ctx.RespondAsync(":x: Timed out or cancelled! Exiting setup...");
                        return;
                    }

                    var message = levelingR.Result.Content;

                    await LevelingSettings.UpdateLevelingEnabled(ctx.Guild.Id, true);
                    await LevelingSettings.UpdateMessage(ctx.Guild.Id, message);
                }
                else
                {
                    await LevelingSettings.UpdateLevelingEnabled(ctx.Guild.Id, false);
                }

                #endregion

                #region Logging setup

                await ctx.RespondAsync(
                    "Let's move on to logging, do you want to enable punishments logs ? **Yes**|**No**");

                result = await ctx.Message.GetNextMessageAsync();

                if (result.TimedOut || result.Result.Content.ToLower() == "cancel")
                {
                    await ctx.RespondAsync(":x: Timed out or cancelled! Exiting setup...");
                    return;
                }

                if (result.Result.Content.ToLower().Contains("yes"))
                {
                    await ctx.RespondAsync(
                        "In which channel do you want to see the logs ?\nUse a channel mention.");

                    var loggingR = await ctx.Message.GetNextMessageAsync();

                    if (loggingR.TimedOut || loggingR.Result.Content.ToLower() == "cancel")
                    {
                        await ctx.RespondAsync(":x: Timed out or cancelled! Exiting setup...");
                        return;
                    }

                    var channel = loggingR.Result.MentionedChannels.First();

                    await LoggingSettings.UpdateLoggingEnabled(ctx.Guild.Id, true);
                    await LoggingSettings.UpdateChannelId(ctx.Guild.Id, channel.Id);
                }
                else
                {
                    await LoggingSettings.UpdateLoggingEnabled(ctx.Guild.Id, false);
                }

                #endregion

                #region Auto-Moderation Setup

                await ctx.RespondAsync(
                    "What about auto-moderation, do you want to enable it ? **Yes**|**No**");

                result = await ctx.Message.GetNextMessageAsync();

                if (result.TimedOut || result.Result.Content.ToLower() == "cancel")
                {
                    await ctx.RespondAsync(":x: Timed out or cancelled! Exiting setup...");
                    return;
                }

                if (result.Result.Content.ToLower().Contains("yes"))
                {
                    /*
                    await ctx.RespondAsync(
                        "Do wanna set moderation roles. User with these roles will be immune to auto-moderation and will be able to mute and kick non-moderators, and ban users if they have permission to.\nUse role mentions. Answer with `none` to disable this.");

                    var moderationR = await ctx.Message.GetNextMessageAsync();

                    if (moderationR.TimedOut || moderationR.Result.Content.ToLower() == "cancel")
                    {
                        await ctx.RespondAsync(":x: Timed out or cancelled! Exiting setup...");
                        return;
                    }

                    var list = moderationR.Result.MentionedRoles.Select(v => v.Id).ToList();
                    */
                    await ctx.RespondAsync(
                        "Cool, do you want to set a list of banned words ? If a message contains one of them, it will delete it and warn the user. User with moderation roles are immune to this.\nUse words separated by a space. Answer with `none` to disable this functionality");

                    var moderationRr = await ctx.Message.GetNextMessageAsync();

                    if (moderationRr.TimedOut || moderationRr.Result.Content.ToLower() == "cancel")
                    {
                        await ctx.RespondAsync(":x: Timed out or cancelled! Exiting setup...");
                        return;
                    }

                    var bannedWords = moderationRr.Result.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (moderationRr.Result.Content == "none")
                        bannedWords = null;

                    await ctx.RespondAsync(
                        $"Alright, do you want to modify the caps percentage ? If a message contains more caps than the defined percentage, it will be deleted and the user will get warned. User with moderation roles are immune to this. Default to >70 %, current set to {currentServerSettings.Moderation.CapsPercentage} %.\nUse a number. Use 0 to disable this. Type `default` to use the current value.");

                    var moderationRrr = await ctx.Message.GetNextMessageAsync();

                    if (moderationRrr.TimedOut || moderationRrr.Result.Content.ToLower() == "cancel")
                    {
                        await ctx.RespondAsync(":x: Timed out or cancelled! Exiting setup...");
                        return;
                    }

                    if (!float.TryParse(moderationRrr.Result.Content, out var percentage))
                    {
                        if (moderationRrr.Result.Content.ToLower() == "default")
                            percentage = currentServerSettings.Moderation.CapsPercentage;
                    }

                    await ModerationSettings.UpdateBannedWords(ctx.Guild.Id, bannedWords);
                    await ModerationSettings.UpdateCapsPercentage(ctx.Guild.Id, percentage);
                    // await ServerSettings.ModerationSettings.UpdateModerationRoles(ctx.Guild.Id, list);
                    await ModerationSettings.UpdateAutoModEnabled(ctx.Guild.Id, true);
                }
                else
                {
                    await ModerationSettings.UpdateAutoModEnabled(ctx.Guild.Id, false);
                }
                
                #endregion

                #region Other Setup

                await ctx.RespondAsync(
                    "Do you want enabled lounges ? Lounges are temporary vocal channels that can be created using `;lounge`. **Yes**|**No**");

                result = await ctx.Message.GetNextMessageAsync();

                if (result.TimedOut || result.Result.Content.ToLower() == "cancel")
                {
                    await ctx.RespondAsync(":x: Timed out or cancelled! Exiting setup...");
                    return;
                }

                if (result.Result.Content.ToLower() == "yes")
                    await OtherSettings.UpdateLoungesEnabled(ctx.Guild.Id, true);
                else
                    await OtherSettings.UpdateLoungesEnabled(ctx.Guild.Id, false);
                
                #endregion
                    
                await ctx.RespondAsync(":tada: Setup is now done! Enjoy using the bot!");                
            });


        }

        [Command("leveling")]
        public async Task Leveling(CommandContext ctx, string parameter, [RemainingText] string value)
        {
            parameter = parameter.ToLower();
            if (parameter != "multiplier" && parameter != "enabled" && parameter != "message")
            {
                await ctx.RespondAsync(":x: Invalid command usage! Please use `;help config leveling`...");
                return;
            }

            switch (parameter)
            {
                case "multiplier":
                    await LevelingSettings.UpdateMultiplier(ctx.Guild.Id, value.ToInt32());
                    break;

                case "enabled":
                    await LevelingSettings.UpdateLevelingEnabled(ctx.Guild.Id, value.ToBoolean());
                    break;

                case "message":
                    await LevelingSettings.UpdateMessage(ctx.Guild.Id, value);
                    break;
            }

            await ctx.RespondAsync($"Setting `{parameter}` successfully updated to *{value}* !");
        }

        [Command("logging")]
        public async Task Logging(CommandContext ctx, string parameter, [RemainingText] string value)
        {
            parameter = parameter.ToLower();
            if (parameter != "channel" && parameter != "enabled")
            {
                await ctx.RespondAsync(":x: Invalid command usage! Please use `;help config logging`...");
                return;
            }

            switch (parameter)
            {
                case "enabled":
                    await LoggingSettings.UpdateLoggingEnabled(ctx.Guild.Id, value.ToBoolean());
                    break;

                case "channel":
                    await LoggingSettings.UpdateChannelId(ctx.Guild.Id,
                        ctx.Message.MentionedChannels.First().Id);
                    break;
            }

            await ctx.RespondAsync($"Setting `{parameter}` successfully updated to **{value}** !");
        }

        [Command("welcome")]
        public async Task Welcome(CommandContext ctx, string parameter, [RemainingText] string value)
        {
            parameter = parameter.ToLower();
            if (parameter != "channel" && parameter != "enabled" && parameter != "message")
            {
                await ctx.RespondAsync(":x: Invalid command usage! Please use `;help config welcome`...");
                return;
            }

            switch (parameter)
            {
                case "enabled":
                    await WelcomeSettings.UpdateWelcomeEnabled(ctx.Guild.Id, value.ToBoolean());
                    break;

                case "channel":
                    await WelcomeSettings.UpdateChannelId(ctx.Guild.Id,
                        ctx.Message.MentionedChannels.First().Id);
                    break;

                case "message":
                    await WelcomeSettings.UpdateMessage(ctx.Guild.Id, value);
                    break;
            }

            await ctx.RespondAsync($"Setting `{parameter}` successfully updated to *{value}* !");
        }

        [Command("moderation")]
        public async Task Moderation(CommandContext ctx, string parameter, [RemainingText] string value)
        {
            parameter = parameter.ToLower();
            if (parameter != "channel" && parameter != "enabled" && parameter != "bannedwords" &&
                parameter != "modroles")
            {
                await ctx.RespondAsync(":x: Invalid command usage! Please use `;help config moderation`...");
                return;
            }

            switch (parameter)
            {
                case "enabled":
                    await ModerationSettings.UpdateAutoModEnabled(ctx.Guild.Id, value.ToBoolean());
                    break;

                case "modroles":
                    await ModerationSettings.UpdateModerationRoles(ctx.Guild.Id,
                        ctx.Message.MentionedRoles.Select(v => v.Id).ToList());
                    break;

                case "bannedwords":
                    await ModerationSettings.UpdateBannedWords(ctx.Guild.Id,
                        value.Split(" ", StringSplitOptions.RemoveEmptyEntries));
                    break;
            }

            await ctx.RespondAsync($"Setting `{parameter}` successfully updated to *{value}* !");
        }

        [Command("other")]
        public async Task Other(CommandContext ctx, string parameter, [RemainingText] string value)
        {
            parameter = parameter.ToLower();
            if (parameter != "loungesenabled")
            {
                await ctx.RespondAsync(":x: Invalid command usage! Please use `;help config other`...");
                return;
            }

            switch (parameter)
            {
                case "loungesenabled":
                    await OtherSettings.UpdateLoungesEnabled(ctx.Guild.Id, value.ToBoolean());
                    break;
            }

            await ctx.RespondAsync($"Setting `{parameter}` successfully updated to *{value}* !");
        }
    }
}