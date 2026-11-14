using Discord;
using Discord.WebSocket;
using MongoDB.Driver;
using OliveToast.Managements;
using OliveToast.Managements.Data;
using OliveToast.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OliveToast
{
    class LogEventHandler
    {
        public static void RegisterEvents(DiscordSocketClient client)
        {
            client.MessageReceived += OnMessageReceived;
            client.MessageUpdated += OnMessageUpdated;
            client.MessageDeleted += OnMessageDeleted;
            client.MessagesBulkDeleted += OnMessagesBulkDeleted;
        }

        private static bool TryGetLogChannel(SocketGuild guild, out SocketTextChannel logChannel)
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(guild.Id).Setting;
            if (!setting.EnabledCategories.Contains(RequireCategoryEnable.CategoryType.Log) || !setting.LogChannelId.HasValue)
            {
                logChannel = null;
                return false;
            }

            logChannel = guild.GetTextChannel(setting.LogChannelId.Value);
            return logChannel is not null;
        }

        public static async Task OnMessageReceived(SocketMessage msg)
        {
            await Task.Factory.StartNew(() =>
            {
                if (msg.Channel is not SocketTextChannel channel || !TryGetLogChannel(channel.Guild, out _))
                {
                    return;
                }

                LogMessageCache.Store(channel.Guild.Id, msg);
            });
        }

        public static async Task OnMessageUpdated(Cacheable<IMessage, ulong> cache, SocketMessage msg, ISocketMessageChannel channel)
        {
            await Task.Factory.StartNew(async () =>
            {
                if (channel is not SocketTextChannel textChannel || !TryGetLogChannel(textChannel.Guild, out SocketTextChannel c))
                {
                    return;
                }

                LoggedMessage before = LogMessageCache.Store(textChannel.Guild.Id, msg);

                if (channel.GetType() != typeof(SocketTextChannel))
                {
                    return;
                }

                EmbedBuilder emb = new()
                {
                    Title = "메시지 수정",
                    Color = new Color(255, 255, 0),
                    Description = $"<#{channel.Id}> 채널에서 [메시지]({msg.GetJumpUrl()})가 수정됐어요\n",
                };
                emb.WithAuthor($"{msg.Author.Username}#{msg.Author.Discriminator} ({msg.Author.Id})", msg.Author.GetAvatar());

                if (before is not null)
                {
                    if (before.Content == msg.Content)
                    {
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(before.Content))
                    {
                        emb.Description += "\n수정 전 내용이 비어있어요";
                    }
                    else
                    {
                        emb.AddField("수정 전 내용", before.Content.Slice(512, out bool isApplied), true);
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
            });
        }

        private static async Task OnMessageDeleted(Cacheable<IMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel)
        {
            await Task.Factory.StartNew(async () =>
            {
                if (channel.Value is not SocketTextChannel textChannel)
                {
                    return;
                }

                LoggedMessage deleted = LogMessageCache.Remove(textChannel.Guild.Id, cache.Id);

                if (!TryGetLogChannel(textChannel.Guild, out SocketTextChannel c))
                {
                    return;
                }

                EmbedBuilder emb = new()
                {
                    Title = "메시지 삭제",
                    Color = new Color(255, 0, 0),
                    Description = $"<#{channel.Id}> 채널에서 메시지({cache.Id})가 삭제됐어요\n",
                };

                if (deleted is not null)
                {
                    emb.WithAuthor($"{deleted.Author.Username}#{deleted.Author.Discriminator} ({deleted.Author.Id})", deleted.Author.GetAvatar());

                    if (string.IsNullOrWhiteSpace(deleted.Content))
                    {
                        emb.Description += "\n내용이 비어있어요";
                    }
                    else
                    {
                        emb.AddField("내용", deleted.Content.Slice(512, out bool isApplied), true);
                        if (isApplied)
                        {
                            await c.SendFileAsync(deleted.Content.ToStream(), "before.txt", "");
                        }
                    }

                    if (deleted.AttachmentUrls.Count != 0)
                    {
                        string urls = "";

                        foreach (string url in deleted.AttachmentUrls)
                        {
                            urls += $"{url}\n";
                        }

                        emb.AddField("파일", urls, true);
                    }
                }
                else
                {
                    emb.WithAuthor("알 수 없음");

                    emb.Description += "\n내용이 캐시에 저장되지 않았어요";
                }

                await c.SendMessageAsync(embed: emb.Build());
            });
        }

        private static Task OnMessagesBulkDeleted(IReadOnlyCollection<Cacheable<IMessage, ulong>> messages, Cacheable<IMessageChannel, ulong> channel)
        {
            if (channel.Value is SocketTextChannel textChannel)
            {
                foreach (var message in messages)
                {
                    LogMessageCache.Remove(textChannel.Guild.Id, message.Id);
                }
            }

            return Task.CompletedTask;
        }
    }
}
