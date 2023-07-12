using Discord;
using Victoria;

namespace TheSwarmManager.Modules.XPlayer {
    public class XLavaPlayer : LavaPlayer {
        public string ChannelName { get; }
        public XLavaPlayer(LavaSocket lavaSocket, IVoiceChannel voiceChannel, ITextChannel textChannel)
            : base(lavaSocket, voiceChannel, textChannel) {
            ChannelName = textChannel.Name;
        }
    }
}