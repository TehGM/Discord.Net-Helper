using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TehGM.DiscordNetHelper.Config
{
    public abstract class BotDataBase<TGuildData> : IBotData<TGuildData> where TGuildData : GuildDataBase
    {
        [JsonProperty("guildsData")]
        public IList<TGuildData> GuildsData { get; set; }

        public virtual TGuildData GetGuildData(ulong guildID)
            => GuildsData?.FirstOrDefault(g => g.GuildID == guildID);

        public virtual TGuildData GetOrCreateGuildData(ulong guildID)
        {
            TGuildData gdata = GetGuildData(guildID);
            if (gdata != null)
                return gdata;
            if (GuildsData == null)
                GuildsData = new List<TGuildData>(1);
            gdata = (TGuildData)Activator.CreateInstance(typeof(TGuildData), guildID);
            GuildsData.Add(gdata);
            return gdata;
        }

        public static async Task<T> LoadAsync<T>(string filepath) where T : BotDataBase<TGuildData>
        {
            if (typeof(T).IsAbstract)
                throw new TypeArgumentException($"{nameof(LoadAsync)} does not accept an abstract type {nameof(T)}.");
            using (StreamReader file = File.OpenText(filepath))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject obj = await JObject.LoadAsync(reader);
                return obj.ToObject<T>();
            }
        }

        public abstract Task SaveAsync(string filepath);
        protected static async Task SaveInternalAsync<T>(string filepath, T instance) where T : BotDataBase<TGuildData>
        {
            if (typeof(T).IsAbstract)
                throw new TypeArgumentException($"{nameof(SaveAsync)} does not accept an abstract type {nameof(T)}.");
            using (FileStream file = File.Create(filepath))
            using (StreamWriter wr = new StreamWriter(file))
            using (JsonTextWriter writer = new JsonTextWriter(wr))
            {
                JObject obj = JObject.FromObject(instance);
                await obj.WriteToAsync(writer);
            }
        }

        public static Task<T> LoadAsync<T>() where T : BotDataBase<TGuildData>
            => LoadAsync<T>(ConfigExtensions.DefaultDataPath);
    }
}
