using Discord;
using Discord.WebSocket;
using TheSwarmManager.Modules.ColorConverter;

namespace TheSwarmManager.Modules.CustomEmbedBuilder {
    public class EmbedBuilding {
        private Colors ColorConverter = new Colors();
        // ? --------------------> NORMAL <--------------------
        public Embed Normal(string title, string description) {
            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(ColorConverter.GetColor("normal"))
                .Build()
            ;
            return embed;
        }
        // ? --------------------> NORMAL(A) <--------------------
        public Embed NormalWithAuthor(SocketUser socketUser, string title, string description) {
            string username = socketUser.Username;
            string avatarUrl = socketUser.GetAvatarUrl();
            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithAuthor(username, avatarUrl)
                .WithDescription(description)
                .WithColor(ColorConverter.GetColor("normal"))
                .Build()
            ;
            return embed;
        }
        // ? --------------------> Normal-CurrentCommand(A) <--------------------
        public Embed NormalCCWithAuthor(SocketInteraction interaction) {
            // string username = socketUser.Username;
            // string avatarUrl = socketUser.GetAvatarUrl();
            string interactionName = (interaction as SocketSlashCommand)?.Data.Name ?? "error getting slash command name";
            var embed = new EmbedBuilder()
                .WithTitle($"Executing {interactionName} command.")
                // .WithAuthor(username, avatarUrl)
                // .WithDescription($"Executing {interactionName} command.")
                .WithColor(ColorConverter.GetColor("normal"))
                .Build()
            ;
            return embed;
        }
        // ? --------------------> ERROR <--------------------
        public Embed Error(string errorText) {
            var embed = new EmbedBuilder()
                .WithTitle("Error")
                .WithColor(ColorConverter.GetColor("red"))
                .WithDescription(errorText)
                .Build()
            ;
            return embed;
        }
        // ? --------------------> ERROR(A) <--------------------
        public Embed ErrorWithAuthor(SocketUser socketUser, string errorText) {
            string username = socketUser.Username;
            string avatarUrl = socketUser.GetAvatarUrl();
            var embed = new EmbedBuilder()
                .WithTitle("Error")
                .WithAuthor(username, avatarUrl)
                .WithColor(ColorConverter.GetColor("red"))
                .WithDescription(errorText)
                .Build()
            ;
            return embed;
        }
        // ? --------------------> SUCCESS <--------------------
        public Embed Success(string description) {
            var embed = new EmbedBuilder()
                .WithTitle("Success")
                .WithDescription(description)
                .WithColor(ColorConverter.GetColor("green"))
                .Build()
            ;
            return embed;
        }
        // ? --------------------> SUCCESS(A) <--------------------
        public Embed SuccessWithAuthor(SocketUser socketUser, string description) {
            string username = socketUser.Username;
            string avatarUrl = socketUser.GetAvatarUrl();
            var embed = new EmbedBuilder()
                .WithTitle("Success")
                .WithAuthor(username, avatarUrl)
                .WithDescription(description)
                .WithColor(ColorConverter.GetColor("green"))
                .Build()
            ;
            return embed;
        }
    }
}