using System.Text;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using TheSwarmManager.Services;

namespace TheSwarmManager.Modules.Logging {
    public enum LogSeverity {
        Critical,
        Debug,
        Error,
        Info,
        Verbose,
        Warning,
    }
    public class Logger {
        private IConfigurationRoot _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddYamlFile("config.yml")
            .Build();
        private PowerShellHandler PowerShell = new PowerShellHandler();
        Dictionary<LogSeverity, ConsoleColor> ColorTable = new Dictionary<LogSeverity, ConsoleColor> {
            {LogSeverity.Critical, ConsoleColor.Magenta},
            {LogSeverity.Debug, ConsoleColor.Yellow},
            {LogSeverity.Error, ConsoleColor.Red},
            {LogSeverity.Info, ConsoleColor.Green},
            {LogSeverity.Verbose, ConsoleColor.Yellow},
            {LogSeverity.Warning, ConsoleColor.Red},
        };
        private LogSeverity LogSeverityConverter(Discord.LogSeverity ls) {
            return (LogSeverity)(int)ls;
        }
        public Task NewLogForEvents(LogMessage msg) {
            NewLog(LogSeverityConverter(msg.Severity), msg.Source, msg.Message);
            return Task.CompletedTask;
        }
        public void NewLog(
            LogSeverity severity = LogSeverity.Info,
            string source = "* no source provided *",
            string message = "* empty *",
            int depth = 0
        ) {
            //? /```                Save Logs to file                  ```\
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var path = "config.yml";
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            using var reader = new StreamReader(path);
            var obj = deserializer.Deserialize<Dictionary<object, object>>(reader);
            reader.Close();

            var logsName = $"Logs/Log-{_config["lastRunDate"]}-{obj["DailyRunCounter"]}.txt";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{date} {source} {message}");
            File.AppendAllText(logsName, sb.ToString());
            //? \...                Save Logs to file                  .../

            if (source == "Victoria" && (message.IndexOf("Unable to connect to the remote server") == -1 && message.IndexOf("Unknown OP code received (ready)") == -1)) { return; }
            // string addSpacesDepth = String.Concat(Enumerable.Repeat(" ", depth));
            string arrow = "⇢";
            // if (depth > 0) { arrow = $"{addSpacesDepth}↳"; }
            if (depth > 0) { arrow = $"↳"; }

            if (message.IndexOf("Unknown OP code received (ready)") != -1) {
                severity = LogSeverity.Info;
                message = "LavaLink successfully connected!";
            }

            if (message.IndexOf("Unable to connect to the remote server") != -1) {
                message = "LavaLink is not running. Trying to connect again...";
                severity = LogSeverity.Warning;
            }

            int sourceLimit = 30;
            string addSpacesSeverity = severity.ToString().Length < "Critical".Length ? String.Concat(Enumerable.Repeat(" ", "Critical".Length - severity.ToString().Length)) : "";
            string addSpacesSource = source.ToString().Length < sourceLimit ? String.Concat(Enumerable.Repeat(" ", sourceLimit - source.ToString().Length)) : "";
            if (source.IndexOf('|') != -1) {
                string firstWord = source.Substring(0, source.IndexOf('|'));
                string secondWord = source.Substring(source.IndexOf('|') + 1);

                source = firstWord + addSpacesSource.Insert(0, " ") + secondWord;
                addSpacesSource = "";
            }
            Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write($"{date}  ");
            Console.ForegroundColor = ColorTable[severity]; Console.Write($"{addSpacesSeverity}{severity.ToString().ToUpper()}");
            Console.ForegroundColor = ConsoleColor.Cyan; Console.Write(" ⇢ ");
            Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write("[ ");
            Console.ForegroundColor = ConsoleColor.Blue; Console.Write($"{source}{addSpacesSource}");
            Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write(" ]");
            Console.ForegroundColor = ConsoleColor.Cyan; Console.Write($"\t{arrow}\t");
            Console.ForegroundColor = ConsoleColor.Gray; Console.Write($"{message}");

            Console.ResetColor();
            Console.WriteLine();
        }
    }
}