using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using TheSwarmManager.Modules.Logging;

namespace TheSwarmManager.Utils.ColorConverter {
    public class Converter {
        private Logger Log = new Logger();
        /// <summary>
        /// Getting color in RGB format from config file by it's name.
        /// </summary>
        /// <param name="requestedColor">Name in config file of requested color.</param>
        /// <returns></returns>
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