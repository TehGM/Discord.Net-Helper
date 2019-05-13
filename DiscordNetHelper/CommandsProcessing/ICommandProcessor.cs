using Discord.WebSocket;
using System.Threading.Tasks;

namespace TehGM.DiscordNetBot.CommandsProcessing
{
    public interface ICommandProcessor
    {
        /// <summary>Processes the message, executing command's callback if all checks pass.</summary>
        /// <param name="client">Discord socket client that received the message.</param>
        /// <param name="message">Actual message received.</param>
        /// <returns>True if all checks passed; otherwise false.</returns>
        Task<bool> ProcessAsync(DiscordSocketClient client, SocketMessage message);
    }
}
