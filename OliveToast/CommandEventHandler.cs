using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using OliveToast.Commands;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OliveToast
{
    class CommandEventHandler
    {
        public static readonly string prefix = ConfigManager.Get("PREFIX");

        public static void RegisterEvents(DiscordSocketClient client, CommandService command)
        {
            client.Log += OnLog;
            command.Log += OnCommandLog;

            client.MessageReceived += OnMessageReceived;
            command.CommandExecuted += OnCommandExecuted;

            client.GuildAvailable += OnJoinGuild;
            client.JoinedGuild += OnJoinGuild;
        }

        public static async Task OnLog(LogMessage msg)
        {
            Console.WriteLine(msg);

            await Task.CompletedTask;
        }

        public static async Task OnCommandLog(LogMessage msg)
        {
            Console.WriteLine(msg);

            await Task.CompletedTask;
        }

        private static async Task OnJoinGuild(SocketGuild arg)
        {
            if (DbManager.Guilds.Find(g => g.GuildId == arg.Id).Any())
            {
                return;
            }

            DbManager.Guilds.InsertOne(new OliveGuild(arg.Id));

            await Task.CompletedTask;
        }

        public static async Task OnMessageReceived(SocketMessage msg)
        {
            SocketUserMessage userMsg = msg as SocketUserMessage;

            if (userMsg == null || userMsg.Content == null ||
                userMsg.Author.Id == Program.Client.CurrentUser.Id || userMsg.Author.IsBot) return;

            SocketCommandContext context = new SocketCommandContext(Program.Client, userMsg);

            if (await Games.WordRelay(context))
            {
                return;
            }
            if (await Games.TypingGame(context))
            {
                return;
            }

            int argPos = 0;
            if (userMsg.HasStringPrefix(prefix, ref argPos) || userMsg.HasMentionPrefix(Program.Client.CurrentUser, ref argPos))
            {
                await Program.Command.ExecuteAsync(context, argPos, Program.Service);
            }
        }

        public static async Task OnCommandExecuted(Discord.Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                if (result.Error == CommandError.UnknownCommand)
                {
                    return;
                }

                var ctx = context as SocketCommandContext;

                EmbedBuilder emb = ctx.CreateEmbed(title: "오류 발생!", description: $"{result.Error}: {result.ErrorReason}");

                await ctx.MsgReplyEmbedAsync(emb.Build());
            }
        }
    }
}
