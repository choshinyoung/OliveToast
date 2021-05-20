using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using MongoDB.Driver;
using OliveToast.Managements;
using System;
using System.Linq;
using System.Threading.Tasks;
using static OliveToast.Managements.OliveGuild.GuildSetting;

namespace OliveToast
{
    class LogEventHandler
    {
        static readonly Color CreateColor = new Color(0, 255, 0);
        static readonly Color DeleteColor = new Color(255, 0, 0);
        static readonly Color UpdateColor = new Color(255, 255, 0);

        public static void RegisterEvents(DiscordSocketClient client)
        {
            client.MessageUpdated += OnMessageUpdated;
            client.MessageDeleted += OnMessageDeleted;

            client.ChannelCreated += OnChannelCreated;
            client.ChannelDestroyed += OnChannelDestroyed;
            client.ChannelUpdated += OnChannelUpdated;

            client.GuildUpdated += OnGuildUpdated;

            client.InviteCreated += OnInviteCreated;
            client.InviteDeleted += OnInviteDeleted;

            client.ReactionAdded += OnReactionAdded;
            client.ReactionRemoved += OnReactionRemoved;
            client.ReactionsCleared += OnReactionCleared;

            client.RoleCreated += OnRoleCreated;
            client.RoleDeleted += OnRoleDeleted;
            client.RoleUpdated += OnRoleUpdated;

            client.UserBanned += OnUserBanned;
            client.UserUnbanned += OnUserUnbanned;

            client.UserJoined += OnuserJoined;
            client.UserLeft += OnUserLeft;

            client.GuildMemberUpdated += OnGuildMemberUpdated;

            client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
        }

        public static async Task OnMessageUpdated(Cacheable<IMessage, ulong> cache, SocketMessage msg, ISocketMessageChannel channel)
        {
            if (channel.GetType() != typeof(SocketTextChannel))
            {
                return;
            }

            SocketGuild guild = ((SocketTextChannel)channel).Guild;
            OliveGuild.GuildSetting setting = OliveGuild.Get(guild.Id).Setting;
            if (!setting.LogType.Contains(LogTypes.메시지수정) || !setting.LogChannelId.HasValue || !guild.Channels.Any(c => c.Id == setting.LogChannelId.Value))
            {
                return;
            }

            SocketTextChannel c = guild.GetTextChannel(setting.LogChannelId.Value);

            EmbedBuilder emb = new EmbedBuilder
            {
                Title = "메시지 수정",
                Color = UpdateColor,
                Description = $"<#{channel.Id}> 채널에서 [메시지]({msg.GetJumpUrl()})가 수정됐어요\n",
                Timestamp = DateTimeOffset.Now.ToKST()
            };
            emb.WithAuthor(msg.Author);

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
            OliveGuild.GuildSetting setting = OliveGuild.Get(guild.Id).Setting;
            if (!setting.LogType.Contains(LogTypes.메시지삭제) || !setting.LogChannelId.HasValue || !guild.Channels.Any(c => c.Id == setting.LogChannelId.Value))
            {
                return;
            }

            SocketTextChannel c = guild.GetTextChannel(setting.LogChannelId.Value);

            EmbedBuilder emb = new EmbedBuilder
            {
                Title = "메시지 삭제",
                Color = DeleteColor,
                Description = $"<#{channel.Id}> 채널에서 메시지({cache.Id})가 삭제됐어요\n",
                Timestamp = DateTimeOffset.Now.ToKST()
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

        private static async Task OnChannelCreated(SocketChannel channel)
        {
            SocketGuild guild = ((SocketGuildChannel)channel).Guild;
            OliveGuild.GuildSetting setting = OliveGuild.Get(guild.Id).Setting;
            if (!setting.LogType.Contains(LogTypes.채널생성) || !setting.LogChannelId.HasValue || !guild.Channels.Any(c => c.Id == setting.LogChannelId.Value))
            {
                return;
            }

            SocketTextChannel c = guild.GetTextChannel(setting.LogChannelId.Value);

            EmbedBuilder emb = new EmbedBuilder
            {
                Title = "채널 생성",
                Color = CreateColor,
                Description = $"새로운 채널 `{((SocketGuildChannel)channel).Name.이가($"`({channel.Id})")} 생성됐어요\n<#{channel.Id}>",
                Timestamp = DateTimeOffset.Now.ToKST()
            };

            await c.SendMessageAsync(embed: emb.Build());
        }

        private static async Task OnChannelUpdated(SocketChannel before, SocketChannel after)
        {

        }

        private static async Task OnChannelDestroyed(SocketChannel channel)
        {
            SocketGuild guild = ((SocketGuildChannel)channel).Guild;
            OliveGuild.GuildSetting setting = OliveGuild.Get(guild.Id).Setting;
            if (!setting.LogType.Contains(LogTypes.채널삭제) || !setting.LogChannelId.HasValue || !guild.Channels.Any(c => c.Id == setting.LogChannelId.Value))
            {
                return;
            }

            SocketTextChannel c = guild.GetTextChannel(setting.LogChannelId.Value);

            EmbedBuilder emb = new EmbedBuilder
            {
                Title = "채널 삭제",
                Color = CreateColor,
                Description = $"채널 `{((SocketGuildChannel)channel).Name.이가($"`({channel.Id})")} 삭제됐어요",
                Timestamp = DateTimeOffset.Now.ToKST()
            };

            await c.SendMessageAsync(embed: emb.Build());
        }

        private static async Task OnGuildUpdated(SocketGuild before, SocketGuild after)
        {

        }

        private static async Task OnInviteDeleted(SocketGuildChannel channel, string code)
        {

        }

        private static async Task OnInviteCreated(SocketInvite invite)
        {

        }

        private static async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {

        }

        private static async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {

        }

        private static async Task OnReactionCleared(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel)
        {

        }

        private static async Task OnRoleCreated(SocketRole role)
        {

        }

        private static async Task OnRoleDeleted(SocketRole role)
        {

        }

        private static async Task OnRoleUpdated(SocketRole before, SocketRole after)
        {

        }

        private static async Task OnUserUnbanned(SocketUser user, SocketGuild guild)
        {

        }

        private static async Task OnUserBanned(SocketUser user, SocketGuild guild)
        {

        }

        private static async Task OnuserJoined(SocketGuildUser user)
        {

        }

        private static async Task OnUserLeft(SocketGuildUser user)
        {

        }

        private static async Task OnGuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {

        }

        private static async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {

        }
    }
}
