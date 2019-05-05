using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TehGM.DiscordNetHelper.Config;

namespace TehGM.DiscordNetHelper
{
    public static class ConfigExtensions
    {
        public const string DefaultDataPath = "botdata.json";

        public static Task SaveAsync<TGuildData>(this IBotData<TGuildData> data)
            => data.SaveAsync(DefaultDataPath);

        public static TGuildData GetGuildData<TGuildData>(this IBotData<TGuildData> data, IGuild guild)
            => data.GetGuildData(guild.Id);

        public static TGuildData GetOrCreateGuildData<TGuildData>(this IBotData<TGuildData> data, IGuild guild)
            => data.GetOrCreateGuildData(guild.Id);
    }
}
