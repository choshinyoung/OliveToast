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

        public static bool IsDebugMode = ConfigManager.Get("DEBUG_MODE") == "True";

        private static readonly DiscordSocketConfig clientConfig = new()
        {
            AlwaysDownloadUsers = true,
            LogLevel = IsDebugMode ? LogSeverity.Debug : LogSeverity.Info,
            MessageCacheSize = 100000,
        };
        private static readonly CommandServiceConfig commandConfig = new()
        {
            DefaultRunMode = RunMode.Async,
            LogLevel = IsDebugMode ? LogSeverity.Debug : LogSeverity.Info,
        };

        static async Task Main()
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
