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
        [Summary("올리브토스트로 말을 할 수 있습니다")]
        public async Task Say([Remainder, Name("말")] string input)
        {
            await ReplyAsync(input);

            if (!Context.IsPrivate && Context.Guild.CurrentUser.GetPermissions((IGuildChannel)Context.Channel).ManageMessages)
            {
                await Context.Message.DeleteAsync();
            }
        }
    }
}
