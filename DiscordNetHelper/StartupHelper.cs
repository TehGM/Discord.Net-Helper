using Discord;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Net.WebSockets;
using System.Collections.Generic;
using System.Reflection;
using TehGM.DiscordNetHelper.Config;

namespace TehGM.DiscordNetHelper
{
    public static class StartupHelper
    {
        public static DiscordSocketClient CreateClient(bool enableConsoleLogging = true)
        {
            DiscordSocketConfig clientConfig = new DiscordSocketConfig();
            clientConfig.WebSocketProvider = DefaultWebSocketProvider.Instance;
            clientConfig.LogLevel = Debugger.IsAttached ? LogSeverity.Debug : LogSeverity.Info;
            DiscordSocketClient client = new DiscordSocketClient(clientConfig);

            if (enableConsoleLogging)
                client.Log += Client_Log;

            return client;
        }

        public static async Task StartClient(BaseSocketClient client, IBotConfig config)
        {
            await client.LoginAsync(TokenType.Bot, config.Auth.Token);
            await client.StartAsync();
        }

        private static Task Client_Log(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        public static IEnumerable<HandlerBase<TBotConfig>> InitializeHandlers<TBotConfig>(DiscordSocketClient client, TBotConfig config) where TBotConfig : IBotConfig
        {
            Type[] types = Assembly.GetExecutingAssembly().FindDerivedTypes(typeof(HandlerBase<TBotConfig>));
            List<HandlerBase<TBotConfig>> handlers = new List<HandlerBase<TBotConfig>>(types.Length);

            for (int i = 0; i < types.Length; i++)
            {
                HandlerBase<TBotConfig> handler = (HandlerBase<TBotConfig>)Activator.CreateInstance(types[i], client, config);
                handlers.Add(handler);
            }

            return handlers;
        }
    }
}
