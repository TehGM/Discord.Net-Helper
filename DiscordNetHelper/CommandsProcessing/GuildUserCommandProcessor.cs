using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TehGM.DiscordNetHelper.CommandsProcessing
{
    public class GuildUserCommandProcessor : ICommandProcessor
    {
        private readonly Regex _regex;
        private readonly Func<SocketUserMessage, SocketTextChannel, SocketGuildUser, Match, Task> _commandProcess;
        private readonly bool _ignoreBots;

        public GuildUserCommandProcessor(Regex regex, Func<SocketUserMessage, SocketTextChannel, SocketGuildUser, Match, Task> method, bool ignoreBots = true)
        {
            this._regex = regex;
            this._commandProcess = method;
            this._ignoreBots = ignoreBots;
        }

        public GuildUserCommandProcessor(string pattern, RegexOptions options, Func<SocketUserMessage, SocketTextChannel, SocketGuildUser, Match, Task> method, bool ignoreBots = true)
            : this(new Regex(pattern, options), method) { }
        public GuildUserCommandProcessor(string pattern, Func<SocketUserMessage, SocketTextChannel, SocketGuildUser, Match, Task> method, bool ignoreBots = true)
            : this(pattern, AnyUserCommandProcessor.DefaultRegexOptions, method) { }

        public async Task<bool> Process(SocketUserMessage message, SocketTextChannel channel, SocketGuildUser user)
        {
            if (_ignoreBots && (message.Author.IsBot || message.Author.IsWebhook))
                return false;

            Match match = _regex.Match(message.Content);
            if (match == null || !match.Success)
                return false;

            await _commandProcess(message, channel, user, match);
            return true;
        }

        public async Task<bool> Process(SocketMessage message)
        {
            var data = await ExtractVariables(message);
            if (data == null)
                return false;
            return await Process(data.Value.Message, data.Value.Channel, data.Value.User);
        }

        public static async Task<(SocketUserMessage Message, SocketTextChannel Channel, SocketGuildUser User)?> ExtractVariables(SocketMessage message)
        {
            if (!(message is SocketUserMessage msg))
                return null;
            if (!(msg.Channel is SocketTextChannel channel))
                return null;

            SocketGuildUser user = await channel.GetGuildUser(msg.Author.Id);
            if (user == null)
                return null;

            return (msg, channel, user);
        }
    }
}
