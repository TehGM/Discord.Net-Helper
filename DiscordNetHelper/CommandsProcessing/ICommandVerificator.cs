using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace TehGM.DiscordNetBot
{
    public interface ICommandVerificator
    {
        /// <summary>Verifies if the message matches specified rules set.</summary>
        /// <param name="command">Message received.</param>
        /// <param name="actualCommand">The command stripped of any prefixes.</param>
        /// <returns>True if <paramref name="command"/> matches specified rules; otherwise false.</returns>
        bool Verify(SocketCommandContext command, out string actualCommand);
    }
}
