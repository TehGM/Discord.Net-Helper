using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TehGM.DiscordNetHelper.Config
{
    public class BotAuth : IDisposable, IBotAuth
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        public void Dispose()
        {
            this.Token = null;
        }
    }
}
