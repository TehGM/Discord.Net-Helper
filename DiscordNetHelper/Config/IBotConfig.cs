﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TehGM.DiscordNetBot.Config
{
    public interface IBotConfig
    {
        ulong AuthorID { get; }
        IBotAuth Auth { get; }
    }
}
