using Discord;
using Discord.WebSocket;
using MongoDB.Driver;
using OliveToast.Managements.data;
using OliveToast.Utilities;
using System.Threading.Tasks;

namespace OliveToast
{
    class LogEventHandler
    {
        public static void RegisterEvents(DiscordSocketClient client)
        {
            client.MessageUpdated += OnMessageUpdated;
            client.MessageDeleted += OnMessageDeleted;
        }
         
        public static async Task OnMessageUpdated(Cacheable<IMessage, ulong> cache, SocketMessage msg, ISocketMessageChannel channel)
        {
            await Task.Factory.StartNew(async () =>
            {
                if (channel.GetType() != typeof(SocketTextChannel))
                {
                    return;
                }

                SocketGuild guild = ((SocketTextChannel)channel).Guild;
                OliveGuild.GuildSetting setting = OliveGuild.Get(guild.Id).Setting;
                if (!setting.EnabledCategories.Contains(RequireCategoryEnable.CategoryType.Log) || !setting.LogChannelId.HasValue)
                {
                    return;
                }
                SocketTextChannel c = guild.GetTextChannel(setting.LogChannelId.Value);

                EmbedBuilder emb = new()
                {
                    Title = "메시지 수정",
                    Color = new Color(255, 255, 0),
                    Description = $"<#{channel.Id}> 채널에서 [메시지]({msg.GetJumpUrl()})가 수정됐어요\n",
                };
                emb.WithAuthor($"{cache.Value.Author.Username}#{cache.Value.Author.Discriminator} ({cache.Value.Author.Id})", cache.Value.Author.GetAvatar());

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
            });
        }

        private static async Task OnMessageDeleted(Cacheable<IMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel)
        {
            await Task.Factory.StartNew(async () =>
            {
                if (!cache.HasValue && !channel.HasValue)
                {
                    return;
                }

                SocketGuild guild = ((SocketTextChannel)channel.Value).Guild;
                OliveGuild.GuildSetting setting = OliveGuild.Get(guild.Id).Setting;
                if (!setting.EnabledCategories.Contains(RequireCategoryEnable.CategoryType.Log) || !setting.LogChannelId.HasValue)
                {
                    return;
                }
                SocketTextChannel c = guild.GetTextChannel(setting.LogChannelId.Value);

                EmbedBuilder emb = new()
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
            });
        }
    }
}
