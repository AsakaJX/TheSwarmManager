using System.Collections.Concurrent;
using TheSwarmManager.Modules.Logging;
using TheSwarmManager.Modules.XPlayer;
using TheSwarmManager.Utils.EmbedBuilder;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace TheSwarmManager.Services {
    public sealed class AudioHandler {
        private Logger Log = new Logger();
        private readonly LavaNode<XLavaPlayer> _lavaNode;
        public readonly HashSet<ulong> VoteQueue;
        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
        private Builder EB = new Builder();

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
            await arg.Player.TextChannel.SendMessageAsync(embed: EB.Normal("Сейчас играет", $"{arg.Track.Title}"));
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
            // await player.TextChannel.SendMessageAsync(embed: EB.Normal("Трек закончился", $"Плеер: {args.Player}\nТрек: {args.Track.Title}\nПричина: {args.Reason.ToString()}"));

            try {
                if (!player.Queue.TryDequeue(out var lavaTrack)) {
                    DateTime now = DateTime.UtcNow;
                    long nowUnix = new DateTimeOffset(now).ToUnixTimeSeconds();
                    long nowUnixPlusMinute = new DateTimeOffset(now.AddMinutes(5)).ToUnixTimeSeconds();

                    await player.TextChannel.SendMessageAsync(embed: EB.Normal("Очередь закончилась", $"Добавьте еще треков или я автоматически отключусь через <t:{nowUnixPlusMinute}:R>."));
                    _ = InitiateDisconnectAsync(args.Player, TimeSpan.FromMinutes(5));
                    return;
                }

                if (lavaTrack is null) {
                    await player.TextChannel.SendMessageAsync(embed: EB.Error("Не могу определить следующий трек в очереди (null).\nПожалуйста сообщите разработчику об этом Discord: thisusernameisunavailable."));
                    return;
                }

                await args.Player.PlayAsync(lavaTrack);
                await args.Player.TextChannel.SendMessageAsync(embed: EB.Normal($"{args.Reason}: {args.Track.Title}", $"Сейчас играет: {lavaTrack.Title}"));
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

            // await player.TextChannel.SendMessageAsync(embed: EB.Normal("Auto disconnect initiated!", $"Disconnecting in {timeSpan}..."));
            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled) {
                return;
            }

            await _lavaNode.LeaveAsync(player.VoiceChannel);
            await player.TextChannel.SendMessageAsync(embed: EB.Normal("Автоматическое отключение", "5 минут прошли. Отключаюсь."));
        }

        private async Task OnTrackException(TrackExceptionEventArgs arg) {
            Log.NewLog(LogSeverity.Error, "Audio Handler", $"Track {arg.Track.Title} threw an exception. Please check Lavalink console/logs.");
            arg.Player.Queue.Enqueue(arg.Track);
            await arg.Player.TextChannel.SendMessageAsync(embed: EB.Normal("Очередь", $"Трек {arg.Track.Title} был заного добавлен в очередь после того как бросил ошибку."));
        }

        private async Task OnTrackStuck(TrackStuckEventArgs arg) {
            Log.NewLog(LogSeverity.Error, "Audio Handler", $"Track {arg.Track.Title} got stuck for {arg.Threshold}ms. Please check Lavalink console/logs.");
            arg.Player.Queue.Enqueue(arg.Track);
            await arg.Player.TextChannel.SendMessageAsync(embed: EB.Normal("Очередь", $"Трек {arg.Track.Title} был заного добавлен в очередь после того как он \"застрял\" на {arg.Threshold}мс."));
        }

        private Task OnWebSocketClosed(WebSocketClosedEventArgs arg) {
            Log.NewLog(LogSeverity.Warning, "Audio Handler", $"Discord WebSocket connection closed with following reason: {arg.Reason}");
            return Task.CompletedTask;
        }
    }
}