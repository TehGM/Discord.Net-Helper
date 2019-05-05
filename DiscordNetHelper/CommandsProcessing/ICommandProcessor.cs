using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TehGM.DiscordNetHelper.CommandsProcessing
{
    public interface ICommandProcessor
    {
        Task<bool> Process(SocketMessage message);
    }
}
