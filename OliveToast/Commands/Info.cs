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
            EmbedBuilder emb = Context.CreateEmbed(title: $"{g.Name} 정보", thumbnailUrl: g.IconUrl);

            emb.AddField("ID", g.Id, true);
            emb.AddField("생성일", g.CreatedAt.ToUniversalTime().AddHours(9).ToString("yyyy년 MM월 dd일 HH시 mm분 ss초"), true);
            emb.AddEmptyField();

            emb.AddField("기본 채널", g.DefaultChannel != null ? g.DefaultChannel.Mention : "-", true);
            emb.AddField("시스템 메시지 채널", g.SystemChannel != null ? g.SystemChannel.Mention : "-", true);
            emb.AddEmptyField();

            emb.AddField("잠수 채널", g.AFKChannel != null ? $"<#{g.AFKChannel.Id}>" : "-", true);
            emb.AddField("비활성화 시간 제한", $"{g.AFKTimeout / 60}분", true);
            emb.AddEmptyField();

            emb.AddField("채널 규칙 및 지침", g.RulesChannel != null ? g.RulesChannel.Mention : "-", true);
            emb.AddField("커뮤니티 업데이트 채널", g.PublicUpdatesChannel != null ? g.PublicUpdatesChannel.Mention : "-", true);
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
            emb.AddField("2단계 인증", g.MfaLevel == MfaLevel.Disabled ? "비활성화" : "활성화", true);

            await Context.MsgReplyEmbedAsync(emb.Build());
        }
    }
}
