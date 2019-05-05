using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TehGM.DiscordNetHelper.Config
{
    public class DefaultBotConfig : IBotConfig
    {
        protected const ulong DefaultAuthorID = 247081094799687682;

        [JsonProperty("auth", Required = Required.Always)]
        public BotAuth Auth { get; protected set; }
        [DefaultValue(DefaultAuthorID)]
        [JsonProperty("authorId", DefaultValueHandling = DefaultValueHandling.Populate)]
        public ulong AuthorID { get; protected set; } = DefaultAuthorID;

        public static async Task<DefaultBotConfig> LoadAsDefaultAsync(string filepath)
        {
            using (StreamReader file = File.OpenText(filepath))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject obj = await JObject.LoadAsync(reader);
                return obj.ToObject<DefaultBotConfig>();
            }
        }

        public static Task<DefaultBotConfig> LoadAsDefaultAsync()
            => LoadAsDefaultAsync("botconfig.json");

        public virtual void ClearAuth()
        {
            this.Auth.Dispose();
            this.Auth = null;
        }
    }
}
