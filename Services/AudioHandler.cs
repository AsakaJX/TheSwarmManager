using System.Collections.Concurrent;
using TheSwarmManager.Modules.CustomEmbedBuilder;
using TheSwarmManager.Modules.Logging;
using TheSwarmManager.Modules.XPlayer;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace TheSwarmManager.Services {
    public sealed class AudioHandler {
        private Logger Log = new Logger();
        private readonly LavaNode<XLavaPlayer> _lavaNode;
        public readonly HashSet<ulong> VoteQueue;
        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
        private EmbedBuilding EB = new EmbedBuilding();

        public AudioHandler(LavaNode<XLavaPlayer> lavaNode) {
            _lavaNode = lavaNode;
            _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();

            _lavaNode.OnPlayerUpdated += OnPlayerUpdated;
            _lavaNode.OnStatsReceived += OnStatsReceived;
            _lavaNode.OnTrackEnded += OnTrackEnded;
            _lavaNode.OnTrackStarted += OnTrackStarted;
            _lavaNode.OnTrackException += OnTrackException;
            _lavaNode.OnTrackStuck += OnTrackStuck;
            _lavaNode.OnWebSocketClosed += OnWebSocketClosed;

            VoteQueue = new HashSet<ulong>();
        }

        private Task OnPlayerUpdated(PlayerUpdateEventArgs arg) {
            // Log.NewLog(LogSeverity.Info, "Audio Handler", $"Track update received for {arg.Track.Title}: {arg.Position}");
            return Task.CompletedTask;
        }

        private Task OnStatsReceived(StatsEventArgs arg) {
            // Log.NewLog(LogSeverity.Info, "Audio Handler", $"Lavalink has been up for {arg.Uptime}.");
            return Task.CompletedTask;
        }

        private async Task OnTrackStarted(TrackStartEventArgs arg) {
            await arg.Player.TextChannel.SendMessageAsync(embed: EB.Normal("Now playing", $"{arg.Track.Title}"));
            if (!_disconnectTokens.TryGetValue(arg.Player.VoiceChannel.Id, out var value)) {
                return;
            }

            if (value.IsCancellationRequested) {
                return;
            }

            value.Cancel(true);
            // await arg.Player.TextChannel.SendMessageAsync("Auto disconnect has been cancelled!");
        }

        private async Task OnTrackEnded(TrackEndedEventArgs args) {
            if (args.Reason != TrackEndReason.Finished) {
                return;
            }

            var player = args.Player;
            await player.TextChannel.SendMessageAsync(embed: EB.Normal("Track Ended", $"Player: {args.Player}\nTrack: {args.Track.Title}\nReason: {args.Reason.ToString()}"));

            try {
                if (!player.Queue.TryDequeue(out var lavaTrack)) {
                    await player.TextChannel.SendMessageAsync(embed: EB.Normal("Queue Ended", "Please add more tracks or bot will auto-disconnect in 5 minutes."));
                    _ = InitiateDisconnectAsync(args.Player, TimeSpan.FromSeconds(300));
                    return;
                }

                if (lavaTrack is null) {
                    await player.TextChannel.SendMessageAsync(embed: EB.Error("Next item in queue is not a track."));
                    return;
                }

                await args.Player.PlayAsync(lavaTrack);
                await args.Player.TextChannel.SendMessageAsync(embed: EB.Normal("{args.Reason}: {args.Track.Title}", $"Now playing: {lavaTrack.Title}"));
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        private async Task InitiateDisconnectAsync(LavaPlayer player, TimeSpan timeSpan) {
            if (!_disconnectTokens.TryGetValue(player.VoiceChannel.Id, out var value)) {
                value = new CancellationTokenSource();
                _disconnectTokens.TryAdd(player.VoiceChannel.Id, value);
            } else if (value.IsCancellationRequested) {
                _disconnectTokens.TryUpdate(player.VoiceChannel.Id, new CancellationTokenSource(), value);
                value = _disconnectTokens[player.VoiceChannel.Id];
            }

            await player.TextChannel.SendMessageAsync(embed: EB.Normal("Auto disconnect initiated!", $"Disconnecting in {timeSpan}..."));
            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled) {
                return;
            }

            await _lavaNode.LeaveAsync(player.VoiceChannel);
            await player.TextChannel.SendMessageAsync(embed: EB.Normal("Auto-Disconnect", "5 minutes passed. Disconnecting."));
        }

        private async Task OnTrackException(TrackExceptionEventArgs arg) {
            Log.NewLog(LogSeverity.Error, "Audio Handler", $"Track {arg.Track.Title} threw an exception. Please check Lavalink console/logs.");
            arg.Player.Queue.Enqueue(arg.Track);
            await arg.Player.TextChannel.SendMessageAsync(embed: EB.Normal("Queue", $"{arg.Track.Title} has been re-added to queue after throwing an exception."));
        }

        private async Task OnTrackStuck(TrackStuckEventArgs arg) {
            Log.NewLog(LogSeverity.Error, "Audio Handler", $"Track {arg.Track.Title} got stuck for {arg.Threshold}ms. Please check Lavalink console/logs.");
            arg.Player.Queue.Enqueue(arg.Track);
            await arg.Player.TextChannel.SendMessageAsync(embed: EB.Normal("Queue", $"{arg.Track.Title} has been re-added to queue after getting stuck."));
        }

        private Task OnWebSocketClosed(WebSocketClosedEventArgs arg) {
            Log.NewLog(LogSeverity.Warning, "Audio Handler", $"Discord WebSocket connection closed with following reason: {arg.Reason}");
            return Task.CompletedTask;
        }
    }
}