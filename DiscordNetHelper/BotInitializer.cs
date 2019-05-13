using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace TehGM.DiscordNetBot
{
    public class BotInitializer<TConfig> : IDisposable where TConfig : IBotConfig
    {
        /// <summary>Started client.</summary>
        /// <remarks>Note this property will be null until <see cref="StartClient"/> is invoked.</remarks>
        public DiscordSocketClient Client { get; private set; }
        /// <summary>Bot's config.</summary>
        public TConfig Config { get; }
        /// <summary>Handlers loaded.</summary>
        /// <remarks>Note this property will be null until <see cref="StartClient"/> is invoked.</remarks>
        public IList<HandlerBase<TConfig>> Handlers { get; private set; }

        /// <summary>Size of message cache.</summary>
        /// <remarks>Note that changing this property has no effect after <see cref="StartClient"/> has been invoked.</remarks>
        public int MessageCacheSize { get; set; }
        /// <summary>Should this instance automatically handle logging to console?</summary>
        public bool HandleLogs { get; set; } = true;
        /// <summary>Log level when a debugger is attached.</summary>
        /// <remarks>Note that changing this property has no effect after <see cref="StartClient"/> has been invoked.</remarks>
        public LogSeverity DebuggingLogLevel { get; set; } = LogSeverity.Debug;
        /// <summary>Log level when no debugger is attached.</summary>
        /// <remarks>Note that changing this property has no effect after <see cref="StartClient"/> has been invoked.</remarks>
        public LogSeverity ProductionLogLevel { get; set; } = LogSeverity.Info;
        /// <summary>Should all handlers in this assembly be automatically loaded?</summary>
        /// <remarks>Note that changing this property has no effect after <see cref="StartClient"/> has been invoked.</remarks>
        public bool AutoLoadHandlers { get; set; } = true;

        public BotInitializer(TConfig config)
        {
            this.Config = config;
        }

        /// <summary>Starts and connects the bot client.</summary>
        /// <remarks><para>If handlers weren't already loaded and <see cref="AutoLoadHandlers"/> is set to true, this method will load all handlers in assembly automatically.</para></remarks>
        /// <returns>Discord socket client instance.</returns>
        public virtual async Task<DiscordSocketClient> StartClient()
        {
            if (Client != null)
                throw new InvalidOperationException($"Client is already started. Use a new instance of {nameof(BotInitializer<TConfig>)} for new clients.");
            DiscordSocketConfig clientConfig = new DiscordSocketConfig();
            clientConfig.WebSocketProvider = Discord.Net.WebSockets.DefaultWebSocketProvider.Instance;
            clientConfig.LogLevel = Debugger.IsAttached ? DebuggingLogLevel : ProductionLogLevel;
            clientConfig.MessageCacheSize = this.MessageCacheSize;
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
            Client?.Dispose();
        }
    }
}
