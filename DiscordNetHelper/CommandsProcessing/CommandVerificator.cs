using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Discord.WebSocket;

namespace TehGM.DiscordNetBot.CommandsProcessing
{
    public class CommandVerificator : ICommandVerificator
    {
        public static ICommandVerificator DefaultPrefixed { get; set; } = new CommandVerificator()
        {
            IgnoreBots = true,
            AcceptMentionPrefix = true,
            AcceptGuildMessages = true,
            AcceptPrivateMessages = true,
            StringPrefix = "!"
        };
        public static ICommandVerificator DefaultPrefixedGuildOnly { get; set; } = new CommandVerificator()
        {
            IgnoreBots = true,
            AcceptMentionPrefix = true,
            AcceptGuildMessages = true,
            AcceptPrivateMessages = false,
            StringPrefix = "!"
        };

        /// <summary>Should this verificator return false for any bot message?</summary>
        public bool IgnoreBots { get; set; }
        /// <summary>Should mention be accepted instead of normal prefix?</summary>
        public bool AcceptMentionPrefix { get; set; }
        /// <summary>String prefix for commands.</summary>
        public string StringPrefix { get; set; }

        /// <summary>Should the verificator accept guild messages?</summary>
        public bool AcceptGuildMessages { get; set; }
        /// <summary>Should the verificator accept DMs?</summary>
        public bool AcceptPrivateMessages { get; set; }

        /// <summary>Does this bot require any prefix (string or mention)?</summary>
        public bool RequirePrefix => AcceptMentionPrefix || !string.IsNullOrWhiteSpace(StringPrefix);

        /// <inheritdoc/>
        public bool Verify(SocketCommandContext command, out string actualCommand)
        {
            actualCommand = null;
            if (IgnoreBots && (command.User.IsBot || command.User.IsWebhook))
                return false;
            if (!AcceptGuildMessages && command.Guild != null)
                return false;
            if (!AcceptPrivateMessages && command.IsPrivate)
                return false;
            if (!RequirePrefix)
            {
                actualCommand = command.Message.Content;
                return true;
            }
            // extract actual command so it can be confirmed with regex
            int cmdIndex = 0;
            if ((AcceptMentionPrefix && command.Message.HasMentionPrefix(command.Client.CurrentUser, ref cmdIndex)) ||
                (!string.IsNullOrWhiteSpace(StringPrefix) && command.Message.HasStringPrefix(StringPrefix, ref cmdIndex)))
            {
                actualCommand = command.Message.Content.Substring(cmdIndex);
                return true;
            }
            return false;
        }
    }
}
