using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Pastel;
using TheSwarmManager.Modules.Logging;
using TheSwarmManager.Modules.Prefixes;
using TheSwarmManager.Modules.XPlayer;
using TheSwarmManager.Utils.EmbedBuilder;
using Victoria;

namespace TheSwarmManager.Services {
    public class BotHandler {
        private Logger Log = new Logger();
        private IConfigurationRoot _config { get; set; }
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;
        private readonly PrefixHandler _prefixHandler;
        private readonly ServiceProvider _services;
        private readonly LavaNode<XLavaPlayer> _lavaNode;
        private readonly AudioHandler _audioService;

        public BotHandler() {
            _config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile("config.yml")
                .Build();

            _services = ConfigureServices();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _interactionService = _services.GetRequiredService<InteractionService>();
            _prefixHandler = _services.GetRequiredService<PrefixHandler>();
            _lavaNode = _services.GetRequiredService<LavaNode<XLavaPlayer>>();
            _audioService = _services.GetRequiredService<AudioHandler>();

            //  Checking for password from the database.
            //! If wrong -> exiting.
            string passwordHash = Log.NewLog(Modules.Logging.LogSeverity.Warning, "Bot Handler|Database", "Please, enter password from the database: ", askForPassword: true);
            Database.DBHandler _db = new Database.DBHandler();
            while (passwordHash == string.Empty)
                Thread.Sleep(1000);
            if (passwordHash != _config["passwordHashes:database"]) {
                Log.NewCriticalError(100, "Bot Handler|Database", "Password is incorrect!".Pastel("#ff3434"));
            } else {
                Log.NewLog(Modules.Logging.LogSeverity.Info, "Bot Handler|Database", "Password is correct!".Pastel("#70ff38"));
                if (_db.TestConnection()) {
                    Log.NewLog(Modules.Logging.LogSeverity.Info, "Database Handler", $"{"Successfully".Pastel("#70ff38")} connected to database!");
                    if (_db.GetConnection()?.State.ToString() == "Open") { _db.CloseConnection(); }
                    _db.OpenConnection();
                }
            }

            SubscribeDiscordEvents();
            SubscribeLavaLinkEvents();
        }

        public async Task InitializeAsync() {
            if (!Directory.Exists("Resources/StableDiffusionOutput")) { Directory.CreateDirectory("Resources/StableDiffusionOutput"); }
            if (!Directory.Exists("CTS")) { Directory.CreateDirectory("CTS"); }

            System.IO.DirectoryInfo STDO_dir = new DirectoryInfo("Resources/StableDiffusionOutput/");
            foreach (FileInfo file in STDO_dir.EnumerateFiles()) {
                file.Delete();
            }

            System.IO.DirectoryInfo PINGCTS_dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            foreach (FileInfo file in PINGCTS_dir.EnumerateFiles()) {
                if (file.Name.IndexOf("CTS/cts-") == -1) { continue; }
                file.Delete();
            }

            //? /```                    Saving run counter for log files                     ```\
            var path = "config.yml";
            var deserializer = new YamlDotNet.Serialization.Deserializer();

            try {
                using var reader = new StreamReader(path);
                var obj = deserializer.Deserialize<Dictionary<object, object>>(reader);
                reader.Close();

                int writeCounter = obj["lastRunDate"].ToString() != DateTime.Now.ToString("yyyy-MM-dd") ? 0 : Convert.ToInt32(obj["DailyRunCounter"]) + 1;
                obj["DailyRunCounter"] = writeCounter;

                var writeDate = DateTime.Now.ToString("yyyy-MM-dd");
                obj["lastRunDate"] = writeDate;

                using var writer = new StreamWriter(path);
                // Save Changes
                var serializer = new YamlDotNet.Serialization.Serializer();
                serializer.Serialize(writer, obj);
                writer.Close();
            } catch (Exception e) {
                Log.NewLog(Modules.Logging.LogSeverity.Error, "Bot Handler", e.ToString());
            }
            //? \...                    Saving run counter for log files                     .../

            await _client.LoginAsync(TokenType.Bot, _config["tokens:discord"]);
            await _client.StartAsync();

            await _services.GetRequiredService<InteractionHandler>().InitializeAsync();
            _prefixHandler.AddModule<PrefixModule>();
            await _prefixHandler.InitializeAsync();

            await Task.Delay(-1);
        }

        // Hook Any Client Events Up Here.
        private void SubscribeLavaLinkEvents() {
            _lavaNode.OnLog += Log.NewLogForEvents;
        }

        private void SubscribeDiscordEvents() {
            _client.Ready += ReadyAsync;
            _client.Log += Log.NewLogForEvents;
        }

        private Task ReadyAsync() {
            _ = Task.Run(async () => {
                await _services.GetRequiredService<InteractionService>().RegisterCommandsGloballyAsync(true);

                if (!_lavaNode.IsConnected)
                    await _lavaNode.ConnectAsync();

                await _client.SetGameAsync("/help intensively", null, ActivityType.Watching);
                await _client.SetActivityAsync(_client.Activity);
            });
            return Task.CompletedTask;
        }
        private ServiceProvider ConfigureServices() => new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig {
                    GatewayIntents = Discord.GatewayIntents.All,
                    LogGatewayIntentWarnings = false,
                    AlwaysDownloadUsers = true,
                }))

                .AddSingleton<LavaNode<XLavaPlayer>>()
                .AddSingleton(x => new LavaConfig() {
                    SelfDeaf = true,
                    ReconnectAttempts = 3,
                    Port = 2333,
                    Hostname = "localhost",
                    EnableResume = true
                })
                .AddSingleton<AudioHandler>()

                .AddSingleton<InteractionService>()
                .AddSingleton<InteractionHandler>()
                .AddSingleton<CommandService>()
                .AddSingleton<PrefixHandler>()
                .AddSingleton<Builder>()

                .AddSingleton<Database.DBHandler>()

                .AddSingleton(_config)

                .BuildServiceProvider();
    }
}