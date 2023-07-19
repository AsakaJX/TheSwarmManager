using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;
using TheSwarmManager.Modules.Logging;

namespace TheSwarmManager.Services {
    public class InteractionHandler {
        private Logger Log = new Logger();
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;

        public InteractionHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services) {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task InitializeAsync() {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.InteractionCreated += HandleInteraction;
        }

        public async Task HandleInteraction(SocketInteraction arg) {
            var ctx = new SocketInteractionContext(_client, arg);
            await _commands.ExecuteCommandAsync(ctx, _services);

            var ArgumentsCollection = (ctx.Interaction as SocketSlashCommand)?.Data.Options.ToArray();
            if (ArgumentsCollection?.Length == 0)
                Log.NewLog(Modules.Logging.LogSeverity.Info, "Interaction Handler|Request", $"{arg.User.Username} requested {(ctx.Interaction as SocketSlashCommand)?.Data.Name}");
            else {
                Log.NewLog(Modules.Logging.LogSeverity.Info, "Interaction Handler|Request", $"{arg.User.Username} requested {(ctx.Interaction as SocketSlashCommand)?.Data.Name} with arguments:");
                // string ArgumentsCollectionString = "";
                for (int i = 0; i < ArgumentsCollection?.Length; i++) {
                    // ArgumentsCollectionString += $"[{ArgumentsCollection[i].Type}]{ArgumentsCollection[i].Name}: {ArgumentsCollection[i].Value}";
                    string newLineOnLastIndex = "";
                    // ? idk about new line
                    // if (i + 1 == ArgumentsCollection.Length) { newLineOnLastIndex = "\n"; }
                    Log.NewLog(Modules.Logging.LogSeverity.Info, "Interaction Handler|Argument", $"[{ArgumentsCollection[i].Type}]{ArgumentsCollection[i].Name}: {ArgumentsCollection[i].Value.ToString()?.Replace("⁩", "").Replace("⁦", "").Trim()}{newLineOnLastIndex}", 1);
                }
            }
            // Log.NewLog(Modules.Logging.LogSeverity.Info, "Interaction Handler|Request", $"{(ctx.Interaction as SocketSlashCommand)?.Data.Options.FirstOrDefault()}", 1);
        }
    }
}