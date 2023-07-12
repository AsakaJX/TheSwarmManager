using TheSwarmManager.Services;

namespace TheSwarmManager {
    public class Initialize {
        // Program entry point
        private static Task Main()
            => new BotHandler().InitializeAsync();
    }
}