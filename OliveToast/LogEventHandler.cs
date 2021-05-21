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
            SocketGuildChannel beforeGuild = before as SocketGuildChannel;
            SocketGuildChannel afterGuild = after as SocketGuildChannel;

            SocketGuild guild = beforeGuild.Guild;
            OliveGuild.GuildSetting setting = OliveGuild.Get(guild.Id).Setting;
            if (!setting.LogType.Contains(LogTypes.채널수정) || !setting.LogChannelId.HasValue || !guild.Channels.Any(c => c.Id == setting.LogChannelId.Value))
            {
                return;
            }

            SocketTextChannel c = guild.GetTextChannel(setting.LogChannelId.Value);

            EmbedBuilder emb = new EmbedBuilder
            {
                Title = "채널 수정",
                Color = UpdateColor,
                Description = $"채널 `{((SocketGuildChannel)before).Name.이가($"`({before.Id})")} 수정됐어요\n<#{before.Id}>",
                Timestamp = DateTimeOffset.Now.ToKST()
            }; 

            if (beforeGuild.Name != afterGuild.Name)
            {
                emb.AddField("채널 이름", $"{beforeGuild.Name} => {afterGuild.Name}", true);
            }
            if (beforeGuild.Position != afterGuild.Position)
            {
                return;
            }

            if (before is SocketTextChannel)
            {
                SocketTextChannel beforeText = before as SocketTextChannel;
                SocketTextChannel afterText = after as SocketTextChannel;

                if (beforeText.IsNsfw != afterText.IsNsfw)
                {
                    emb.AddField("연령 제한 채널", $"{beforeText.IsNsfw.ToEmoji()} => {afterText.IsNsfw.ToEmoji()}", true);
                }
                if (beforeText.SlowModeInterval != afterText.SlowModeInterval)
                {
                    emb.AddField("슬로우 모드", $"{beforeText.SlowModeInterval.ToTimeString()} => {afterText.SlowModeInterval.ToTimeString()}", true);
                }
                if (beforeText.Topic != afterText.Topic)
                {
                    emb.AddField("채널 주제", $"{beforeText.Topic} => {afterText.Topic}", true);
                }
            }
            else if (before is SocketVoiceChannel)
            {
                SocketVoiceChannel beforeVoice = before as SocketVoiceChannel;
                SocketVoiceChannel afterVoice = after as SocketVoiceChannel;
                
                if (beforeVoice.Bitrate != afterVoice.Bitrate)
                {
                    emb.AddField("비트 레이트", $"{beforeVoice.Bitrate / 1000}kbps => {afterVoice.Bitrate / 1000}kbps", true);
                }
                if (beforeVoice.UserLimit != afterVoice.UserLimit)
                {
                    emb.AddField("최대 사용자 수", $"{beforeVoice.UserLimit?.ToString() ?? "∞"} => {afterVoice.UserLimit?.ToString() ?? "∞"}", true);
                }
            }

            await c.SendMessageAsync(embed: emb.Build());
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
                Color = DeleteColor,
                Description = $"채널 `{((SocketGuildChannel)channel).Name.이가($"`({channel.Id})")} 삭제됐어요",
                Timestamp = DateTimeOffset.Now.ToKST()
            };

            await c.SendMessageAsync(embed: emb.Build());
        }

        private static async Task OnGuildUpdated(SocketGuild before, SocketGuild after)
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(before.Id).Setting;
            if (!setting.LogType.Contains(LogTypes.서버수정) || !setting.LogChannelId.HasValue || !before.Channels.Any(c => c.Id == setting.LogChannelId.Value))
            {
                return;
            }

            SocketTextChannel c = before.GetTextChannel(setting.LogChannelId.Value);

            EmbedBuilder emb = new EmbedBuilder
            {
                Title = "서버 수정",
                Color = UpdateColor,
                Description = "서버가 수정됐어요",
                Timestamp = DateTimeOffset.Now.ToKST()
            };

            if (before.AFKChannel != after.AFKChannel)
            {
                emb.AddField("비활성화 채널", $"{before.AFKChannel?.Name ?? ""} => {after.AFKChannel?.Name ?? ""}", true);
            }
            if (before.AFKTimeout != after.AFKTimeout)
            {
                emb.AddField("비활성화 시간 제한", $"{before.AFKTimeout / 60}분 => {after.AFKTimeout / 60}분", true);
            }
            if (before.DefaultChannel != after.DefaultChannel)
            {
                emb.AddField("기본 채널", $"{before.DefaultChannel.Name} => {after.DefaultChannel.Name}", true);
            }
            if (before.ExplicitContentFilter != after.ExplicitContentFilter)
            {
                string b = before.ExplicitContentFilter switch
                {
                    ExplicitContentFilterLevel.Disabled => "비활성화",
                    ExplicitContentFilterLevel.MembersWithoutRoles => "역할 없는 맴버만",
                    ExplicitContentFilterLevel.AllMembers => "모든 맴버",
                    _ => "-"
                };
                string a = after.ExplicitContentFilter switch
                {
                    ExplicitContentFilterLevel.Disabled => "비활성화",
                    ExplicitContentFilterLevel.MembersWithoutRoles => "역할 없는 맴버만",
                    ExplicitContentFilterLevel.AllMembers => "모든 맴버",
                    _ => "-"
                };
                emb.AddField("유해 미디어 콘텐츠 필터", $"{b} => {a}", true);
            }
            if (before.IconUrl != after.IconUrl)
            {
                emb.AddField("아이콘", $"{before.IconUrl} => {after.IconUrl}", true);
            }
            if (before.MfaLevel != after.MfaLevel)
            {
                emb.AddField("2단계 인증", $"{(before.MfaLevel == MfaLevel.Disabled).ToEmoji()} => {(after.MfaLevel == MfaLevel.Disabled).ToEmoji()}", true);
            }
            if (before.Name != after.Name)
            {
                emb.AddField("이름", $"{before.Name} => {after.Name}", true);
            }
            if (before.Owner != after.Owner)
            {
                emb.AddField("서버 주인", $"{before.Owner.Username}#{before.Owner.Discriminator} => {after.Owner.Username}#{after.Owner.Discriminator}", true);
            }
            if (before.PublicUpdatesChannel != after.PublicUpdatesChannel)
            {
                emb.AddField("커뮤니티 업데이트 채널", $"{before.PublicUpdatesChannel.Name} => {after.PublicUpdatesChannel.Name}", true);
            }
            if (before.RulesChannel != after.RulesChannel)
            {
                emb.AddField("채널 규칙 및 지침", $"{before.RulesChannel.Name} => {after.RulesChannel.Name}", true);
            }
            if (before.SystemChannel != after.SystemChannel)
            {
                emb.AddField("시스템 채널", $"{before.SystemChannel.Name} => {after.SystemChannel.Name}", true);
            }
            if (before.VerificationLevel != after.VerificationLevel)
            {
                string b = before.VerificationLevel switch
                {
                    VerificationLevel.None => "없음",
                    VerificationLevel.Low => "낮음",
                    VerificationLevel.Medium => "중간",
                    VerificationLevel.High => "높음",
                    VerificationLevel.Extreme => "매우 높음",
                    _ => "-"
                };
                string a = after.VerificationLevel switch
                {
                    VerificationLevel.None => "없음",
                    VerificationLevel.Low => "낮음",
                    VerificationLevel.Medium => "중간",
                    VerificationLevel.High => "높음",
                    VerificationLevel.Extreme => "매우 높음",
                    _ => "-"
                };
                emb.AddField("보안 수준", $"{b} => {a}", true);
            }

            await c.SendMessageAsync(embed: emb.Build());
        }

        private static async Task OnInviteCreated(SocketInvite invite)
        {
            SocketGuild guild = invite.Guild;
            OliveGuild.GuildSetting setting = OliveGuild.Get(guild.Id).Setting;
            if (!setting.LogType.Contains(LogTypes.초대링크생성) || !setting.LogChannelId.HasValue || !guild.Channels.Any(c => c.Id == setting.LogChannelId.Value))
            {
                return;
            }

            SocketTextChannel c = guild.GetTextChannel(setting.LogChannelId.Value);

            EmbedBuilder emb = new EmbedBuilder
            {
                Title = "초대링크 생성",
                Color = CreateColor,
                Description = $"<#{invite.Channel.Id}> 채널의 [초대링크]({invite.Url})가 생성됐어요",
                Timestamp = DateTimeOffset.Now.ToKST()
            };
            emb.WithAuthor(invite.Inviter);

            emb.AddField("코드", invite.Code);

            emb.AddField("잔여 유효 기간", invite.MaxAge.ToTimeString(), true);
            emb.AddField("최대 사용 횟수", invite.MaxUses, true);

            await c.SendMessageAsync(embed: emb.Build());
        }

        private static async Task OnInviteDeleted(SocketGuildChannel channel, string code)
        {
            SocketGuild guild = channel.Guild;
            OliveGuild.GuildSetting setting = OliveGuild.Get(guild.Id).Setting;
            if (!setting.LogType.Contains(LogTypes.초대링크삭제) || !setting.LogChannelId.HasValue || !guild.Channels.Any(c => c.Id == setting.LogChannelId.Value))
            {
                return;
            }

            SocketTextChannel c = guild.GetTextChannel(setting.LogChannelId.Value);

            EmbedBuilder emb = new EmbedBuilder
            {
                Title = "초대링크 제거",
                Color = DeleteColor,
                Description = $"<#{channel.Id}> 채널의 초대링크 ({code})가 삭제됐어요",
                Timestamp = DateTimeOffset.Now.ToKST()
            };

            await c.SendMessageAsync(embed: emb.Build());
        }

        private static async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel.GetType() != typeof(SocketTextChannel))
            {
                return;
            }

            SocketGuild guild = ((SocketTextChannel)channel).Guild;
            OliveGuild.GuildSetting setting = OliveGuild.Get(guild.Id).Setting;
            if (!setting.LogType.Contains(LogTypes.반응추가) || !setting.LogChannelId.HasValue || !guild.Channels.Any(c => c.Id == setting.LogChannelId.Value))
            {
                return;
            }

            SocketTextChannel c = guild.GetTextChannel(setting.LogChannelId.Value);

            EmbedBuilder emb = new EmbedBuilder
            {
                Title = "반응 추가",
                Color = DeleteColor,
                Description = $"<#{channel.Id}> 채널에서 [메시지](https://discord.com/channels/{guild.Id}/{channel.Id}/{cache.Id})에 반응 {reaction.Emote.Name.이가()} 추가됐어요",
                Timestamp = DateTimeOffset.Now.ToKST()
            };

            await c.SendMessageAsync(embed: emb.Build());
        }

        private static async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
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
