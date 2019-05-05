using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TehGM.DiscordNetHelper.Config
{
    public abstract class GuildDataBase : IGuildData
    {
        [JsonProperty("guildId", Required = Required.Always)]
        public ulong GuildID { get; private set; }

        protected internal GuildDataBase(ulong guildId)
        {
            this.GuildID = guildId;
        }
    }
}
