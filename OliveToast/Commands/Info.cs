using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OliveToast.Managements.RequireCategoryEnable;
using static OliveToast.Managements.RequirePermission;

namespace OliveToast.Commands
{
    [Name("정보")]
    [RequireCategoryEnable(CategoryType.Info)]
    public class Info : ModuleBase<SocketCommandContext>
    {
        [Command("서버 정보"), Alias("서버정보", "서버")]
        [RequirePermission(PermissionType.UseBot), RequireContext(ContextType.Guild)]
        [Summary("서버의 정보를 확인합니다")]
        public async Task ServerInfo()
        {
            SocketGuild g = Context.Guild;
            EmbedBuilder emb = Context.CreateEmbed(title: $"{g.Name}의 정보", thumbnailUrl: g.IconUrl);

            emb.AddField("ID", g.Id, true);
            emb.AddField("생성일", g.CreatedAt.ToKSTString(), true);
            emb.AddEmptyField();

            emb.AddField("시스템 메시지 채널", g.SystemChannel != null ? g.SystemChannel.Mention : "-", true);
            emb.AddField("채널 규칙 및 지침", g.RulesChannel != null ? g.RulesChannel.Mention : "-", true);
            emb.AddField("커뮤니티 업데이트 채널", g.PublicUpdatesChannel != null ? g.PublicUpdatesChannel.Mention : "-", true);

            emb.AddField("잠수 채널", g.AFKChannel != null ? $"<#{g.AFKChannel.Id}>" : "-", true);
            emb.AddField("비활성화 시간 제한", $"{g.AFKTimeout / 60}분", true);
            emb.AddEmptyField();

            emb.AddField("서버 주인", g.Owner.Mention, true);
            emb.AddField("맴버 수", g.MemberCount, true);
            emb.AddField("역할 개수", g.Roles.Count, true);

            emb.AddField("카테고리 개수", g.CategoryChannels.Count, true);
            emb.AddField("텍스트 채널 개수", g.TextChannels.Count, true);
            emb.AddField("음성 채널 개수", g.VoiceChannels.Count, true);

            emb.AddField("부스트 횟수", g.PremiumSubscriptionCount, true);
            emb.AddField("레벨", $"레벨 {(int)g.PremiumTier}", true);
            emb.AddField("서버 이모티콘 개수", g.Emotes.Count, true);

            emb.AddField("보안 수준", g.VerificationLevel switch {
                VerificationLevel.None => "없음",
                VerificationLevel.Low => "낮음",
                VerificationLevel.Medium => "중간",
                VerificationLevel.High => "높음",
                VerificationLevel.Extreme => "매우 높음",
                _ => "-"
            }, true);
            emb.AddField("유해 미디어 콘텐츠 필터", g.ExplicitContentFilter switch {
                ExplicitContentFilterLevel.Disabled => "비활성화",
                ExplicitContentFilterLevel.MembersWithoutRoles => "역할 없는 맴버만",
                ExplicitContentFilterLevel.AllMembers => "모든 맴버",
                _ => "-"
            }, true);
            emb.AddField("2단계 인증", (g.MfaLevel == MfaLevel.Enabled).ToEmoji(), true);

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("채널 정보"), Alias("채널정보", "채널")]
        [RequirePermission(PermissionType.UseBot), RequireContext(ContextType.Guild)]
        [Summary("채널의 정보를 확인합니다\n`채널`은 생략할 수 있습니다")]
        public async Task ChannerInfo([Name("채널"), Remainder] SocketTextChannel c = null)
        {
            if (c == null)
            {
                c = Context.Channel as SocketTextChannel;
            }

            EmbedBuilder emb = Context.CreateEmbed(title: $"{c.Name}의 정보");

            emb.AddField("ID", c.Id, true);
            emb.AddField("생성일", c.CreatedAt.ToKSTString(), true);
            emb.AddEmptyField();

            emb.AddField("카테고리", c.Category != null ? c.Category.Name : "-", true);
            emb.AddField("연령 제한 채널", c.IsNsfw.ToEmoji(), true);
            emb.AddField("슬로우 모드", c.SlowModeInterval.ToTimeString(), true);

            emb.AddField("채널 주제", c.Topic ?? "** **");

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("역할 정보"), Alias("역할정보", "역할")]
        [RequirePermission(PermissionType.UseBot), RequireContext(ContextType.Guild)]
        [Summary("역할의 정보를 확인합니다")]
        public async Task RoleInfo([Name("역할"), Remainder] SocketRole r)
        {
            EmbedBuilder emb = Context.CreateEmbed(title: $"{r.Name}의 정보", color: r.Color);

            emb.AddField("ID", r.Id, true);
            emb.AddField("생성일", r.CreatedAt.ToKSTString(), true);
            emb.AddEmptyField();

            emb.AddField("색", r.Color, true);
            emb.AddField("분리하여 표시", r.IsHoisted.ToEmoji(), true);
            emb.AddField("맨션 허용", r.IsMentionable.ToEmoji(), true);

            emb.AddField("서버 일반 권한", @$"
관리자: {r.Permissions.Administrator.ToEmoji()}
채널 보기: {r.Permissions.ViewChannel.ToEmoji()}
채널 관리하기: {r.Permissions.ManageChannels.ToEmoji()}
역할 관리하기: {r.Permissions.ManageRoles.ToEmoji()}
이모티콘 관리: {r.Permissions.ManageEmojisAndStickers.ToEmoji()}
감사 로그 보기: {r.Permissions.ViewAuditLog.ToEmoji()}
서버 인사이트 보기: {r.Permissions.ViewGuildInsights.ToEmoji()}
웹후크 관리하기: {r.Permissions.ManageWebhooks.ToEmoji()}
서버 관리하기: {r.Permissions.ManageGuild.ToEmoji()}
            ", true);
            emb.AddField("맴버십 권한", @$"
초대 코드 만들기: {r.Permissions.CreateInstantInvite.ToEmoji()}
별명 변경하기: {r.Permissions.ChangeNickname.ToEmoji()}
별명 관리하기: {r.Permissions.ManageNicknames.ToEmoji()}
맴버 추방하기: {r.Permissions.KickMembers.ToEmoji()}
맴버 차단하기: {r.Permissions.BanMembers.ToEmoji()}
            ", true);
            emb.AddEmptyField();

            emb.AddField("채팅 채널 권한", @$"
메시지 보내기: {r.Permissions.SendMessages.ToEmoji()}
링크 첨부: {r.Permissions.EmbedLinks.ToEmoji()}
파일 첨부: {r.Permissions.AttachFiles.ToEmoji()}
반응 추가하기: {r.Permissions.AddReactions.ToEmoji()}
외부 이모티콘 사용: {r.Permissions.UseExternalEmojis.ToEmoji()}
모든 역할 맨션하기: {r.Permissions.MentionEveryone.ToEmoji()}
메시지 관리: {r.Permissions.ManageMessages.ToEmoji()}
메시지 기록 보기: {r.Permissions.ReadMessageHistory.ToEmoji()}
TTS 메시지 전송: {r.Permissions.SendTTSMessages.ToEmoji()}
            ", true);
            emb.AddField("음성 채널 권한", @$"
연결: {r.Permissions.Connect.ToEmoji()}
말하기: {r.Permissions.Speak.ToEmoji()}
동영상: {r.Permissions.Stream.ToEmoji()}
음성 감지 사용: {r.Permissions.UseVAD.ToEmoji()}
우선 발언권: {r.Permissions.PrioritySpeaker.ToEmoji()}
맴버들의 마이크 음소거하기: {r.Permissions.MuteMembers.ToEmoji()}
맴버의 헤드셋 음소거하기: {r.Permissions.DeafenMembers.ToEmoji()}
맴버 이동: {r.Permissions.MoveMembers.ToEmoji()}
            ", true);
            emb.AddEmptyField();

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("유저 정보"), Alias("유저정보", "유저")]
        [RequirePermission(PermissionType.UseBot), RequireContext(ContextType.Guild)]
        [Summary("유저의 정보를 확인합니다\n`유저`는 생략할 수 있습니다")]
        public async Task UserInfo([Name("유저"), Remainder] SocketGuildUser u = null)
        {
            if (u == null)
            {
                u = Context.User as SocketGuildUser;
            }

            EmbedBuilder emb = Context.CreateEmbed(title: $"{u.Username}의 정보", thumbnailUrl: u.GetAvatar());

            emb.AddField("유저네임", u.Username, true);
            emb.AddField("닉네임", u.Nickname ?? "-", true);
            emb.AddField("태그", u.Discriminator, true);

            emb.AddField("ID", u.Id, true);
            emb.AddField("봇 여부", u.IsBot.ToEmoji(), true);
            emb.AddEmptyField();

            emb.AddField("계정 생성일", u.CreatedAt.ToKSTString());
            emb.AddField("서버 참가일", u.JoinedAt?.ToKSTString());

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("유저 정보"), Alias("유저정보", "유저")]
        [RequirePermission(PermissionType.UseBot), RequireContext(ContextType.DM | ContextType.Group)]
        [Summary("유저의 정보를 확인합니다\n`유저`는 생략할 수 있습니다")]
        public async Task UserInfo([Name("유저"), Remainder] SocketUser u = null)
        {
            if (u == null)
            {
                u = Context.User;
            }

            EmbedBuilder emb = Context.CreateEmbed(title: $"{u.Username}의 정보", thumbnailUrl: u.GetAvatar());

            emb.AddField("유저네임", u.Username, true);
            emb.AddField("태그", u.Discriminator, true);
            emb.AddEmptyField();

            emb.AddField("ID", u.Id, true);
            emb.AddField("봇 여부", u.IsBot.ToEmoji(), true);
            emb.AddEmptyField();

            emb.AddField("계정 생성일", u.CreatedAt.ToKSTString());

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("봇 정보"), Alias("봇정보", "봇")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("봇의 정보를 확인합니다")]
        public async Task BotInfo()
        {
            EmbedBuilder emb = Context.CreateEmbed(title: "올리브토스트의 정보", thumbnailUrl: Program.Client.CurrentUser.GetAvatar());

            emb.AddField("서버 수", $"{Program.Client.Guilds.Count}곳", true);
            List<ulong> users = new();
            foreach(SocketGuild guild in Program.Client.Guilds)
            {
                users.AddRange(guild.Users.Select(u => u.Id).Where(u => !users.Contains(u)));
            }
            emb.AddField("유저 수", $"{users.Count}명", true);
            emb.AddEmptyField();

            emb.AddField("핑", $"{Program.Client.Latency}ms", true);
            TimeSpan t = DateTime.Now - Program.Uptime;
            emb.AddField("업타임", $"{t.Days}일 {t.Hours}시간", true);

            emb.AddField("버전", "v6.0.0 beta");

            emb.AddField(":mailbox_with_mail:", $"· [봇 초대 링크]({Utility.GetInvite()})\n· [UniqueCode 공식 서버](https://discord.gg/ARCdUzC)", true);
            emb.AddField(":page_facing_up:", $"· [소스코드](https://github.com/choshinyoung/OliveToast/)\n· [버그 제보](https://github.com/choshinyoung/OliveToast/issues)", true);
            
            emb.AddField("크레딧", "제작: <@396163884005851137>\n\n도움: <@410260925795270656> <@404891182423080960> <@310247242546151434>\n\n프로필 일러스트: <@679641309010853895>");

            await Context.ReplyEmbedAsync(emb.Build());
        }
    }
}
