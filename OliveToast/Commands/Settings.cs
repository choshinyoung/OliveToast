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
    [Name("설정")]
    [RequireCategoryEnable(CategoryType.Setting), RequireContext(ContextType.Guild)]
    public class Settings : ModuleBase<SocketCommandContext>
    {
        [Command("활성화")]
        [RequirePermission(PermissionType.ManageBotSetting)]
        [Summary("지정된 카테고리를 활성화합니다\n`일반`, `정보`, `검색`, `게임`, `텍스트`, `이미지`, `커맨드`, `레벨`, `로그`, `설정` 중 하나를 선택할 수 있습니다")]
        public async Task Enable([Name("카테고리")] string category)
        {
            if (!CategoryNames.Contains(category))
            {
                await Context.ReplyEmbedAsync($"알 수 없는 카테고리에요\n{string.Join(", ", CategoryNames.Select(c => $"`{c}`"))} 중 하나를 선택할 수 있어요");
                return;
            }

            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;
            CategoryType cat = StringToCategory(category);

            if (setting.EnabledCategories.Contains(cat))
            {
                await Context.ReplyEmbedAsync($"이미 활성화돼있는 카테고리에요");
                return;
            }

            setting.EnabledCategories.Add(cat);
            OliveGuild.Set(Context.Guild.Id, g => g.Setting, setting);

            await Context.ReplyEmbedAsync($"{category} 카테고리를 활성화했어요");
        }

        [Command("비활성화")]
        [RequirePermission(PermissionType.ManageBotSetting)]
        [Summary("지정된 카테고리를 비활성화합니다")]
        public async Task Disable([Name("카테고리")] string category)
        {
            if (!CategoryNames.Contains(category))
            {
                await Context.ReplyEmbedAsync($"알 수 없는 카테고리에요\n{string.Join(", ", CategoryNames.Select(c => $"`{c}`"))} 중 하나를 선택할 수 있어요");
                return;
            }

            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;
            CategoryType cat = StringToCategory(category);

            if (cat == CategoryType.Setting)
            {
                await Context.ReplyEmbedAsync($"비활성화 할 수 없는 카테고리에요");
                return;
            }

            if (!setting.EnabledCategories.Contains(cat))
            {
                await Context.ReplyEmbedAsync($"이미 비활성화돼있는 카테고리에요");
                return;
            }

            setting.EnabledCategories.Remove(cat);
            OliveGuild.Set(Context.Guild.Id, g => g.Setting, setting);

            await Context.ReplyEmbedAsync($"{category} 카테고리를 비활성화했어요");
        }

        [Command("활성화 확인"), Alias("활성화 보기", "활성화"), Priority(1)]
        [RequirePermission(PermissionType.ManageBotSetting)]
        [Summary("활성화돼있는 카테고리들을 확인합니다")]
        public async Task SeeEnabledCategories()
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;

            EmbedBuilder emb = Context.CreateEmbed(title: "카테고리 활성화 확인");

            foreach(CategoryType cat in Enum.GetValues(typeof(CategoryType)))
            {
                emb.AddField(CategoryToString(cat), setting.EnabledCategories.Contains(cat).ToEmoji(), true);
            }

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("권한 설정"), Alias("권한")]
        [RequirePermission(PermissionType.ServerAdmin)]
        [Summary("역할별로 봇 사용 권한을 설정합니다\n`봇 사용`, `커맨드 관리`, `설정 관리`, `봇으로 말하기` 중 하나를 선택할 수 있습니다")]
        public async Task SetPermissionRole([Name("권한")] string permission, [Name("")] string permission2, [Remainder, Name("역할")] SocketRole role)
        {
            permission += $" {permission2}";

            if (!PermissionNames.Contains(permission))
            {
                await Context.ReplyEmbedAsync($"알 수 없는 권한이에요\n{string.Join(", ", PermissionNames[..(PermissionNames.Length - 2)].Select(c => $"`{c}`"))} 중 하나를 선택할 수 있어요");
                return;
            }

            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;
            PermissionType perm = StringToPermission(permission);

            if (perm == PermissionType.ServerAdmin || perm == PermissionType.BotAdmin)
            {
                await Context.ReplyEmbedAsync($"설정할 수 없는 권한이에요");
                return;
            }

            string sPerm = perm.ToString();

            if (setting.PermissionRoles.ContainsKey(sPerm))
            {
                setting.PermissionRoles[sPerm] = role.Id;
            }
            else
            {
                setting.PermissionRoles.Add(sPerm, role.Id);
            }

            OliveGuild.Set(Context.Guild.Id, g => g.Setting, setting);

            await Context.ReplyEmbedAsync($"{permission} 권한을 설정했어요\n이제 {role.Mention.이()}나 그 위의 역할이 있는 유저는 {permission} 권한이 필요한 커맨드를 사용할 수 있어요");
        }

        [Command("권한 확인"), Alias("권한 보기", "권한")]
        [RequirePermission(PermissionType.ServerAdmin)]
        [Summary("봇 사용 권한을 확인합니다")]
        public async Task SeePermissionRole()
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;

            EmbedBuilder emb = Context.CreateEmbed(title: "권한 확인");

            foreach (PermissionType perm in ((PermissionType[])Enum.GetValues(typeof(PermissionType)))[..(PermissionNames.Length - 2)])
            {
                bool roleExist = setting.PermissionRoles.ContainsKey(perm.ToString()) && Context.Guild.Roles.Any(r => r.Id == setting.PermissionRoles[perm.ToString()]);
                emb.AddField(PermissionToString(perm), roleExist ? Context.Guild.GetRole(setting.PermissionRoles[perm.ToString()]).Mention : "기본값", true);
            }

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("권한 제거"), Alias("권한 설정 취소")]
        [RequirePermission(PermissionType.ServerAdmin)]
        [Summary("봇 사용 권한을 제거합니다\n`봇 사용`, `커맨드 관리`, `설정 관리`, `봇으로 말하기` 중 하나를 선택할 수 있습니다")]
        public async Task RemovePermissionRole([Name("권한")] string permission, [Name("")] string permission2)
        {
            permission += $" {permission2}";

            if (!PermissionNames.Contains(permission))
            {
                await Context.ReplyEmbedAsync($"알 수 없는 권한이에요\n{string.Join(", ", PermissionNames[..(PermissionNames.Length - 2)].Select(c => $"`{c}`"))} 중 하나를 선택할 수 있어요");
                return;
            }

            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;
            PermissionType perm = StringToPermission(permission);

            if (perm == PermissionType.ServerAdmin || perm == PermissionType.BotAdmin)
            {
                await Context.ReplyEmbedAsync($"설정할 수 없는 권한이에요");
                return;
            }

            string sPerm = perm.ToString();

            if (!setting.PermissionRoles.ContainsKey(sPerm))
            {
                await Context.ReplyEmbedAsync("해당 권한은 설정되지 않아서 제거할 수 없어요");
                return;
            }

            setting.PermissionRoles.Remove(sPerm);

            OliveGuild.Set(Context.Guild.Id, g => g.Setting, setting);

            await Context.ReplyEmbedAsync($"이제 {permission} 제거돼서 기본값으로 작동해요");
        }

        [Command("환영 메시지 설정"), Alias("환영메시지 설정")]
        [RequirePermission(PermissionType.ManageBotSetting)]
        [Summary("유저가 입장할 때 보내는 메시지를 설정합니다")]
        public async Task SetJoinMessage([Name("메시지"), Remainder] string msg)
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;

            string prv = setting.JoinMessage;

            setting.JoinMessage = msg;

            OliveGuild.Set(Context.Guild.Id, g => g.Setting, setting);

            EmbedBuilder emb = Context.CreateEmbed("환영메시지를 설정했어요");
            emb.AddField("이전 메시지", prv);
            emb.AddField("새 메시지", msg);

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("환영 메시지 보기"), Alias("환영메시지 보기", "환영 메시지 확인", "환영메시지 확인")]
        [RequirePermission(PermissionType.ManageBotSetting)]
        [Summary("환영 메시지를 확인합니다")]
        public async Task CheckJoinMessage()
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;

            await Context.ReplyEmbedAsync(setting.JoinMessage);
        }

        [Command("퇴장 메시지 설정"), Alias("퇴장메시지 설정")]
        [RequirePermission(PermissionType.ManageBotSetting)]
        [Summary("유저가 서버를 나갔을 때 보내는 메시지를 설정합니다")]
        public async Task SetLeaveMessage([Name("메시지"), Remainder] string msg)
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;

            string prv = setting.LeaveMessage;

            setting.LeaveMessage = msg;

            OliveGuild.Set(Context.Guild.Id, g => g.Setting, setting);

            EmbedBuilder emb = Context.CreateEmbed("퇴장메시지를 설정했어요");
            emb.AddField("이전 메시지", prv);
            emb.AddField("새 메시지", msg);

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("퇴장 메시지 보기"), Alias("퇴장메시지 보기", "퇴장 메시지 확인", "퇴장메시지 확인")]
        [RequirePermission(PermissionType.ManageBotSetting)]
        [Summary("퇴장 메시지를 확인합니다")]
        public async Task CheckLeaveMessage()
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;

            await Context.ReplyEmbedAsync(setting.LeaveMessage);
        }
    }
}
