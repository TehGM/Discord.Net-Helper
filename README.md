# Discord.NET Helper
This is a library I developed for my own bot projects that use [Discord.NET](https://github.com/discord-net/Discord.Net). The library contains shared bot startup and commands handling code.

The main idea of this library is to provide myself some patterns I like and RegEx based commands system on top of [Discord.NET](https://github.com/discord-net/Discord.Net).

>It's important that I point out this library is created for personal purposes, and I am only sharing it cause why not. As such, it may be imperfect in many regards, or change a lot without warning over the time as I explore patterns or C# deeper, or simply get new ideas. I also will likely not implement any requested features unless I personally like them - however you are welcome to fork this repository if you wish.

## Bot Config
Most of the functionalities of the library require bot to have a config. Config can contain whatever you want, but the config class needs to implement [IBotConfig](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/Config/IBotConfig.cs) interface, which requires to contain 2 things at least: [IBotAuth](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/Config/IBotAuth.cs) and Author ID. The actual implementation can vary depending on the requirements of the bot, but library contains a simple default [BotAuth](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/Config/BotAuth.cs) class that by default loads auth from `/Config/auth.json` file.

Example bot config:
```csharp
public class BotConfig : IBotConfig
{
    public const string DefaultPath = "Config/config.json";

    // auth
    [JsonIgnore]
    public IBotAuth Auth { get; private set; }
    // bot's custom runtime data which can be saved at runtime
    [JsonIgnore]
    public BotData Data { get; private set; }

    [JsonProperty("authorId")]
    public ulong AuthorID { get; private set; }

    public static async Task<BotConfig> LoadAsync()
    {
        JToken fileContents = await JsonFileExtensions.LoadFromFileAsync(DefaultPath);
        BotConfig config = fileContents.ToObject<BotConfig>();
        config.Data = await BotData.LoadAsync();
        config.Auth = await BotAuth.LoadAsync();
        return config;
    }
}
```


## Handler
Handlers are classes which simplify processing of Discord Socket's events. They allow logically separate sets of commands and bot's features, but if you wish you can use one handler for everything.

To create the handler, create a new class which inherits from generic [HandlerBase\<TConfig\>](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/CommandsProcessing/HandlerBase.cs).
```csharp
class MyCustomHandler : HandlerBase<BotConfig>
{ 
    public MyCustomHandler(DiscordSocketClient client, BotConfig config) : base(client, config)
    {
    	// additional initialization
        // can also add commands to stack here
    }
}
```

### Commands stack
Handler has a built-in feature called commands stack. When a message is received, the default implementation of `OnMessageReceived` will process each command until the first returns true. For example, if following two commands are added in following order:
```csharp
CommandsStack.Add(new RegexUserCommand("^foo bar", CmdFooBar));
CommandsStack.Add(new RegexUserCommand("^foo", CmdFoo));
```
and the bot receives command `foo bar`, only `CmdFooBar` will be invoked, even though the later regex is also matching. This feature prevents bot responding to same message multiple times, and can be useful for help command or unrecognized command responses.

>The recommended place to add commands to stack is Handler's constructor.

### Overriding methods
Event handling methods can be overriden. They are empty by default, with exception of `OnMessageReceived`, which is used to execute Commands stack, so special care needs to be taken when overriding it. All other methods are safe to override, which allows to easily add custom handling of socket events and provides friendly-named parameters.
```csharp
protected override async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
{
    // do something with reaction here
}
```
By default, each of the overriden methods will be executed on a separate task to prevent blocking socket connection task. If this behaviour is undesirable, you can listen to events of `Client` directly - they will execute on the same task. Alternatively, `SwitchTaskContext` to false will disable this behaviour for entire handler. 

> Currently, majority of client events are relayed through few tasks to allow such behaviour. If you're manually building this library and performance is *really* critical, you may want to remove events you don't use from constructor **and** `Dispose()` method of [HandlerBase\<T\>](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/CommandsProcessing/HandlerBase.cs).


### ProductionOnly attribute
Handler can additionally have [\[ProductionOnly\]](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/CommandsProcessing/ProductionOnlyAttribute.cs) attribute added. Doing so prevents the handler from being automatically loaded by [BotInitializer](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/BotInitializer.cs) if a debugger is attached, for example by Visual Studio. This can be useful when bot is already running with stable commands while you're testing out new ones.

>An example of an alternative approach is to implement custom "debug" mode, and use a custom [ICommandVerificator](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/CommandsProcessing/ICommandVerificator.cs) to return false for finished commands when running with debugger. However, this is not implemented in this library and requires a custom implementation


## Commands Processing
One of my main issues with Discord.NET is it's command handling solution. While it works for simple cases, and supports nice features (like DI for example), I wanted to use RegEx for commands. This library features a simple to use commands handling based on RegEx, which can be easily extended with custom classes.

### Command
Command processor is the heart of my command processing system - is effectively an instance of a command. It is designed to take a message, and run series of checks to determine if assigned callback should be executed. [ICommandProcessor](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/CommandsProcessing/ICommandProcessor.cs)'s `ProcessAsync` method is used together with handler's Commands stack - if it returns true, the next commands in the stack will not be checked.

This library comes with a default [RegexUserCommand](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/CommandsProcessing/RegexUserCommand.cs) command processor, which is designed to use ICommandVerificator and RegEx to validate the command.

[RegexUserCommand](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/CommandsProcessing/RegexUserCommand.cs) invokes the provided method, inserting message context and Regex Match as params. Match can be used to easily retrieve groups from regex pattern.

```csharp
class MyCustomHandler : HandlerBase<BotConfig>
{ 
    public MyCustomHandler(DiscordSocketClient client, BotConfig config) : base(client, config)
    {
    	// execute CmdFoo when user sends "!foo <something>" (using default verificator)
    	CommandsStack.Add(new RegexUserCommand("^foo (.+)", CmdFoo));
    }
    private async Task CmdFoo(SocketCommandContext message, Match match)
    {
        string userContent = match.Groups[1].Value;
        await message.ReplyAsync($"Hi, {message.User.Mention}\nYou sent me `{userContent}`");
        await message.ReplyAsync("I will remove your message in 5 seconds, though.");
        await Task.Delay(5 * 1000);
        await message.Message.DeleteAsync();
    }
}
```

### Command Verificator
Command verificator has similar purpose as Command Processor and is designed to work together with it. In fact, it could be a part of a Command Processor itself. However, the separation gives few benefits:
1. Simplifies constructor of a command.
2. Allows sharing common checks (such as checking for prefix) between commands easily with a single instance of checks set.
3. Reduces Command Processor responsibility to just validating command's text, and running the verificator.
4. Allows creating few default sets of checks, such as set for guild-only commands.

This library comes with a default [CommandVerificator](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/CommandsProcessing/ICommandVerificator.cs) implementation, which additionally has two default instances - `DefaultPrefixed` and `DefaultPrefixedGuildOnly`, which accept a string or mention as prefix, and automatically ignore bot messages. Additionally, this implementation of verificator will automatically strip the message of prefix, and provide it to the CommandProcessor prefix-less. [RegexUserCommand](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/CommandsProcessing/RegexUserCommand.cs) by default will use `DefaultPrefixed`, but another instance can be supplied through a constructor.
```csharp
ICommandVerificator verificator = new MyCustomVerificator();
verificator.MyProperty = myValue;
// use custom verificator for "foo bar" command
CommandsStack.Add(new RegexUserCommand("^foo bar", CmdFooBar, verificator));
// command "foo" will just use default verificator
CommandsStack.Add(new RegexUserCommand("^foo", CmdFoo));
```

The default verificator has following properties:
- IgnoreBots = true,
- AcceptMentionPrefix = true,
- AcceptGuildMessages = true,
- AcceptPrivateMessages = true,
- StringPrefix = "!"

You can change any property of default verificator. If it's not enough, you can extend default [CommandVerificator](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/CommandsProcessing/ICommandVerificator.cs) implementation, or create a completely new one by implementing [ICommandVerificator](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/CommandsProcessing/ICommandVerificator.cs) interface.

## Starting the bot
One of the goals of the library is to simplify the bot starting. For this purpose, I created [BotInitializer](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/BotInitializer.cs) class. It's sole purpose is to start the client.

```csharp
private static BotInitializer<BotConfig> _initializer;

static async Task Main(string[] args)
{
    // load our custom config
    BotConfig config = await BotConfig.LoadAsync();
    // create initializer
    _initializer = new BotInitializer<BotConfig>(config);
    // (optional) set properties, for example message cache size:
    _initializer.MessageCacheSize = 10;
    // start and connect client
    await _initializer.StartClient();
    // prevent application from exiting
    await Task.Delay(-1);
}
```

The class has few properties that can be changed, however they need to be changed before `StartClient()` method is invoked. Changing them after calling that method will have no effect. An exception to this is `HandleLogs` property, which can be changed at any time.

After calling `StartClient()`, you can use `Client` property to retrieve the discord socket client instance. Keep in mind, that according to [Discord.NET Documentation](https://discord.foxbot.me/docs/guides/getting_started/first-bot.html), the client may not be connected to Discord yet - use Connected event for initialization actions that require client to be connected.
```csharp
static async Task Main(string[] args)
{
    /* ... */
    await _initializer.StartClient();
    _initializer.Client.Connected += Client_Connected;
    /* ... */
}

private static Task Client_Connected()
    => _initializer.Client.SetGameAsync("!help", null, ActivityType.Listening);
```

### Changing prefix
Command Verificator is responsible for checking for prefix. If you use custom implementation of ICommandVerificator, you need to make your command verificator accept correct prefix in it's logic.

If you use instances of default verificator, change `StringPrefix` property of the instance.
```csharp
ICommandVerificator verificator = new CommandVerificator();
// change the prefix
verificator.StringPrefix = "??";
// use custom verificator for "??foo bar" command
CommandsStack.Add(new RegexUserCommand("^foo bar", CmdFooBar, verificator));
```
If you don't use instances and let [RegexUserCommand](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/CommandsProcessing/RegexUserCommand.cs) use default instance, simply change prefix in default instances. This can be done for example in Main method, before starting the bot.
```csharp
static async Task Main(string[] args)
{
    // change prefix of default verificator
    (CommandVerificator.DefaultPrefixed as CommandVerificator).StringPrefix = "??";
    // change prefix of default guild only verificator (optional, as not automatically used)
    (CommandVerificator.DefaultPrefixedGuildOnly as CommandVerificator).StringPrefix = "??";

    // other startup logic
}
```

### Automatic handlers loading

If `AutoLoadHandlers` property is set to true (and it is by default), calling `StartClient()` method will automatically load  handlers that inherit from defined in the same assembly as bot [HandlerBase\<TConfig\>](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/CommandsProcessing/HandlerBase.cs). Handlers that have [\[ProductionOnly\]](https://github.com/TehGM/DiscordNetHelper/blob/master/DiscordNetHelper/CommandsProcessing/ProductionOnlyAttribute.cs) attribute will not be loaded if debugger is attached (for example, when running bot with Debugging in Visual Studio).

Any handler can be manually created by using it's constructor. This has to be done after `StartClient()` has been called, as handlers need access to Client instance.