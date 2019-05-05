using Discord;
using Discord.WebSocket;
using System;
using TehGM.DiscordNetHelper.Config;
using System.Threading.Tasks;

namespace TehGM.DiscordNetHelper
{
    public abstract class HandlerBase<TBotConfig> : IDisposable where TBotConfig : IBotConfig
    {
        protected DiscordSocketClient Client { get; private set; }
        protected TBotConfig Config { get; }

        private SocketUser _authorUser;

        public SocketUser AuthorUser
        {
            get
            {
                if (_authorUser == null)
                    _authorUser = Client.GetUser(Config.AuthorID);
                return _authorUser;
            }
        }

        public HandlerBase(DiscordSocketClient client, TBotConfig config)
        {
            this.Client = client;
            this.Config = config;

            Client.MessageReceived += Client_MessageReceived;
            Client.ReactionAdded += Client_ReactionAdded;
            Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
            // todo: add other event handlers as they become needed.

            Console.WriteLine($"{this.GetType().Name} initialized.");
        }

        protected virtual Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnMessageReceived(SocketMessage message)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            return Task.CompletedTask;
        }

        private Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            Task.Run(() => OnUserVoiceStateUpdated(user, oldState, newState));
            return Task.CompletedTask;
        }

        private Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            Task.Run(() => OnReactionAdded(message, channel, reaction));
            return Task.CompletedTask;
        }

        private Task Client_MessageReceived(SocketMessage message)
        {
            Task.Run(() => OnMessageReceived(message));
            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            if (Client != null)
            {
                Client.MessageReceived -= Client_MessageReceived;
                Client.ReactionAdded -= Client_ReactionAdded;
                Client.UserVoiceStateUpdated -= Client_UserVoiceStateUpdated;
                Client = null;
            }
        }
    }
}
