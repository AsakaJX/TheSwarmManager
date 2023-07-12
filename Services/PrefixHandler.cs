using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
// using TheSwarmManager.Modules.Configuration;
using TheSwarmManager.Modules.Logging;

namespace TheSwarmManager.Services {
    internal class PrefixHandler {
        private Logger Log = new Logger();
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;

        public PrefixHandler(DiscordSocketClient client, CommandService commands, IConfigurationRoot config) {
            _client = client;
            _commands = commands;
            _config = config;
        }

        public async Task InitializeAsync() {
            _client.MessageReceived += HandleCommandAsync;
            await Task.CompletedTask;
        }

        public void AddModule<T>() {
            _commands.AddModuleAsync<T>(null);
        }

        public async Task ifMessageRespond(SocketMessage message, string ifMessage, string respondMessage, string respondMessageType = "string", string respondMessageWithFile = "") {
            if (message.Author.IsBot || message.Content.ToLower() != ifMessage) { return; }

            switch (respondMessageType) {
                case "video":
                    await message.Channel.SendFileAsync($"Resources/{respondMessage}", respondMessageWithFile);
                    return;
                default:
                    await message.Channel.SendMessageAsync(respondMessage);
                    return;
            }
        }

        public async Task ifMessageRespond(SocketMessage message, bool SearchForSubstring, string ifMessage, string respondMessage, string respondMessageType = "string", string respondMessageWithFile = "") {
            if (message.Author.IsBot) { return; }
            for (int i = 0; i <= message.Content.ToString().Length - ifMessage.Length; i++) {
                if (message.Content.ToString().Substring(i, ifMessage.Length).ToLower() != ifMessage)
                    continue;

                switch (respondMessageType) {
                    case "video":
                        await message.Channel.SendFileAsync($"Resources/{respondMessage}", respondMessageWithFile);
                        return;
                    default:
                        await message.Channel.SendMessageAsync(respondMessage);
                        return;
                }
            }
        }

        public async Task ifMentionedRespondWithPrefix(SocketMessage message) {
            for (int i = 0; i < message.MentionedUsers.ToArray().Length; i++) {
                if (message.Author.Id == _client.CurrentUser.Id) { return; }
                if (!(message.MentionedUsers.ToArray()[i].ToString() == _client.CurrentUser.ToString() && message.Content.ToString().Trim().IndexOf('>') == message.Content.ToString().Length - 1))
                    continue;

                await message.Channel.SendMessageAsync($"Братик, ты всегда можешь обращаться ко мне через префикс [ **{_config["prefix"][0]}** ], __тебе__ я отвечу в любой момент (⁄ ⁄>⁄ ▽ ⁄<⁄ ⁄)");
                Log.NewLog(Modules.Logging.LogSeverity.Info, "Prefix Handler|Request", $"{message.Author.ToString()} requested prefix");
                return;
            }
        }

        public async Task HandlePingCommand(SocketMessage message, CancellationToken token) {

            while (!token.IsCancellationRequested) {
                for (int i = 0; i < message.Content.ToString().Length - 4; i++) {
                    if (message.Author.IsBot || message.Content.ToString().Substring(i, "ping".Length) != "ping")
                        continue;
                    for (int h = 0; h < message.MentionedUsers.ToArray().Length; h++) {
                        if (message.MentionedUsers.ToArray()[h].Id == _client.CurrentUser.Id) {
                            await message.Channel.SendMessageAsync($"(￢_￢;) Братик, я не умею разговаривать сама с собой...");
                            continue;
                        }
                        string msgOriginal = message.Content.ToString();
                        string msgUserText = msgOriginal.Substring(msgOriginal.IndexOf('>', 1) + 1, (msgOriginal.IndexOf('*', 1)) - (msgOriginal.IndexOf('>', 1) + 1));
                        string msgToSend = $"<@{message.MentionedUsers.ToArray()[h].Id}>{msgUserText}";

                        int msgLimit = Convert.ToInt32(msgOriginal.Substring(msgOriginal.IndexOf('*', 1) + 1));
                        if (msgLimit <= 0 || msgLimit > 250) {
                            await message.Channel.SendMessageAsync($"(￢_￢;) Братик, лимит может быть в пределе от 1 до 250...");
                            break;
                        }

                        for (int g = msgLimit; g != 0; g--) {
                            await Discord.UserExtensions.SendMessageAsync(message.MentionedUsers.ToArray()[h], msgToSend);
                            await message.Channel.SendMessageAsync($"[ Left: {g - 1} ] {msgToSend}");
                        }
                    }
                }
            }
        }

        public async Task HandleCommandAsync(SocketMessage messageParam) {
            _ = Task.Run(async () => {
                var message = messageParam as SocketUserMessage;
                int argPos = 0;

                if (message == null) return;

                await ifMessageRespond(message, "forcedivorce", "https://cdn.discordapp.com/attachments/1069011963759304825/1071811178671656970/cat-jam.gif");
                await ifMessageRespond(message, true, "trade", "https://cdn.discordapp.com/attachments/1069011963759304825/1071812371678494870/trade-offer.gif");
                await ifMessageRespond(message, true, "meow", "mewo.mp4", "video", "/ᐠﹷ ‸ ﹷ ᐟ\\\\ ﾉ");
                await ifMessageRespond(message, "ayo", "ayo.gif", "video");

                await ifMentionedRespondWithPrefix(message);
                if (!(message.HasCharPrefix(_config["prefix"][0], ref argPos)) || message.Author.IsBot || message.Content.ToString().Trim().IndexOf('>') != -1) return;

                var context = new SocketCommandContext(_client, message);
                string contextConverted = message.Content.ToString().Trim(new Char[] { _config["prefix"][0] });

                await _commands.ExecuteAsync(
                    context: context,
                    argPos: argPos,
                    services: null);

                if (!(_commands.Search(contextConverted).IsSuccess) && message.MentionedUsers.ToArray().Length == 0)
                    await message.Channel.SendMessageAsync("(￢_￢;) Братик... я тебя не понимаю...");

                Log.NewLog(Modules.Logging.LogSeverity.Info, "Prefix Handler|Request", $"{message.Author.ToString()} requested {contextConverted}");
            });
            await Task.CompletedTask;
        }
    }
}