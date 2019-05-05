using System;
using System.Collections.Generic;
using System.Text;

namespace TehGM.DiscordNetHelper.Config
{
    public interface IBotConfig
    {
        BotAuth Auth { get; }
        ulong AuthorID { get; }
        void ClearAuth();
    }
}
