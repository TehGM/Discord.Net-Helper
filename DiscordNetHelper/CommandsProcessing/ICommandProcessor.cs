using Discord.WebSocket;
using System.Threading.Tasks;

namespace TehGM.DiscordBot.CommandsProcessing
{
    public interface ICommandProcessor
    {
        Task<bool> ProcessAsync(DiscordSocketClient client, SocketMessage message);
    }
}
