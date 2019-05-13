using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TehGM.DiscordNetBot.Config;

namespace TehGM.DiscordNetBot.CommandsProcessing
{
    public abstract class HandlerBase<TConfig> : IDisposable where TConfig : IBotConfig
    {
        /// <summary>Discord socket client for this handler instance.</summary>
        protected DiscordSocketClient Client { get; private set; }
        /// <summary>Config for the bot.</summary>
        protected TConfig Config { get; }
        /// <summary>Should overridable methods be invoked on a separate task?</summary>
        public bool SwitchTaskContext { get; set; } = true;
        /// <summary>Stack of commands.</summary>
        protected IList<ICommandProcessor> CommandsStack { get; set; }

        // when handler is constructed, the client might not be connected yet
        // so delay init to first access attempt
        private SocketUser _authorUser;
        /// <summary>Instance of user which is the bot's author.</summary>
        /// <remarks><para>This value may throw exceptions if the bot is not connected to Discord Socket.</para>
        /// <para>Requires a valid user ID to be provided in bot config.</para></remarks>
        public SocketUser AuthorUser
        {
            get
            {
                if (_authorUser == null)
                    _authorUser = Client.GetUser(Config.AuthorID);
                return _authorUser;
            }
        }

        /// <summary>Initializes the handler.</summary>
        /// <param name="client">Discord socket client for this handler instance.</param>
        /// <param name="config">Bot's config.</param>
        public HandlerBase(DiscordSocketClient client, TConfig config)
        {
            this.Client = client;
            this.Config = config;
            this.CommandsStack = new List<ICommandProcessor>();

            Client.MessageReceived += Client_MessageReceived;
            Client.ReactionAdded += Client_ReactionAdded;
            Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
            // TODO: more event handlers as they become needed

            Console.WriteLine($"{this.GetType().Name} initialized.");
        }

        protected virtual Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            return Task.CompletedTask;
        }

        protected virtual async Task OnMessageReceived(SocketMessage message)
        {
            for (int i = 0; i < CommandsStack.Count; i++)
            {
                if (await CommandsStack[i].ProcessAsync(Client, message))
                    return;
            }
        }

        protected virtual Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState previousState, SocketVoiceState nextState)
        {
            return Task.CompletedTask;
        }

        // relaying events to children
        private Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState previousState, SocketVoiceState nextState)
            => InvokeTask(() => OnUserVoiceStateUpdated(user, previousState, nextState));
        private Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
            => InvokeTask(() => OnReactionAdded(message, channel, reaction));
        private Task Client_MessageReceived(SocketMessage message)
            => InvokeTask(() => OnMessageReceived(message));

        private Task InvokeTask(Func<Task> task)
        {
            if (SwitchTaskContext)
            {
                Task.Run(task);
                return Task.CompletedTask;
            }
            return task.Invoke();
        }

        public virtual void Dispose()
        {
            if (Client != null)
            {
                Client.MessageReceived -= Client_MessageReceived;
                Client.ReactionAdded -= Client_ReactionAdded;
                Client = null;
            }
        }

        /// <summary>Gets default prefix from the default <see cref="CommandVerificator"/>.</summary>
        /// <returns>String representing default bot's prefix.</returns>
        public string GetDefaultPrefix()
        {
            string prefix = (CommandVerificator.DefaultPrefixed as CommandVerificator).StringPrefix;
            if (prefix == null)
                prefix = Client.CurrentUser.Mention;
            return prefix;
        }
    }
}
