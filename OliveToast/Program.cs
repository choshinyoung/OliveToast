using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using OliveToast.Managements;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace OliveToast
{
    class Program
    {
        public static DiscordSocketClient Client;
        public static CommandService Command;
        private static IServiceProvider Service;

        private readonly DiscordSocketConfig clientConfig = new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Debug,
        };
        private readonly CommandServiceConfig commandConfig = new CommandServiceConfig
        {
            LogLevel = LogSeverity.Debug,
        };

        private readonly string prefix = ConfigManager.Get("PREFIX");

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Client = new DiscordSocketClient(clientConfig);
            Command = new CommandService(commandConfig);
            Service = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(Command)
                .BuildServiceProvider();

            await Command.AddModulesAsync(Assembly.GetEntryAssembly(), Service);

            string token = ConfigManager.Get("TOKEN");
            await Client.LoginAsync(TokenType.Bot, token);

            Client.Log += OnLog;
            Client.MessageReceived += OnMessageReceived;

            Command.Log += OnCommandLog;
            Command.CommandExecuted += OnCommandExecuted;

            await Client.StartAsync();
            await Client.SetGameAsync("+도움", null, ActivityType.Playing);

            await Task.Delay(-1);
        }

        private async Task OnLog(LogMessage msg)
        {
            Console.WriteLine(msg);

            await Task.CompletedTask;
        }

        private async Task OnMessageReceived(SocketMessage msg)
        {
            SocketUserMessage userMsg = msg as SocketUserMessage;

            if (userMsg == null || userMsg.Content == null ||
                userMsg.Author.Id == Client.CurrentUser.Id || userMsg.Author.IsBot) return;

            int argPos = 0;
            if (userMsg.HasStringPrefix(prefix, ref argPos) || userMsg.HasMentionPrefix(Client.CurrentUser, ref argPos))
            {
                SocketCommandContext context = new SocketCommandContext(Client, userMsg);

                await Command.ExecuteAsync(context, argPos, Service);
            }
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        private async Task OnCommandLog(LogMessage msg)
        {
            Console.WriteLine(msg);

            await Task.CompletedTask;
        }
    }
}
