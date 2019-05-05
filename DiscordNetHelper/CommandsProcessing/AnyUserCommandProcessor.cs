using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TehGM.DiscordNetHelper.CommandsProcessing
{
    public class AnyUserCommandProcessor : ICommandProcessor
    {
        public const RegexOptions DefaultRegexOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline;

        private readonly Regex _regex;
        private readonly Func<SocketUserMessage, Match, Task> _commandProcess;
        private readonly bool _ignoreBots;

        public AnyUserCommandProcessor(Regex regex, Func<SocketUserMessage, Match, Task> method, bool ignoreBots = true)
        {
            this._regex = regex;
            this._commandProcess = method;
            this._ignoreBots = ignoreBots;
        }

        public AnyUserCommandProcessor(string pattern, RegexOptions options, Func<SocketUserMessage, Match, Task> method, bool ignoreBots = true)
            : this(new Regex(pattern, options), method, ignoreBots) { }
        public AnyUserCommandProcessor(string pattern, Func<SocketUserMessage, Match, Task> method, bool ignoreBots = true)
            : this(pattern, AnyUserCommandProcessor.DefaultRegexOptions, method, ignoreBots) { }

        public async Task<bool> Process(SocketMessage message)
        {
            if (!(message is SocketUserMessage msg))
                return false;
            if (_ignoreBots && (msg.Author.IsBot || msg.Author.IsWebhook))
                return false;

            Match match = _regex.Match(message.Content);
            if (match == null || !match.Success)
                return false;

            await _commandProcess.Invoke(msg, match);
            return true;
        }
    }
}
