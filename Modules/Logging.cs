using Discord;
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
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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