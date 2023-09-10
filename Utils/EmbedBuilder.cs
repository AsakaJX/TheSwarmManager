using Discord;
using Discord.WebSocket;
using TheSwarmManager.Utils.ColorConverter;

namespace TheSwarmManager.Utils.EmbedBuilder {
    public class Builder {
        private Converter ColorConverter = new Converter();
        // ? --------------------> NORMAL <--------------------
        /// <summary>
        /// Regular embed.
        /// </summary>
        /// <param name="title">Title of the embed.</param>
        /// <param name="description">Description of the embed.</param>
        /// <returns></returns>
        public Embed Normal(string title, string description) {
            var embed = new Discord.EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(ColorConverter.GetColor("normal"))
                .Build()
            ;
            return embed;
        }
        // ? --------------------> NORMAL(A) <--------------------
        /// <summary>
        /// Regular embed with author. With "normal" color.
        /// </summary>
        /// <param name="socketUser">Author of the message.</param>
        /// <param name="title">Title of the embed.</param>
        /// <param name="description">Description of the embed.</param>
        /// <returns></returns>
        public Embed NormalWithAuthor(SocketUser socketUser, string title, string description) {
            string username = socketUser.Username;
            string avatarUrl = socketUser.GetAvatarUrl();
            var embed = new Discord.EmbedBuilder()
                .WithTitle(title)
                .WithAuthor(username, avatarUrl)
                .WithDescription(description)
                .WithColor(ColorConverter.GetColor("normal"))
                .Build()
            ;
            return embed;
        }
        // ? --------------------> Normal-CurrentCommand(A) <--------------------
        /// <summary>
        /// Embed for slash commands that could take a while to respond,
        /// so we're responding with that one, because there's 3 sec limit for response.
        /// </summary>
        /// <param name="interaction">Interaction that's slow as fuck :) (We need name of interaction)</param>
        /// <returns></returns>
        public Embed NormalCCWithAuthor(SocketInteraction interaction) {
            // string username = socketUser.Username;
            // string avatarUrl = socketUser.GetAvatarUrl();
            string interactionName = (interaction as SocketSlashCommand)?.Data.Name ?? "error getting slash command name";
            var embed = new Discord.EmbedBuilder()
                .WithTitle($"Executing {interactionName} command.")
                // .WithAuthor(username, avatarUrl)
                // .WithDescription($"Executing {interactionName} command.")
                .WithColor(ColorConverter.GetColor("normal"))
                .Build()
            ;
            return embed;
        }
        // ? --------------------> ERROR <--------------------
        /// <summary>
        /// Embed for error cases.
        /// </summary>
        /// <param name="errorText">Message of the error.</param>
        /// <returns></returns>
        public Embed Error(string errorText) {
            var embed = new Discord.EmbedBuilder()
                .WithTitle("Ошибка")
                .WithColor(ColorConverter.GetColor("red"))
                .WithDescription(errorText)
                .Build()
            ;
            return embed;
        }
        // ? --------------------> ERROR(A) <--------------------
        /// <summary>
        /// Embed for error cases, but with author.
        /// </summary>
        /// <param name="socketUser">~~MOTHEF*CKER~~ Author of the error</param>
        /// <param name="errorText">Message of the error.</param>
        /// <returns></returns>
        public Embed ErrorWithAuthor(SocketUser socketUser, string errorText) {
            string username = socketUser.Username;
            string avatarUrl = socketUser.GetAvatarUrl();
            var embed = new Discord.EmbedBuilder()
                .WithTitle("Ошибка")
                .WithAuthor(username, avatarUrl)
                .WithColor(ColorConverter.GetColor("red"))
                .WithDescription(errorText)
                .Build()
            ;
            return embed;
        }
        // ? --------------------> SUCCESS <--------------------
        /// <summary>
        /// Embed for success results from commands.
        /// </summary>
        /// <param name="description">Description of the embed.</param>
        /// <returns></returns>
        public Embed Success(string description) {
            var embed = new Discord.EmbedBuilder()
                .WithTitle("Успех")
                .WithDescription(description)
                .WithColor(ColorConverter.GetColor("green"))
                .Build()
            ;
            return embed;
        }
        // ? --------------------> SUCCESS(A) <--------------------
        /// <summary>
        /// Embed for success results from commands, but with author.
        /// </summary>
        /// <param name="socketUser">~~Good boy~~ Author of the message.</param>
        /// <param name="description">Description of the embed.</param>
        /// <returns></returns>
        public Embed SuccessWithAuthor(SocketUser socketUser, string description) {
            string username = socketUser.Username;
            string avatarUrl = socketUser.GetAvatarUrl();
            var embed = new Discord.EmbedBuilder()
                .WithTitle("Успех")
                .WithAuthor(username, avatarUrl)
                .WithDescription(description)
                .WithColor(ColorConverter.GetColor("green"))
                .Build()
            ;
            return embed;
        }
    }
}