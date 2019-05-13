using System;
using System.Collections.Generic;
using System.Text;

namespace TehGM.DiscordBot.Config
{
    public interface IBotConfig
    {
        ulong AuthorID { get; }
        BotAuth Auth { get; }
    }
}
