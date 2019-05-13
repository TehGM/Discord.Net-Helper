using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using TehGM.DiscordBot.CommandsProcessing;
using TehGM.DiscordBot.Config;
using TehGM.DiscordBot.Extensions;

namespace TehGM.DiscordBot
{
    public class BotInitializer<TConfig> : IDisposable where TConfig : IBotConfig
    {
        public DiscordSocketClient Client { get; private set; }
        public TConfig Config { get; private set; }
        public IList<HandlerBase<TConfig>> Handlers { get; private set; }

        public bool HandleLogs { get; set; } = true;
        public LogSeverity DebuggingLogLevel { get; set; } = LogSeverity.Debug;
        public LogSeverity ProductionLogLevel { get; set; } = LogSeverity.Info;
        public bool AutoLoadHandlers { get; set; } = true;

        public BotInitializer(TConfig config)
        {
            this.Config = config;
        }

        public virtual async Task<DiscordSocketClient> StartClient()
        {
            DiscordSocketConfig clientConfig = new DiscordSocketConfig();
            clientConfig.WebSocketProvider = Discord.Net.WebSockets.DefaultWebSocketProvider.Instance;
            clientConfig.LogLevel = Debugger.IsAttached ? DebuggingLogLevel : ProductionLogLevel;
            Client = new DiscordSocketClient(clientConfig);

            Client.Log += Client_Log;

            if (AutoLoadHandlers && Handlers == null)
                Handlers = InitializeHandlers(Client, Config);

            await Client.LoginAsync(TokenType.Bot, Config.Auth.Token);
            await Client.StartAsync();
            return Client;
        }

        private Task Client_Log(LogMessage arg)
        {
            if (HandleLogs)
                Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        public static IList<HandlerBase<TConfig>> InitializeHandlers(DiscordSocketClient client, TConfig config)
        {
            Type[] types = Assembly.GetEntryAssembly().FindDerivedTypes(typeof(HandlerBase<TConfig>));
            List<HandlerBase<TConfig>> handlers = new List<HandlerBase<TConfig>>(types.Length);

            for (int i = 0; i < types.Length; i++)
            {
                Type t = types[i];
                if (Debugger.IsAttached && Attribute.IsDefined(t, typeof(ProductionOnlyAttribute)))
                    continue;
                HandlerBase<TConfig> handler = (HandlerBase<TConfig>)Activator.CreateInstance(t, client, config);
                handlers.Add(handler);
            }

            return handlers;
        }

        public virtual void Dispose()
        {
            Client.Log -= Client_Log;
        }
    }
}
