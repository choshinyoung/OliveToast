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
    [Name("게임")]
    [RequireCategoryEnable(CategoryType.Game)]
    public class Game : ModuleBase<SocketCommandContext>
    {
        [Command("주사위"), Alias("주사위 굴리기")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("주사위를 굴립니다\n면 수는 생략할 수 있습니다")]
        public async Task Dice([Name("면 수")] int count = 6)
        {
            await Context.MsgReplyEmbedAsync($"{new Random().Next(1, count + 1)}!");
        }
    }
}
