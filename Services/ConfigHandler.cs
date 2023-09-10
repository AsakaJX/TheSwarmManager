using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;

namespace TheSwarmManager.Services {
    public class ConfigHandler {
        public IConfigurationRoot GetConfig() {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile("config.yml")
                .Build();
        }
    }
}