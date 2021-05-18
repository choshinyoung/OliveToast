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
    class EventHandler
    {
        public static readonly string prefix = ConfigManager.Get("PREFIX");

        public static void RegisterEvents(DiscordSocketClient client, CommandService command)
        {
            client.Log += OnLog;
            command.Log += OnCommandLog;

            client.GuildAvailable += OnJoinGuild;
            client.JoinedGuild += OnJoinGuild;

            client.MessageReceived += OnMessageReceived;
            command.CommandExecuted += OnCommandExecuted;

            client.MessageUpdated += OnMessageUpdated;
            client.MessageDeleted += OnMessageDeleted;
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

        public static async Task OnMessageUpdated(Cacheable<IMessage, ulong> cache, SocketMessage msg, ISocketMessageChannel channel)
        {
            if (channel.GetType() != typeof(SocketTextChannel))
            {
                return;
            }

            SocketGuild guild = ((SocketTextChannel)channel).Guild;
            ulong? logChannelId = OliveGuild.Get(guild.Id).Setting.LogChannelId;
            if (!logChannelId.HasValue || !guild.Channels.Any(c => c.Id == logChannelId.Value))
            {
                return;
            }

            SocketTextChannel c = guild.GetTextChannel(logChannelId.Value);

            EmbedBuilder emb = new EmbedBuilder
            {
                Title = "메시지 수정",
                Color = new Color(255, 255, 0),
                Author = new EmbedAuthorBuilder { Name = $"{msg.Author.Username}#{msg.Author.Discriminator} ({msg.Author.Id})", IconUrl = msg.Author.GetAvatar() },
                Description = $"<#{channel.Id}> 채널에서 [메시지]({msg.GetJumpUrl()})가 수정됐어요\n",
            };

            if (cache.HasValue)
            {
                if (cache.Value.Content == msg.Content)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(cache.Value.Content))
                {
                    emb.Description += "\n수정 전 내용이 비어있어요";
                }
                else
                {
                    emb.AddField("수정 전 내용", cache.Value.Content.Slice(512, out bool isApplied), true);
                    if (isApplied)
                    {
                        await c.SendFileAsync(msg.Content.ToStream(), "before.txt", "");
                    }
                }
            }
            else
            {
                emb.Description += "\n수정 전 내용은 캐시에 저장되지 않았어요";
            }

            if (string.IsNullOrWhiteSpace(msg.Content))
            {
                emb.Description += "\n수정 후 내용이 비어있어요";
            }
            else
            {
                emb.AddField("수정 후 내용", msg.Content.Slice(512, out bool isApplied), true);
                if (isApplied)
                {
                    await c.SendFileAsync(msg.Content.ToStream(), "after.txt", "");
                }
            }

            await c.SendMessageAsync(embed: emb.Build());
        }

        private static async Task OnMessageDeleted(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel)
        {
            if (channel.GetType() != typeof(SocketTextChannel))
            {
                return;
            }

            SocketGuild guild = ((SocketTextChannel)channel).Guild;
            ulong? logChannelId = OliveGuild.Get(guild.Id).Setting.LogChannelId;
            if (!logChannelId.HasValue || !guild.Channels.Any(c => c.Id == logChannelId.Value))
            {
                return;
            }

            SocketTextChannel c = guild.GetTextChannel(logChannelId.Value);

            EmbedBuilder emb = new EmbedBuilder
            {
                Title = "메시지 삭제",
                Color = new Color(255, 0, 0),
                Description = $"<#{channel.Id}> 채널에서 메시지({cache.Id})가 삭제됐어요\n",
            };

            if (cache.HasValue)
            {
                emb.WithAuthor($"{cache.Value.Author.Username}#{cache.Value.Author.Discriminator} ({cache.Value.Author.Id})", cache.Value.Author.GetAvatar());

                if (string.IsNullOrWhiteSpace(cache.Value.Content))
                {
                    emb.Description += "\n내용이 비어있어요";
                }
                else
                {
                    emb.AddField("내용", cache.Value.Content.Slice(512, out bool isApplied), true);
                    if (isApplied)
                    {
                        await c.SendFileAsync(cache.Value.Content.ToStream(), "before.txt", "");
                    }
                }
            }
            else
            {
                emb.WithAuthor("알 수 없음");

                emb.Description += "\n내용이 캐시에 저장되지 않았어요";
            }

            await c.SendMessageAsync(embed: emb.Build());
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
