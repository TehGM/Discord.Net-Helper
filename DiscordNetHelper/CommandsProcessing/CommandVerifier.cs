using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Discord.WebSocket;

namespace TehGM.DiscordNetBot
{
    public class CommandVerifier : ICommandVerifier
    {
        public static ICommandVerifier DefaultPrefixed { get; set; } = new CommandVerifier()
        {
            IgnoreBots = true,
            AcceptMentionPrefix = true,
            AcceptGuildMessages = true,
            AcceptPrivateMessages = true,
            StringPrefix = "!",
            TrimSpaceAfterStringPrefix = false
        };
        public static ICommandVerifier DefaultPrefixedGuildOnly { get; set; } = new CommandVerifier()
        {
            IgnoreBots = true,
            AcceptMentionPrefix = true,
            AcceptGuildMessages = true,
            AcceptPrivateMessages = false,
            StringPrefix = "!",
            TrimSpaceAfterStringPrefix = false
        };

        /// <summary>Should this verifier return false for any bot message?</summary>
        public bool IgnoreBots { get; set; }
        /// <summary>Should mention be accepted instead of normal prefix?</summary>
        public bool AcceptMentionPrefix { get; set; }
        /// <summary>String prefix for commands.</summary>
        public string StringPrefix { get; set; }

        /// <summary>Should the verifier accept guild messages?</summary>
        public bool AcceptGuildMessages { get; set; }
        /// <summary>Should the verifier accept DMs?</summary>
        public bool AcceptPrivateMessages { get; set; }
        /// <summary>Should spaces after <see cref="StringPrefix"/> be removed?</summary>
        /// <remarks><para>This affects only actualCommand given by <see cref="Verify(SocketCommandContext, out string)"/> method.</para>
        /// <para>Spaces are always removed after Mention prefix.</para>
        /// <para>Enable this if you want command and prefix to be space separated, ie "!bot ping".</para></remarks>
        public bool TrimSpaceAfterStringPrefix { get; set; }

        /// <summary>Does this bot require any prefix (string or mention)?</summary>
        public bool RequirePrefix => AcceptMentionPrefix || !string.IsNullOrWhiteSpace(StringPrefix);

        /// <inheritdoc/>
        public virtual bool Verify(SocketCommandContext command, out string actualCommand)
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
            if (AcceptMentionPrefix && command.Message.HasMentionPrefix(command.Client.CurrentUser, ref cmdIndex))
            {
                actualCommand = GetActualCommand(command, cmdIndex, true);
                return true;
            }
            if (!string.IsNullOrWhiteSpace(StringPrefix) && command.Message.HasStringPrefix(StringPrefix, ref cmdIndex))
            {
                actualCommand = GetActualCommand(command, cmdIndex, TrimSpaceAfterStringPrefix);
                return true;
            }
            return false;
        }

        private static string GetActualCommand(SocketCommandContext command, int cmdIndex, bool trimPrefixSpace)
        {
            string actualCommand = command.Message.Content.Substring(cmdIndex);
            if (trimPrefixSpace)
                actualCommand = actualCommand.TrimStart(' ');
            return actualCommand;
        }
    }
}
