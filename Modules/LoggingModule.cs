using System.Security.Cryptography;
using System.Text;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Pastel;
using TheSwarmManager.Services.Database;

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
        private readonly int sourceLimit = 35;
        private readonly IConfigurationRoot _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddYamlFile("config.yml")
            .Build();

        private readonly Dictionary<LogSeverity, string> ColorTable = new Dictionary<LogSeverity, string> {
            {LogSeverity.Critical, "#ea00ff"},
            {LogSeverity.Debug, "#fbff00"},
            {LogSeverity.Error, "#ff3434"},
            {LogSeverity.Info, "#70ff38"},
            {LogSeverity.Verbose, "#fbff00"},
            {LogSeverity.Warning, "#ff3434"},
        };

        /// <summary>
        /// Custom logging method for events. Developed by Asaka.
        /// </summary>
        /// <param name="msg">Log message that needs to be processed.</param>
        /// <returns>Completed task.</returns>
        public Task NewLogForEvents(LogMessage msg) {
            NewLog((LogSeverity)(int)msg.Severity, msg.Source, msg.Message);
            return Task.CompletedTask;
        }
        /// <summary>
        /// Like System.Console.ReadLine(), only with a mask. Default mask is "*".
        /// </summary>
        /// <param name="mask">a <c>char</c> representing your choice of console mask</param>
        /// <returns>the string the user typed in </returns>

        private string ReadPassword(char mask) {
            const int ENTER = 13, BACKSP = 8, CTRLBACKSP = 127;
            int[] FILTERED = { 0, 27, 9, 10 /*, 32 space, if you care */ };

            var pass = new Stack<char>();
            char chr = (char)0;

            while ((chr = System.Console.ReadKey(true).KeyChar) != ENTER) {
                if (chr == BACKSP) {
                    if (pass.Count > 0) {
                        System.Console.Write("\b \b");
                        pass.Pop();
                    }
                } else if (chr == CTRLBACKSP) {
                    while (pass.Count > 0) {
                        System.Console.Write("\b \b");
                        pass.Pop();
                    }
                } else if (FILTERED.Count(x => chr == x) > 0) { } else {
                    pass.Push((char)chr);
                    System.Console.Write(mask.ToString().Pastel(ConsoleColor.Red));
                }
            }

            Console.WriteLine();

            string output = ComputeSHA256(new string(pass.Reverse().ToArray()));
            if (output == _config["passwordHashes:database"].ToString()) {
                DBHandler db = new DBHandler();
                db.SetupConnectionInformation(new string(pass.Reverse().ToArray()));
            }

            return ComputeSHA256(new string(pass.Reverse().ToArray()));
        }

        private static string ComputeSHA256(string s) {
            using (SHA256 sha256 = SHA256.Create()) {
                // Compute the hash of the given string
                byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(s));

                // Convert the byte array to string format
                return BitConverter.ToString(hashValue).Replace("-", "");
            }
        }

        /// <summary>
        /// Like System.Console.ReadLine(), only with a mask. Default mask is "*".
        /// </summary>
        /// <returns>the string the user typed in </returns>
        private string ReadPassword() {
            return ReadPassword('*');
        }
        /// <summary>
        /// Custom logging method. Developed by Asaka.
        /// </summary>
        /// <param name="severity">Severity of the message</param>
        /// <param name="source">Source of the message</param>
        /// <param name="message">The message itself</param>
        /// <param name="depth">(Optional)Depth of the message (just changes arrow)</param>
        /// <param name="askForInput">(Optional)Ask for input before writing new line ?</param>
        /// <param name="askForPassword">(Optional)Ask for password before writing new line ?</param>
        /// <returns>Empty string or input/password, depends on what you choosen.</returns>

        public string NewLog(
            LogSeverity severity = LogSeverity.Info,
            string source = "* no source provided *",
            string message = "* empty *",
            int depth = 0,
            bool askForInput = false,
            bool askForPassword = false
        ) {
            //? /```````````````````````````Save Logs to file```````````````````````````\
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var path = "config.yml";
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            using var reader = new StreamReader(path);
            var obj = deserializer.Deserialize<Dictionary<object, object>>(reader);
            reader.Close();

            var logsName = $"Logs/Log-{obj["lastRunDate"]}-{obj["DailyRunCounter"]}.txt";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{date} {source} {message}");
            File.AppendAllText(logsName, sb.ToString());
            //? \...........................Save Logs to file.........................../

            if (source == "Victoria" && (message.IndexOf("Unable to connect to the remote server") == -1 && message.IndexOf("Unknown OP code received (ready)") == -1)) { return string.Empty; }
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

            string addSpacesSeverity = severity.ToString().Length < "Critical".Length ? String.Concat(Enumerable.Repeat(" ", "Critical".Length - severity.ToString().Length)) : "";
            string addSpacesSource = source.ToString().Length < sourceLimit ? String.Concat(Enumerable.Repeat(" ", sourceLimit - source.ToString().Length)) : "";
            if (source.IndexOf('|') != -1) {
                string firstWord = source.Substring(0, source.IndexOf('|'));
                string secondWord = source.Substring(source.IndexOf('|') + 1);

                source = firstWord + addSpacesSource.Insert(0, " ") + secondWord;
                addSpacesSource = "";
            }

            // Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write($"{date}  ");
            // Console.ForegroundColor = ColorTable[severity]; Console.Write($"{addSpacesSeverity}{severity.ToString().ToUpper()}");
            // Console.ForegroundColor = ConsoleColor.Cyan; Console.Write(" ⇢ ");
            // Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write("[ ");
            // Console.ForegroundColor = ConsoleColor.Blue; Console.Write($"{source}{addSpacesSource}");
            // Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write(" ]");
            // Console.ForegroundColor = ConsoleColor.Cyan; Console.Write($"\t{arrow}\t");
            // Console.ForegroundColor = ConsoleColor.Gray; Console.Write($"{message}");

            Console.Write($"{date.Pastel(ConsoleColor.Gray)}  {addSpacesSeverity}{severity.ToString().ToUpper().Pastel(ColorTable[severity])}{" ⇢ ".Pastel(ConsoleColor.Cyan)}{"[ ".Pastel("#707070")}{source.Pastel("#2B52AB")}{addSpacesSource}{" ]".Pastel("#707070")}       {arrow.Pastel(ConsoleColor.Cyan)}       {message.Pastel(ConsoleColor.DarkGray)}");

            if (askForInput) {
                string input = Convert.ToString(Console.Read());
                return input;
            }

            if (askForPassword)
                return ReadPassword();

            Console.WriteLine();
            _ = Task.Run(() => {
                ConsoleKeyInfo key = Console.ReadKey(true);
                while (key.KeyChar.ToString().ToLower() != "q") {
                    key = Console.ReadKey(true);
                }
                Console.Write($"{date.Pastel(ConsoleColor.Gray)}  {" "}{LogSeverity.Warning.ToString().ToUpper().Pastel(ColorTable[LogSeverity.Warning])}{" ⇢ ".Pastel(ConsoleColor.Cyan)}{"[ ".Pastel("#707070")}{"User Input".Pastel("#2B52AB")}{"                         "}{" ]".Pastel("#707070")}       {arrow.Pastel(ConsoleColor.Cyan)}       {"\"Q\" key has been pressed! Shutting down...".Pastel(ConsoleColor.DarkGray)}");
                Environment.Exit(1000);
            });
            return string.Empty;
        }
        /// <summary>
        /// Custom logging method for critical errors that need to use Environment.Exit(code)
        /// </summary>
        /// <param name="code">Error code</param>
        /// <param name="source">Source of the error</param>
        /// <param name="message">(Optional)Custom message before system log is sent.</param>
        /// <param name="depth">(Optional)Depth of the message(just changes arrow)</param>
        public void NewCriticalError(
            int code = -1,
            string source = "* no source provided *",
            string message = "* empty *",
            int depth = 0
        ) {
            string messageFirstHalf = $"Program experienced a {"CRITICAL".Pastel("#ea00ff")} error with code {code.ToString().Pastel("#ea00ff")}";
            string messageSecondHalf = message;
            //? /```````````````````````````Save Logs to file```````````````````````````\
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var path = "config.yml";
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            using var reader = new StreamReader(path);
            var obj = deserializer.Deserialize<Dictionary<object, object>>(reader);
            reader.Close();

            var logsName = $"Logs/Log-{obj["lastRunDate"]}-{obj["DailyRunCounter"]}.txt";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{date} {source} {messageFirstHalf} + {messageSecondHalf}");
            File.AppendAllText(logsName, sb.ToString());
            //? \...........................Save Logs to file.........................../

            string arrow = "⇢";
            if (depth > 0) { arrow = $"↳"; }

            string addSpacesSeverity = "Critical".ToString().Length < "Critical".Length ? String.Concat(Enumerable.Repeat(" ", "Critical".Length - "Critical".ToString().Length)) : "";
            string addSpacesSource = source.ToString().Length < sourceLimit ? String.Concat(Enumerable.Repeat(" ", sourceLimit - source.ToString().Length)) : "";
            if (source.IndexOf('|') != -1) {
                string firstWord = source.Substring(0, source.IndexOf('|'));
                string secondWord = source.Substring(source.IndexOf('|') + 1);

                source = firstWord + addSpacesSource.Insert(0, " ") + secondWord;
                addSpacesSource = "";
            }

            if (message != "* empty *") {
                Console.Write($"{date.Pastel(ConsoleColor.Gray)}  {addSpacesSeverity}{"CRITICAL".Pastel("#ea00ff")}{" ⇢ ".Pastel(ConsoleColor.Cyan)}{"[ ".Pastel("#707070")}{source.Pastel("#2B52AB")}{addSpacesSource}{" ]".Pastel("#707070")}       {"↳".Pastel(ConsoleColor.Cyan)}       {messageSecondHalf.Pastel(ConsoleColor.DarkGray)}");
                Console.WriteLine();
            }
            Console.Write($"{date.Pastel(ConsoleColor.Gray)}  {addSpacesSeverity}{"CRITICAL".Pastel("#ea00ff")}{" ⇢ ".Pastel(ConsoleColor.Cyan)}{"[ ".Pastel("#707070")}{source.Pastel("#2B52AB")}{addSpacesSource}{" ]".Pastel("#707070")}       {arrow.Pastel(ConsoleColor.Cyan)}       {messageFirstHalf.Pastel(ConsoleColor.DarkGray)}");
            Console.WriteLine();

            DBHandler db = new DBHandler();
            if (db.GetConnection()?.State.ToString() == "Open") { db.CloseConnection(); }
            Environment.Exit(code);
        }
    }
}