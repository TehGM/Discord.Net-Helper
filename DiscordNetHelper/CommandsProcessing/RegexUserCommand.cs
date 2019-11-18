using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TehGM.DiscordNetBot
{
    public class RegexUserCommand : ICommandProcessor
    {
        /// <summary>Default regex options when creating a command.</summary>
        /// <remarks>This is used only by constructors that take string pattern instead of <see cref="Regex"/> instance.</remarks>
        public static RegexOptions DefaultRegexOptions { get; set; } = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline;
        /// <summary>Default verifier to use when it's not explicitly provided in constructor.</summary>
        public static ICommandVerifier DefaultVerifier => CommandVerifier.DefaultPrefixed;

        private readonly Regex _regex;
        private readonly Func<SocketCommandContext, Match, Task> _method;
        private readonly ICommandVerifier _verifier;

        /// <summary>Creates a new command instance.</summary>
        /// <param name="verifier">Verifier to use when processing messages.</param>
        /// <param name="regex">Regex pattern the message should match after being stripped of prefixes by verifier.</param>
        /// <param name="method">Callback to execute if message passes all checks when processing.</param>
        public RegexUserCommand(Regex regex, Func<SocketCommandContext, Match, Task> method, ICommandVerifier verifier)
        {
            this._verifier = verifier;
            this._regex = regex;
            this._method = method;
        }
        /// <summary>Creates a new command instance using a default verifier.</summary>
        /// <param name="regex">Regex pattern the message should match after being stripped of prefixes by verifier.</param>
        /// <param name="method">Callback to execute if message passes all checks when processing.</param>
        public RegexUserCommand(Regex regex, Func<SocketCommandContext, Match, Task> method)
            : this(regex, method, DefaultVerifier) { }
        /// <summary>Creates a new command instance.</summary>
        /// <param name="verifier">Verifier to use when processing messages.</param>
        /// <param name="pattern">Regex pattern the message should match after being stripped of prefixes by verifier.</param>
        /// <param name="options">Regex options for created Regex instance.</param>
        /// <param name="method">Callback to execute if message passes all checks when processing.</param>
        public RegexUserCommand(string pattern, RegexOptions options, Func<SocketCommandContext, Match, Task> method, ICommandVerifier verifier)
            : this(new Regex(pattern, options), method, verifier) { }
        /// <summary>Creates a new command instance.</summary>
        /// <param name="verifier">Verifier to use when processing messages.</param>
        /// <param name="pattern">Regex pattern the message should match after being stripped of prefixes by verifier.</param>
        /// <param name="method">Callback to execute if message passes all checks when processing.</param>
        public RegexUserCommand(string pattern, Func<SocketCommandContext, Match, Task> method, ICommandVerifier verifier)
            : this(pattern, DefaultRegexOptions, method, verifier) { }
        /// <summary>Creates a new command instance.</summary>
        /// <param name="pattern">Regex pattern the message should match after being stripped of prefixes by verifier.</param>
        /// <param name="options">Regex options for created Regex instance.</param>
        /// <param name="method">Callback to execute if message passes all checks when processing.</param>
        public RegexUserCommand(string pattern, RegexOptions options, Func<SocketCommandContext, Match, Task> method)
            : this(new Regex(pattern, options), method, DefaultVerifier) { }
        /// <summary>Creates a new command instance using a default verifier.</summary>
        /// <param name="pattern">Regex pattern the message should match after being stripped of prefixes by verifier.</param>
        /// <param name="method">Callback to execute if message passes all checks when processing.</param>
        public RegexUserCommand(string pattern, Func<SocketCommandContext, Match, Task> method)
            : this(pattern, DefaultRegexOptions, method) { }

        /// <inheritdoc/>
        public virtual async Task<bool> ProcessAsync(DiscordSocketClient client, SocketMessage message)
        {
            if (!(message is SocketUserMessage msg))
                return false;
            SocketCommandContext ctx = new SocketCommandContext(client, msg);
            if (!_verifier.Verify(ctx, out string cmd))
                return false;

            Match match = _regex.Match(cmd);
            if (match == null || !match.Success)
                return false;

            await _method.Invoke(ctx, match);
            return true;
        }
    }
}
