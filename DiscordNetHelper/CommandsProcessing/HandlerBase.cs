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

            // note: maintain alphabetic order for easy maintenance
            Client.ChannelCreated += Client_ChannelCreated;
            Client.ChannelDestroyed += Client_ChannelDestroyed;
            Client.ChannelUpdated += Client_ChannelUpdated;
            Client.Connected += Client_Connected;
            Client.CurrentUserUpdated += Client_CurrentUserUpdated;
            Client.Disconnected += Client_Disconnected;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.GuildMembersDownloaded += Client_GuildMembersDownloaded;
            Client.GuildMemberUpdated += Client_GuildMemberUpdated;
            Client.GuildUpdated += Client_GuildUpdated;
            Client.JoinedGuild += Client_JoinedGuild;
            Client.LatencyUpdated += Client_LatencyUpdated;
            Client.LeftGuild += Client_LeftGuild;
            Client.LoggedIn += Client_LoggedIn;
            Client.LoggedOut += Client_LoggedOut;
            Client.MessageDeleted += Client_MessageDeleted;
            Client.MessageReceived += Client_MessageReceived;
            Client.MessageUpdated += Client_MessageUpdated;
            Client.ReactionAdded += Client_ReactionAdded;
            Client.ReactionRemoved += Client_ReactionRemoved;
            Client.ReactionsCleared += Client_ReactionsCleared;
            Client.Ready += Client_Ready;
            Client.RecipientAdded += Client_RecipientAdded;
            Client.RecipientRemoved += Client_RecipientRemoved;
            Client.RoleCreated += Client_RoleCreated;
            Client.RoleDeleted += Client_RoleDeleted;
            Client.RoleUpdated += Client_RoleUpdated;
            Client.UserBanned += Client_UserBanned;
            Client.UserIsTyping += Client_UserIsTyping;
            Client.UserJoined += Client_UserJoined;
            Client.UserLeft += Client_UserLeft;
            Client.UserUnbanned += Client_UserUnbanned;
            Client.UserUpdated += Client_UserUpdated;
            Client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
            Client.VoiceServerUpdated += Client_VoiceServerUpdated;

            Console.WriteLine($"{this.GetType().Name} initialized.");
        }

        #region VIRTUAL EVENT HANDLERS
        protected virtual async Task OnMessageReceived(SocketMessage message)
        {
            for (int i = 0; i < CommandsStack.Count; i++)
            {
                if (await CommandsStack[i].ProcessAsync(Client, message))
                    return;
            }
        }
        protected virtual Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState previousState, SocketVoiceState nextState)
            => Task.CompletedTask;
        protected virtual Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
            => Task.CompletedTask;
        protected virtual Task OnMessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
            => Task.CompletedTask;
        protected virtual Task OnVoiceServerUpdated(SocketVoiceServer server)
            => Task.CompletedTask;
        protected virtual Task OnUserUnbanned(SocketUser user, SocketGuild guild)
            => Task.CompletedTask;
        protected virtual Task OnRoleUpdated(SocketRole roleBefore, SocketRole roleAfter)
            => Task.CompletedTask;
        protected virtual Task OnRoleDeleted(SocketRole role)
            => Task.CompletedTask;
        protected virtual Task OnRoleCreated(SocketRole role)
            => Task.CompletedTask;
        protected virtual Task OnRecipientRemoved(SocketGroupUser user)
            => Task.CompletedTask;
        protected virtual Task OnRecipientAdded(SocketGroupUser user)
            => Task.CompletedTask;
        protected virtual Task OnReady()
            => Task.CompletedTask;
        protected virtual Task OnReactionsCleared(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel)
            => Task.CompletedTask;
        protected virtual Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
            => Task.CompletedTask;
        protected virtual Task OnMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
            => Task.CompletedTask;
        protected virtual Task OnLoggedOut()
            => Task.CompletedTask;
        protected virtual Task OnLoggedIn()
            => Task.CompletedTask;
        protected virtual Task OnLeftFuild(SocketGuild guild)
            => Task.CompletedTask;
        protected virtual Task OnLatencyUpdated(int previousValue, int newValue)
            => Task.CompletedTask;
        protected virtual Task OnJoinedGuild(SocketGuild guild)
            => Task.CompletedTask;
        protected virtual Task OnGuildUpdated(SocketGuild guildBefore, SocketGuild guildAfter)
            => Task.CompletedTask;
        protected virtual Task OnGuildMemberUpdated(SocketGuildUser userBefore, SocketGuildUser userAfter)
            => Task.CompletedTask;
        protected virtual Task OnGuildMembersDownloaded(SocketGuild guild)
            => Task.CompletedTask;
        protected virtual Task OnGuildAvailable(SocketGuild guild)
            => Task.CompletedTask;
        protected virtual Task OnDisconnected(Exception exception)
            => Task.CompletedTask;
        protected virtual Task OnCurrentUserUpdated(SocketSelfUser userBefore, SocketSelfUser userAfter)
            => Task.CompletedTask;
        protected virtual Task OnConnected()
            => Task.CompletedTask;
        protected virtual Task OnChannelUpdated(SocketChannel channelBefore, SocketChannel channelAfter)
            => Task.CompletedTask;
        protected virtual Task OnChannelDestroyed(SocketChannel channel)
            => Task.CompletedTask;
        protected virtual Task OnChannelCreated(SocketChannel channel)
            => Task.CompletedTask;
        protected virtual Task OnUserIsTyping(SocketUser user, ISocketMessageChannel channel)
            => Task.CompletedTask;
        protected virtual Task OnUserBanned(SocketUser user, SocketGuild guild)
            => Task.CompletedTask;
        protected virtual Task OnUserUpdated(SocketUser userBefore, SocketUser userAfter)
            => Task.CompletedTask;
        protected virtual Task OnUserLeft(SocketGuildUser user)
            => Task.CompletedTask;
        protected virtual Task OnUserJoined(SocketGuildUser user)
            => Task.CompletedTask;
        #endregion

        #region EVENT RELAYS
        private Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState previousState, SocketVoiceState nextState)
            => InvokeTask(() => OnUserVoiceStateUpdated(user, previousState, nextState));
        private Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
            => InvokeTask(() => OnReactionAdded(message, channel, reaction));
        private Task Client_MessageReceived(SocketMessage message)
            => InvokeTask(() => OnMessageReceived(message));
        private Task Client_MessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel)
            => InvokeTask(() => OnMessageUpdated(messageBefore, messageAfter, channel));
        private Task Client_VoiceServerUpdated(SocketVoiceServer arg)
            => InvokeTask(() => OnVoiceServerUpdated(arg));
        private Task Client_UserUnbanned(SocketUser arg1, SocketGuild arg2)
            => InvokeTask(() => OnUserUnbanned(arg1, arg2));
        private Task Client_RoleUpdated(SocketRole arg1, SocketRole arg2)
            => InvokeTask(() => OnRoleUpdated(arg1, arg2));
        private Task Client_RoleDeleted(SocketRole arg)
            => InvokeTask(() => OnRoleDeleted(arg));
        private Task Client_RoleCreated(SocketRole arg)
            => InvokeTask(() => OnRoleCreated(arg));
        private Task Client_RecipientRemoved(SocketGroupUser arg)
            => InvokeTask(() => OnRecipientRemoved(arg));
        private Task Client_RecipientAdded(SocketGroupUser arg)
            => InvokeTask(() => OnRecipientAdded(arg));
        private Task Client_Ready()
            => InvokeTask(OnReady);
        private Task Client_ReactionsCleared(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2)
            => InvokeTask(() => OnReactionsCleared(arg1, arg2));
        private Task Client_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
            => InvokeTask(() => OnReactionRemoved(arg1, arg2, arg3));
        private Task Client_MessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
            => InvokeTask(() => OnMessageDeleted(arg1, arg2));
        private Task Client_LoggedOut()
            => InvokeTask(OnLoggedOut);
        private Task Client_LoggedIn()
            => InvokeTask(OnLoggedIn);
        private Task Client_LeftGuild(SocketGuild arg)
            => InvokeTask(() => OnLeftFuild(arg));
        private Task Client_LatencyUpdated(int arg1, int arg2)
            => InvokeTask(() => OnLatencyUpdated(arg1, arg2));
        private Task Client_JoinedGuild(SocketGuild arg)
            => InvokeTask(() => OnJoinedGuild(arg));
        private Task Client_GuildUpdated(SocketGuild arg1, SocketGuild arg2)
            => InvokeTask(() => OnGuildUpdated(arg1, arg2));
        private Task Client_GuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
            => InvokeTask(() => OnGuildMemberUpdated(arg1, arg2));
        private Task Client_GuildMembersDownloaded(SocketGuild arg)
            => InvokeTask(() => OnGuildMembersDownloaded(arg));
        private Task Client_GuildAvailable(SocketGuild arg)
            => InvokeTask(() => OnGuildAvailable(arg));
        private Task Client_Disconnected(Exception arg)
            => InvokeTask(() => OnDisconnected(arg));
        private Task Client_CurrentUserUpdated(SocketSelfUser arg1, SocketSelfUser arg2)
            => InvokeTask(() => OnCurrentUserUpdated(arg1, arg2));
        private Task Client_Connected()
            => InvokeTask(OnConnected);
        private Task Client_ChannelUpdated(SocketChannel arg1, SocketChannel arg2)
            => InvokeTask(() => OnChannelUpdated(arg1, arg2));
        private Task Client_ChannelDestroyed(SocketChannel arg)
            => InvokeTask(() => OnChannelDestroyed(arg));
        private Task Client_ChannelCreated(SocketChannel arg)
            => InvokeTask(() => OnChannelCreated(arg));
        private Task Client_UserIsTyping(SocketUser arg1, ISocketMessageChannel arg2)
            => InvokeTask(() => OnUserIsTyping(arg1, arg2));
        private Task Client_UserBanned(SocketUser arg1, SocketGuild arg2)
            => InvokeTask(() => OnUserBanned(arg1, arg2));
        private Task Client_UserUpdated(SocketUser arg1, SocketUser arg2)
            => InvokeTask(() => OnUserUpdated(arg1, arg2));
        private Task Client_UserLeft(SocketGuildUser arg)
            => InvokeTask(() => OnUserLeft(arg));
        private Task Client_UserJoined(SocketGuildUser arg)
            => InvokeTask(() => OnUserJoined(arg));

        private Task InvokeTask(Func<Task> task)
        {
            if (SwitchTaskContext)
            {
                Task.Run(task);
                return Task.CompletedTask;
            }
            return task.Invoke();
        }
        #endregion

        public virtual void Dispose()
        {
            if (Client != null)
            {
                // note: maintain alphabetic order for easy maintenance
                Client.ChannelCreated -= Client_ChannelCreated;
                Client.ChannelDestroyed -= Client_ChannelDestroyed;
                Client.ChannelUpdated -= Client_ChannelUpdated;
                Client.Connected -= Client_Connected;
                Client.CurrentUserUpdated -= Client_CurrentUserUpdated;
                Client.Disconnected -= Client_Disconnected;
                Client.GuildAvailable -= Client_GuildAvailable;
                Client.GuildMembersDownloaded -= Client_GuildMembersDownloaded;
                Client.GuildMemberUpdated -= Client_GuildMemberUpdated;
                Client.GuildUpdated -= Client_GuildUpdated;
                Client.JoinedGuild -= Client_JoinedGuild;
                Client.LatencyUpdated -= Client_LatencyUpdated;
                Client.LeftGuild -= Client_LeftGuild;
                Client.LoggedIn -= Client_LoggedIn;
                Client.LoggedOut -= Client_LoggedOut;
                Client.MessageDeleted -= Client_MessageDeleted;
                Client.MessageReceived -= Client_MessageReceived;
                Client.MessageUpdated -= Client_MessageUpdated;
                Client.ReactionAdded -= Client_ReactionAdded;
                Client.ReactionRemoved -= Client_ReactionRemoved;
                Client.ReactionsCleared -= Client_ReactionsCleared;
                Client.Ready -= Client_Ready;
                Client.RecipientAdded -= Client_RecipientAdded;
                Client.RecipientRemoved -= Client_RecipientRemoved;
                Client.RoleCreated -= Client_RoleCreated;
                Client.RoleDeleted -= Client_RoleDeleted;
                Client.RoleUpdated -= Client_RoleUpdated;
                Client.UserBanned -= Client_UserBanned;
                Client.UserIsTyping -= Client_UserIsTyping;
                Client.UserJoined -= Client_UserJoined;
                Client.UserLeft -= Client_UserLeft;
                Client.UserUnbanned -= Client_UserUnbanned;
                Client.UserUpdated -= Client_UserUpdated;
                Client.UserVoiceStateUpdated -= Client_UserVoiceStateUpdated;
                Client.VoiceServerUpdated -= Client_VoiceServerUpdated;
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
