using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TehGM.DiscordNetHelper.Config
{
    public interface IBotData<TGuildData>
    {
        IList<TGuildData> GuildsData { get; }

        TGuildData GetGuildData(ulong guildID);
        TGuildData GetOrCreateGuildData(ulong guildID);
        Task SaveAsync(string filepath);
    }
}
