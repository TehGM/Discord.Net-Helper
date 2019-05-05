using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TehGM.DiscordNetHelper.CommandsProcessing
{
    public static class CommandsProcessingExtensions
    {
        public static async Task<bool> ProcessUntilFirstMatching<T>(this IEnumerable<T> commandsStack, SocketMessage message) where T : ICommandProcessor
        {
            foreach (var prc in commandsStack)
            {
                if (await prc.Process(message))
                    return true;
            }
            return false;
        }

        public static async Task<bool> ProcessUntilFirstMatching(this IEnumerable<GuildUserCommandProcessor> commandsStack, SocketMessage message)
        {
            var data = await GuildUserCommandProcessor.ExtractVariables(message);
            if (data == null)
                return false;
            return await commandsStack.ProcessUntilFirstMatching(data.Value.Message, data.Value.Channel, data.Value.User);
        }

        public static async Task<bool> ProcessUntilFirstMatching(this IEnumerable<GuildUserCommandProcessor> commandsStack,
            SocketUserMessage message, SocketTextChannel channel, SocketGuildUser user)
        {
            foreach (var prc in commandsStack)
            {
                if (await prc.Process(message, channel, user))
                    return true;
            }
            return false;
        }
    }
}
