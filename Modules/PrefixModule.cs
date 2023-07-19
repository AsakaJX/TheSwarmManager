using System.Diagnostics;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using TheSwarmManager.Modules.CustomEmbedBuilder;
using TheSwarmManager.Modules.Logging;

namespace TheSwarmManager.Modules.Prefixes {
    public class PrefixModule : ModuleBase<SocketCommandContext> {
        private Logger Log = new Logger();
        private readonly IConfigurationRoot _config;
        private readonly EmbedBuilding _EmbedBuilder;
        private EmbedBuilding EB = new EmbedBuilding();
        public PrefixModule() {
            _config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile("config.yml")
                .Build();

            _EmbedBuilder = new EmbedBuilding();
        }

        [Command("help")]
        public async Task HandleHelpCommand() {
            var embed = new EmbedBuilder { };
            embed
                .WithColor(176, 98, 101)
                .WithAuthor(
                    "Братик хочет узнать меня получше (≧◡≦)",
                    Context.Message.Author.GetAvatarUrl()
                )
                .WithTitle("Some Title")
                .AddField(
                    "**                                     ✬◦°˚°◦. ɢᴇɴᴇʀᴀʟ .◦°˚°◦✬                                     **",
                    "**``help``** - :t_rex: **Tech Support**\n" + "**``slots``** - рулетка\n" + ""
                )
                .AddField(
                    //"**                                       ✬◦°˚°◦. ᴏᴛʜᴇʀ .◦°˚°◦✬                                     **",
                    "**✬◦°˚°◦. ᴏᴛʜᴇʀ .◦°˚°◦✬**",
                    "***SomeCommand - Some description of that command***\n"
                        + "***SomeOtherCommand - A little bit longer description of that some command***"
                )
                .WithImageUrl("https://i.imgur.com/81MSEM0.gif");
            // Building embed
            await ReplyAsync(embed: embed.Build());
        }

        [Command("asaka")]
        public async Task HandleLoveCommand() {
            var path = "data.yml";
            var deserializer = new YamlDotNet.Serialization.Deserializer();

            try {
                using var reader = new StreamReader(path);
                var obj = deserializer.Deserialize<Dictionary<object, object>>(reader);
                var data = (Dictionary<object, object>)obj["other"];
                reader.Close();

                int incrementCounter = Convert.ToInt32(data["asaka_love_counter"]) + 1;
                data["asaka_love_counter"] = incrementCounter;

                using var writer = new StreamWriter(path);
                // Save Changes
                var serializer = new YamlDotNet.Serialization.Serializer();
                serializer.Serialize(writer, obj);

                await ReplyAsync($"Асаку полюбили уже {data["asaka_love_counter"]} раз. :heart:");
            } catch (Exception e) {
                Log.NewLog(Logging.LogSeverity.Error, "Prefix Module", e.ToString());
            }
        }

        [Command("sdn", RunMode = RunMode.Async)]
        public async Task HandleShutDownNowCommand() {
            if (Context.User.Id == Convert.ToUInt64(_config["usersGuild:owner"])) {
                Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "/bin/zsh";
                proc.StartInfo.Arguments = "-ic \" " + "shutdown -h 0" + " \"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
            } else {
                await ReplyAsync(embed: _EmbedBuilder.Error("Ты не мой любимый братик! Отстань от меня! ヾ(`ヘ´)ﾉﾞ"));
            }
        }

        [Command("sd", RunMode = RunMode.Async)]
        public async Task HandleShutDownCommand() {
            if (Context.User.Id == Convert.ToUInt64(_config["usersGuild:owner"])) {
                Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "/bin/zsh";
                proc.StartInfo.Arguments = "-ic \" " + "shutdown" + " \"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                do {
                    //string line = proc.StandardError.ReadLine();
                    await ReplyAsync(embed: _EmbedBuilder.Success("Братик, твой пк выключится через 1 минуту! Введи команду 'sdc' чтобы отменить. (＞﹏＜)"));
                }
                while (!proc.StandardOutput.EndOfStream);
            } else {
                await ReplyAsync(embed: _EmbedBuilder.Error("Ты не мой любимый братик! Отстань от меня! ヾ(`ヘ´)ﾉﾞ"));
            }
        }

        [Command("sdc", RunMode = RunMode.Async)]
        public async Task HandleShutDownCancelCommand() {
            if (Context.User.Id == Convert.ToUInt64(_config["usersGuild:owner"])) {
                Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "/bin/zsh";
                proc.StartInfo.Arguments = "-ic \" " + "shutdown -c" + " \"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                do {
                    // ! Could or couldn't work ->     ?? "This line is null." 
                    string line = proc.StandardError.ReadLine() ?? "This line is null.";
                    await ReplyAsync(embed: _EmbedBuilder.Success("Братик, выключение пк было остановлено! (￣ω￣)7"));
                }
                while (!proc.StandardOutput.EndOfStream);
            } else {
                await ReplyAsync(embed: _EmbedBuilder.Error("Ты не мой любимый братик! Отстань от меня! ヾ(`ヘ´)ﾉﾞ"));
            }
        }

        [Command("slots", RunMode = RunMode.Async)]
        public async Task HandleSlotsCommand() {
            var user = Context.User as SocketGuildUser;
            if (user == null) { return; }

            var embed = EB.NormalWithAuthor(Context.User, "",
                "```"
                + "    ╭—————————————╮     \n"
                + " ╭——│ Asaka Slots │——╮  \n"
                + " │  ╰—————————————╯  │  \n"
                + " │   0  0  0  0  0   │┏┓\n"
                + " │ > 0  0  0  0  0 < │┃ \n"
                + " │   0  0  0  0  0   │┛ \n"
                + " │                   │  \n"
                + " ╰———————————————————╯  "
                + "```").ToEmbedBuilder();

            await ReplyAsync(embed: embed.Build());

            var msg = await ReplyAsync(embed: embed.Build());
            //msg.ModifyAsync();

            string[] SlotsIndexes = new string[]
            {
                "83",
                "108",
                "133",
                "86",
                "111",
                "136",
                "89",
                "114",
                "139",
                "92",
                "117",
                "142",
                "95",
                "120",
                "145"
            };

            // 373
            Dictionary<char, int> Reel1 = new Dictionary<char, int>()
            {
                { '0', 80 },
                { '1', 70 },
                { '2', 60 },
                { '3', 50 },
                { '4', 40 },
                { '5', 30 },
                { '6', 20 },
                { '7', 10 },
                { '8', 3  },
                { '9', 10 }
            };
            // 372
            Dictionary<char, int> Reel2 = new Dictionary<char, int>()
            {
                { '0', 80 },
                { '1', 70 },
                { '2', 60 },
                { '3', 50 },
                { '4', 40 },
                { '5', 30 },
                { '6', 20 },
                { '7', 10 },
                { '8', 2  },
                { '9', 10 }
            };
            // 371
            Dictionary<char, int> Reel3 = new Dictionary<char, int>()
            {
                { '0', 80 },
                { '1', 70 },
                { '2', 60 },
                { '3', 50 },
                { '4', 40 },
                { '5', 30 },
                { '6', 20 },
                { '7', 10 },
                { '8', 1  },
                { '9', 10 }
            };
            // 374
            Dictionary<char, int> Reel4 = new Dictionary<char, int>()
            {
                { '0', 80 },
                { '1', 70 },
                { '2', 60 },
                { '3', 50 },
                { '4', 40 },
                { '5', 30 },
                { '6', 20 },
                { '7', 10 },
                { '8', 4  },
                { '9', 10 }
            };
            // 373
            Dictionary<char, int> Reel5 = new Dictionary<char, int>()
            {
                { '0', 80 },
                { '1', 70 },
                { '2', 60 },
                { '3', 50 },
                { '4', 40 },
                { '5', 30 },
                { '6', 20 },
                { '7', 10 },
                { '8', 3  },
                { '9', 10 }
            };

            Dictionary<char, int>[] ReelsCollection = new Dictionary<char, int>[] {
                Reel1, Reel2, Reel3, Reel4, Reel5
            };

            Dictionary<int, string> ReelsReady = new Dictionary<int, string>()
            {
                { 0, "" },
                { 1, "" },
                { 2, "" },
                { 3, "" },
                { 4, "" }
            };

            void LuckManipulation(ulong[] userID, char prizeID, int newPrizeValue, bool toggle = true) {
                if (!toggle) { return; }
                for (int i = 0; i < userID.Length; i++) {
                    if (user.Id == userID[i]) {
                        Reel1[prizeID] = newPrizeValue;
                        Reel2[prizeID] = newPrizeValue;
                        Reel3[prizeID] = newPrizeValue;
                        Reel4[prizeID] = newPrizeValue;
                        Reel5[prizeID] = newPrizeValue;
                    }
                }
            }

            LuckManipulation(new ulong[] { 230758744798134282 }, '8', 10000, false);
            LuckManipulation(new ulong[] {
                358116406421618689,
                929080513438822510
            }, '6', 100);
            LuckManipulation(new ulong[] { 323046843795898369 }, '8', 6000, false);

            // Preparing reels
            for (int i = 0; i < ReelsCollection.Length; i++) {
                foreach (var key in ReelsCollection[i]) {
                    for (int j = 0; j < key.Value; j++) {
                        ReelsReady[i] += key.Key;
                    }
                }
            }

            var gettingEmbed = msg.Embeds.First().ToEmbedBuilder();
            string embedDescription = gettingEmbed.Description;
            char[] embedDescriptionArray = embedDescription.ToCharArray();

            Random rand = new Random();
            Dictionary<char, int> Dict = new Dictionary<char, int>();

            for (int i = 0; i < 15; i++) {
                embedDescriptionArray[Convert.ToInt32(SlotsIndexes[i])] = ReelsReady[i / 3][
                    rand.Next(0, ReelsReady[i / 3].Length)
                ];

                if ((i + 1) % 3 == 0) {
                    embedDescription = new string(embedDescriptionArray);
                    await msg.ModifyAsync(x => x.Embed = gettingEmbed.WithDescription(embedDescription).Build());
                    await Task.Delay(1000);
                }

                if (i - 1 == 0 || (i - 1) % 3 == 0) {
                    if (!Dict.ContainsKey(embedDescriptionArray[Convert.ToInt32(SlotsIndexes[i])]))
                        Dict.Add(embedDescriptionArray[Convert.ToInt32(SlotsIndexes[i])], 1);
                    else
                        Dict[embedDescriptionArray[Convert.ToInt32(SlotsIndexes[i])]]++;
                }
            }

            var MiddleRow = Dict.ToList();
            MiddleRow.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            int MiddleRowMaxValue = MiddleRow[0].Value;
            int MiddleRowMaxKey = 0;

            for (int i = 0; i < MiddleRow.Count; i++) {
                if (MiddleRow[i].Value == MiddleRowMaxValue && Convert.ToInt32(MiddleRow[i].Key.ToString()) > MiddleRowMaxKey)
                    MiddleRowMaxKey = Convert.ToInt32(MiddleRow[i].Key.ToString());
            }

            string PrizeString = "";

            var MiddleValue = MiddleRow[0].Value;

            if (MiddleValue == 5)
                PrizeString = "* J A C K P O T *";

            string[] PrizesArray = new string[] {
                "Ничего xD",                    // 0
                "Кирпичи :bricks:",             // 1
                "Бетон :bricks:",               // 2
                "meow",                         // 3
                "Таймаут на 5 минут :skull:",   // 4
                "Таймаут на 15 минут :skull:",  // 5
                "Таймаут на 30 минут :skull:",  // 6
                "Кик с сервера",                // 7
                "Шанс на получение админки!",   // 8
                "Роль Элитного раба Нейро-самы" // 9
            };

            int[] MultiplierBlackList = new int[] { 0, 3, 7, 8, 9 };

            embedDescription = new string(embedDescription.Remove(156, PrizeString.Length).Insert(156, PrizeString));
            string embedAuthorName = new string(gettingEmbed.Author.Name.ToString());

            int PrizeMultiplier = MiddleValue == 5 ? PrizeMultiplier = 10 : PrizeMultiplier = MiddleValue;
            string CongratulationString = "";
            for (int i = 0; i < MultiplierBlackList.Length; i++) {
                if (PrizeMultiplier < 1) { break; }
                if (MiddleRowMaxKey == MultiplierBlackList[i]) {
                    CongratulationString = $"<@{Context.User.Id}> Братик, ты выйграл ***{PrizesArray[MiddleRowMaxKey]}***";
                    break;
                }
                CongratulationString = $"<@{Context.User.Id}> Братик, ты выйграл ***{PrizesArray[MiddleRowMaxKey]} x {PrizeMultiplier}***";
            }

            switch (MiddleValue) {
                case 1:
                    //await ReplyAsync(embed: _EmbedBuilder.Normal($"Joined {voiceState.VoiceChannel.Name}!", ""));
                    await ReplyAsync($"<@{Context.User.Id}> Братик, ты выйграл ***{PrizesArray[0]}***");
                    embedAuthorName = $"{embedAuthorName} - {PrizesArray[0]}"; break;
                case 2:
                    await ReplyAsync(CongratulationString);
                    embedAuthorName = $"{embedAuthorName} - {PrizesArray[MiddleRowMaxKey]}"; break;
                case 3:
                    await ReplyAsync(CongratulationString);
                    embedAuthorName = $"{embedAuthorName} - {PrizesArray[MiddleRowMaxKey]}"; break;
                case 4:
                    await ReplyAsync(CongratulationString);
                    embedAuthorName = $"{embedAuthorName} - {PrizesArray[MiddleRowMaxKey]}"; break;
                case 5:
                    await msg.ModifyAsync(x => x.Embed = gettingEmbed.WithDescription(embedDescription).Build());
                    embedAuthorName = $"{embedAuthorName} - {PrizesArray[MiddleRowMaxKey]}";
                    await ReplyAsync(CongratulationString); break;
                default:
                    await ReplyAsync(CongratulationString);
                    embedAuthorName = $"{embedAuthorName} - {PrizesArray[MiddleRowMaxKey]}"; break;
            }

            await msg.ModifyAsync(x => x.Embed = gettingEmbed.WithAuthor(embedAuthorName, Context.User.GetAvatarUrl()).Build());

            var _pconfig = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddYamlFile("config.yml")
                    .Build();

            var ownerRole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(_config["roleGuild:owner"]));
            var eliteVictimRole = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Id == Convert.ToUInt64(_config["roleGuild:elite"]));

            if (MiddleValue == 1) { return; }
            if (MiddleRowMaxKey == 4 || MiddleRowMaxKey == 5 || MiddleRowMaxKey == 6 || MiddleRowMaxKey == 7) {
                if (user.Roles.ToArray().Contains(ownerRole)) {
                    await ReplyAsync("Братик, ты слишком ценный материал в моем улье, я не могу позволить себе причинить тебе боль :heart:");
                    return;
                }
            }

            switch (MiddleRowMaxKey) {
                case 3:
                    if (MiddleValue == 5) {
                        await ReplyAsync("/ᐠﹷ ‸ ﹷ ᐟ\\\\ ﾉ\nhttps://www.youtube.com/watch?v=XGiqxxEjhNo");
                        break;
                    }
                    await Context.Channel.SendFileAsync("Resources/Media/mewo.mp4", "/ᐠﹷ ‸ ﹷ ᐟ\\\\ ﾉ"); break;
                case 4:
                    if (!user.Roles.ToArray().Contains(ownerRole)) {
                        await user.SetTimeOutAsync(TimeSpan.FromMinutes(5 * PrizeMultiplier));
                    }
                    break;
                case 5:
                    if (!user.Roles.ToArray().Contains(ownerRole)) {
                        await user.SetTimeOutAsync(TimeSpan.FromMinutes(15 * PrizeMultiplier));
                    }
                    break;
                case 6:
                    if (!user.Roles.ToArray().Contains(ownerRole)) {
                        await user.SetTimeOutAsync(TimeSpan.FromMinutes(30 * PrizeMultiplier));
                    }
                    break;
                case 7:
                    if (!user.Roles.ToArray().Contains(ownerRole)) {
                        await Discord.UserExtensions.SendMessageAsync(user, "Поздравляем с получением приза в рулетке Асаки (asaka#3260)!\nhttps://discord.gg/aQ2VMhSeak");
                        await user.KickAsync("Поздравляем с получением приза в рулетке Асаки (asaka#3260)!");
                    }
                    break;
                case 8:
                    if (MiddleValue != 5) {
                        await ReplyAsync("Братик ты не смог выбить все 5 восьмерок! Поэтому админки ты не получишь. ( `ε´ )");
                        return;
                    }
                    await ReplyAsync("Поздравляю братик! Ты выбил 5 восьмерок! Молодец! Хотя мне казалось что шанса 1 к 100 миллиардам будет достаточно... ");
                    await Task.Delay(3000);
                    await ReplyAsync("А теперь перейдем к самой интересной части! У тебя есть шанс 50 на 50 чтобы выбить админку либо же получить вместо нее ***:bricks: ЦЕЛУЮ КУЧУ КИРПИЧЕЙ :bricks:***!!!");
                    await Task.Delay(1000);
                    await ReplyAsync("Удачи Братик!");

                    string str = "Поздравляю братик! Ты выйграл";
                    for (int f = 0; f < 3; f++) {
                        str = str.Insert(str.Length, ".");
                        await Task.Delay(1000);
                        await ReplyAsync(str);
                    }

                    int PrizeAdmin = rand.Next(0, 2);
                    if (PrizeAdmin == 0)
                        await ReplyAsync("Поздравляю братик! Ты выйграл ***:bricks: ЦЕЛУЮ КУЧУ КИРПИЧЕЙ :bricks:***");
                    else {
                        await ReplyAsync("Поздравляю братик! Ты выйграл ***:crown: Админку :crown:***");
                        if (user.Roles.ToArray().Contains(ownerRole)) {
                            // await ReplyAsync("Братик, ты выйграл админку, но так как у тебя она уже была, я не смогу тебе ее выдать!");
                            await ReplyAsync("Братик, ты выйграл админку, но так как у тебя она уже была, я заберу у тебя ее!");
                            await user.RemoveRoleAsync(ownerRole);
                            await Task.Delay(1000);
                            await ReplyAsync("Удачи в следующий раз! (ノ_<。)ヾ(´ ▽ ` )");
                            break;
                        }
                        await user.AddRoleAsync(ownerRole);
                    }
                    break;
                case 9:
                    if (!user.Roles.ToArray().Contains(eliteVictimRole)) {
                        await user.AddRoleAsync(eliteVictimRole);
                    } else {
                        await ReplyAsync("Братик, ты уже мой любимый и элитный раб, тебе не нужна вторая такая же роль ( 〃▽〃)");
                    }
                    break;
                default:
                    break;
            }
        }

        [Command("slots.stats")]
        public async Task HandleSlotsStatsCommand() {
            await ReplyAsync("");
        }
    }
}