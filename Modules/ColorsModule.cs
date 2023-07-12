using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using TheSwarmManager.Modules.Logging;

namespace TheSwarmManager.Modules.ColorConverter {
    public class Colors {
        private Logger Log = new Logger();
        public Color GetColor(string requestedColor) {
            var _config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile("config.yml")
                .Build();

            if (_config[$"colors:{requestedColor}:0"] == null) {
                Log.NewLog(Modules.Logging.LogSeverity.Error, "Colors Module", "Invalid requested color!");
                return new Color(0, 0, 0);
            }

            return new Color(Convert.ToInt32(_config[$"colors:{requestedColor}:0"]), Convert.ToInt32(_config[$"colors:{requestedColor}:1"]), Convert.ToInt32(_config[$"colors:{requestedColor}:2"]));
        }
    }
}