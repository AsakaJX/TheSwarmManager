using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using TheSwarmManager.Modules.CustomEmbedBuilder;
using TheSwarmManager.Modules.Logging;
using TheSwarmManager.Modules.Prefixes;
using TheSwarmManager.Modules.XPlayer;
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

            SubscribeDiscordEvents();
            SubscribeLavaLinkEvents();
        }

        public async Task InitializeAsync() {
            System.IO.DirectoryInfo di = new DirectoryInfo("Resources/StableDiffusionOutput/");
            foreach (FileInfo file in di.EnumerateFiles()) {
                file.Delete();
            }

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

                if (!_lavaNode.IsConnected) {
                    await _lavaNode.ConnectAsync();
                }
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
                .AddSingleton<EmbedBuilding>()

                .AddSingleton(_config)

                .BuildServiceProvider();
    }
}