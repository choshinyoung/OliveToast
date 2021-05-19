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
        public static IServiceProvider Service;

        public static DateTime Uptime;

        private readonly DiscordSocketConfig clientConfig = new DiscordSocketConfig
        {
            AlwaysDownloadUsers = true,
            LogLevel = LogSeverity.Debug,
            MessageCacheSize = 100000,
        };
        private readonly CommandServiceConfig commandConfig = new CommandServiceConfig
        {
            DefaultRunMode = RunMode.Async,
            LogLevel = LogSeverity.Debug,
        };

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

            CommandEventHandler.RegisterEvents(Client, Command);
            LogEventHandler.RegisterEvents(Client);

            Uptime = DateTime.Now;

            await Client.StartAsync();
            await Client.SetGameAsync($"{ConfigManager.Get("PREFIX")}도움", null, ActivityType.Playing);

            await Task.Delay(-1);
        }
    }
}
