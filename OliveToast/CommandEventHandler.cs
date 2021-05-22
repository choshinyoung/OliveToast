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

            client.GuildAvailable += OnGuildAvailable;
            client.JoinedGuild += OnJoinGuild;
            client.LeftGuild += OnLeftGuild;

            client.Ready += OnReady;
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

        private static async Task OnGuildAvailable(SocketGuild guild)
        {
            if (DbManager.Guilds.Find(g => g.GuildId == guild.Id).Any())
            {
                return;
            }

            DbManager.Guilds.InsertOne(new OliveGuild(guild.Id));

            await Task.CompletedTask;
        }

        private static async Task OnJoinGuild(SocketGuild guild)
        {
            await OnGuildAvailable(guild);

            await KoreanBots.UpdateServerCountAsync(Program.Client.Guilds.Count);
        }

        private static async Task OnLeftGuild(SocketGuild arg)
        {
            await KoreanBots.UpdateServerCountAsync(Program.Client.Guilds.Count);
        }

        private static async Task OnReady()
        {
            await KoreanBots.UpdateServerCountAsync(Program.Client.Guilds.Count);
        }

        public static async Task OnMessageReceived(SocketMessage msg)
        {
            if (msg is not SocketUserMessage userMsg || userMsg.Content == null ||
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
            else
            {
                if (new Random().Next(0, 5) == 0 && !await KoreanBots.IsVotedAsync(context.User.Id))
                {
                    await (context as SocketCommandContext).MsgReplyEmbedAsync("아직 디코봇 웹사이트에서 올리브토스트에게 하트를 주지 않았습니다.\n[이곳](https://koreanbots.dev/bots/495209098929766400)에서 올리브토스트에게 투표해주세요!");
                }
            }
        }
    }
}
