using Discord;
using Discord.Commands;
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
    [RequireCategoryEnable(CategoryType.Setting)]
    public class Settings : ModuleBase<SocketCommandContext>
    {
        [Command("활성화")]
        [RequirePermission(PermissionType.ManageBotSetting)]
        [Summary("지정된 카테고리를 활성화합니다")]
        public async Task Enable([Name("카테고리")] string category)
        {
            if (!CategoryNames.Contains(category))
            {
                await Context.MsgReplyEmbedAsync($"알 수 없는 카테고리에요\n{string.Join(", ", CategoryNames.Select(c => $"`{c}`"))} 중 하나를 선택할 수 있어요");
                return;
            }

            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;
            CategoryType cat = StringToCategory(category);

            if (setting.EnabledCategories.Contains(cat))
            {
                await Context.MsgReplyEmbedAsync($"이미 활성화돼있는 카테고리에요");
                return;
            }

            setting.EnabledCategories.Add(cat);
            OliveGuild.Set(Context.Guild.Id, g => g.Setting, setting);

            await Context.MsgReplyEmbedAsync($"{category} 카테고리를 활성화했어요");
        }

        [Command("비활성화")]
        [RequirePermission(PermissionType.ManageBotSetting)]
        [Summary("지정된 카테고리를 비활성화합니다")]
        public async Task Disable([Name("카테고리")] string category)
        {
            if (!CategoryNames.Contains(category))
            {
                await Context.MsgReplyEmbedAsync($"알 수 없는 카테고리에요\n{string.Join(", ", CategoryNames.Select(c => $"`{c}`"))} 중 하나를 선택할 수 있어요");
                return;
            }

            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;
            CategoryType cat = StringToCategory(category);

            if (!setting.EnabledCategories.Contains(cat))
            {
                await Context.MsgReplyEmbedAsync($"이미 비활성화돼있는 카테고리에요");
                return;
            }

            setting.EnabledCategories.Remove(cat);
            OliveGuild.Set(Context.Guild.Id, g => g.Setting, setting);

            await Context.MsgReplyEmbedAsync($"{category} 카테고리를 비활성화했어요");
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

            await Context.MsgReplyEmbedAsync(emb.Build());
        }
    }
}
