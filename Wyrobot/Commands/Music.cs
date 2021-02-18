using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

// ReSharper disable UnusedMember.Global
// ReSharper disable HeuristicUnreachableCode

namespace Wyrobot.Core.Commands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MusicCommands : BaseCommandModule
    {
        private Queue<LavalinkTrack> _playlist = new Queue<LavalinkTrack>();
        private readonly MusicChannel _musicChannel = new MusicChannel();

        [Command("join"), Description("Join the channel you're in. Requires to be in a voice channel.")]
        public async Task Join(CommandContext ctx)
        {
            var channel = ctx.Member.VoiceState.Channel;

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            await node.ConnectAsync(channel);
            
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":done:"));
        }

        [Command("leave"), Aliases("disconnect"), Description("Leave the channel you're in. Requires to be in a voice channel.")]
        public async Task Leave(CommandContext ctx)
        {
            var channel = ctx.Member.VoiceState.Channel;
            
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }
            
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            var conn = node.GetGuildConnection(channel.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            await conn.DisconnectAsync();
            
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":done:"));
        }
        
        [Command("play"), Aliases("enqueue", "p"), Description("Plays a music. It can be a search on YouTube or a link to a YouTube video. Requires to be in a voice channel.")]
        public async Task Play(CommandContext ctx, [RemainingText, Description("Searches a video on YouTube, or you can use a YouTube link.")] string search)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }
            
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                var channel = ctx.Member.VoiceState.Channel;

                if (channel.Type != ChannelType.Voice)
                {
                    await ctx.RespondAsync("Not a valid voice channel.");
                    return;
                }
                await node.ConnectAsync(channel);
                await ctx.RespondAsync($"Joined {channel.Name}!");
            }
            
            conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            
            conn.PlaybackFinished += ConnOnPlaybackFinished;

            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed 
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }

            var track = loadResult.Tracks.First();

            if (conn.CurrentState.CurrentTrack == null)
            {
                await conn.PlayAsync(track);
                await ctx.RespondAsync($"Now playing {track.Title}! ({track.Uri})");
            }
            else
            {
                _playlist.Enqueue(track);
                _musicChannel.Channel = ctx.Channel;
                await ctx.RespondAsync($"Enqueued {track.Title}!");
            }
        }
        
        [Command("pause"), Aliases("poz"), Description("Pauses the current track. Requires to be in a voice channel.")]
        public async Task Pause(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.PauseAsync();
            
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":done:"));
        }
        
        [Command("resume"), Description("Resumes the current track. Requires to be in a voice channel.")]
        public async Task Resume(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.ResumeAsync();
            
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":done:"));
        }
        
        [Command("stop"), Description("Stops the current track and disconnects from the voice channel. Requires to be in a voice channel.")]
        public async Task Stop(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.StopAsync();
            await conn.DisconnectAsync();
            
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":done:"));
        }
        
        [Command("seek"), Description("Sees to the given position. Requires to be in a voice channel.")]
        public async Task Seek(CommandContext ctx, TimeSpan position)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.SeekAsync(position);
            
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":done:"));
        }

        [Command("queue"), Aliases("list", "q"), Description("Shows the current queue for this server.")]
        public async Task Queue(CommandContext ctx, [Description("Page to show. Defaults to 1.")] int page = 1)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            var track = conn.CurrentState.CurrentTrack;
            
            var altPlaylist = _playlist;
            
            var length = altPlaylist.Count;

            var pageNumber = length;
            
            while (pageNumber % 10 != 0)
                pageNumber++;

            pageNumber /= 10;
            
            if (page > pageNumber)
            {
                await ctx.RespondAsync(
                    $"Now playing, **{track.Title}**, position: {conn.CurrentState.PlaybackPosition:h':'mm':'ss}/{track.Length:h':'mm':'ss}");
                return;
            }

            for (var j = 0; j < (page - 1) * 10; j++)
            {
                altPlaylist.Dequeue();
            }

            var embedBuilder = new DiscordEmbedBuilder {Color = DiscordColor.Grayple};
            embedBuilder.WithTitle("Music queue");
            embedBuilder.WithFooter($"Page {page}/{pageNumber}\nWyrobot#7218");

            embedBuilder.AddField($"Now playing. {track.Title}", $"From: {track.Author}, duration: {conn.CurrentState.PlaybackPosition}/{track.Length}", true);
            await ctx.RespondAsync(null, false, embedBuilder.Build());
            embedBuilder.ClearFields();
            
            var place = (page - 1) * 10 + 1;
            foreach (var v in altPlaylist.Take(10))
            {
                embedBuilder.AddField($"{place}. {v.Title}", $"From: {v.Author}, duration: {v.Length}");
                place++;
            }
            
            await ctx.RespondAsync(null, false, embedBuilder.Build());
        }

        [Command("skip"), Description("Skips the current track. Requires to be in a voice channel."), RequirePermissions(Permissions.ManageMessages)]
        public async Task Skip(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            conn.PlaybackFinished += ConnOnPlaybackFinished;
            await conn.StopAsync();
            
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":done:"));
        }

        [Command("nowplaying"), Aliases("np"), Description("Shows the track that is currently playing.")]
        public async Task NowPlaying(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }
            
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            var track = conn.CurrentState.CurrentTrack;
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await ctx.RespondAsync(
                $"Now playing, **{track.Title}**, position: {conn.CurrentState.PlaybackPosition:h':'mm':'ss}/{track.Length:h':'mm':'ss}");
        }

        [Command("cancel"), Aliases("removelast", "dellast"), Description("Removes the last song added.")]
        public async Task Cancel(CommandContext ctx)
        {
            if (!_playlist.Any()) return;

            _playlist = new Queue<LavalinkTrack>(_playlist.Take(_playlist.Count - 1));
            
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":done:"));
        }
        
        [Command("remove"), Aliases("del"), Description("Removes a track at a given index.")]
        public async Task Remove(CommandContext ctx, [Description("Index of the track to remove.")] int index)
        {
            if (!_playlist.Any()) return;

            if (index == 1)
            {
                var dummy = _playlist.Dequeue();
            }
            else
            {
                var playlist1 = _playlist.Take(index - 1);
                var playlist2 = _playlist.Skip(index + 1);
                _playlist = new Queue<LavalinkTrack>(playlist1.Concat(playlist2));
            }

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":done:"));
        }
        
        [Command("voteskip"), Aliases("vskip"), Description("Allows users present in a vocal channel to vote to skip the current song.")]
        [SuppressMessage("ReSharper", "RedundantCast")]
        public async Task VoteSkip(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            conn.PlaybackFinished += ConnOnPlaybackFinished;
            
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }
            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }
            if (conn.Channel.Users.Count(u => conn.Channel.Users.Contains(u) && !u.IsBot) == 1)
            {
                await conn.StopAsync();
            }

            _ = Task.Run(async () =>
            {
                var message =
                    await ctx.RespondAsync(
                        $"`0/{(int)conn.Channel.Users.Count(u => conn.Channel.Users.Contains(u) && !u.IsBot) / 2 + 1}` React under this message to vote to skip the current song");
                await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));

                var time = 0;
                while (time < 30)
                {
                    try
                    {
                        var res = await message.GetReactionsAsync(
                            DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"), 100);
                        
                        var count = res.Count(u => conn.Channel.Users.Contains(u) && !u.IsBot);

                        var majority = (int)conn.Channel.Users.Count(u => conn.Channel.Users.Contains(u) && !u.IsBot) / 2 + 1;
                        var majorityVote = count >= majority;
                        
                        if (majorityVote)
                        { 
                            await conn.StopAsync();
                            await message.ModifyAsync("People asked for it! Current song is skipped! :fast_forward:");
                            await message.DeleteAllReactionsAsync();
                            return;
                        }
                        
                        await message.ModifyAsync($"`{count}/{(int)conn.Channel.Users.Count(u => conn.Channel.Users.Contains(u) && !u.IsBot) / 2 + 1}` React under this message to vote to skip the current song");
                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    time++;
                    
                    await Task.Delay(1000);
                }

                await message.ModifyAsync("VoteSkip expired...");
                await message.DeleteAllReactionsAsync();
            });
        }

        private async Task ConnOnPlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
        {
            if (_playlist.Any() && sender.CurrentState.CurrentTrack == null)
            {
                await sender.PlayAsync(_playlist.Dequeue());
                if (_musicChannel.Channel != null)
                    await _musicChannel.Channel.SendMessageAsync($"Now playing **{sender.CurrentState.CurrentTrack.Title}**! ({sender.CurrentState.CurrentTrack.Uri})");
            }
        }

        private class MusicChannel { public DiscordChannel Channel { get; set; } }
    }
}