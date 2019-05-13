using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace TehGM.DiscordNetBot
{
    public class BotAuth : IBotAuth, IDisposable
    {
        public static string DefaultPath { get; set; } = "Config/auth.json";

        [JsonProperty("token", Required = Required.Always)]
        public string Token { get; private set; }

        public static async Task<BotAuth> LoadAsync(string filePath)
        {
            JToken fileContents = await JsonFileExtensions.LoadFromFileAsync(filePath);
            return fileContents.ToObject<BotAuth>();
        }

        public static Task<BotAuth> LoadAsync()
            => LoadAsync(DefaultPath);

        public void Dispose()
        {
            this.Token = null;
        }
    }
}
