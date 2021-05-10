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
    [Name("텍스트")]
    [RequireCategoryEnable(CategoryType.Text)]
    public class Text : ModuleBase<SocketCommandContext>
    {
        [Command("말하기")]
        [RequirePermission(PermissionType.SpeakByBot)]
        [Summary("올리브토스트로 말을 할 수 있습니다\n`말`은 500자 이하여야 합니다")]
        public async Task Say([Remainder, Name("말")] string input)
        {
            if (input.Length > 500)
            {
                await Context.MsgReplyEmbedAsync("도배 방지를 위해 500자 이하만 입력할 수 있어요");
                return;
            }

            await ReplyAsync(input);

            if (!Context.IsPrivate && Context.Guild.CurrentUser.GetPermissions((IGuildChannel)Context.Channel).ManageMessages)
            {
                await Context.Message.DeleteAsync();
            }
        }

        [Command("거꾸로"), Alias("로꾸거")]
        [RequirePermission(PermissionType.SpeakByBot)]
        [Summary("말을 거꾸로 합니다\n`말`은 500자 이하여야 합니다")]
        public async Task Reverse([Remainder, Name("말")] string input)
        {
            if (input.Length > 500)
            {
                await Context.MsgReplyEmbedAsync("도배 방지를 위해 500자 이하만 입력할 수 있어요");
                return;
            }

            await ReplyAsync(string.Join("", input.Reverse()));
        }
    }
}
