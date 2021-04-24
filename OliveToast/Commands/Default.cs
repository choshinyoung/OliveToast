using Discord;
using Discord.Commands;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OliveToast.Managements.RequireCategoryEnable;
using static OliveToast.Managements.RequirePermission;

namespace OliveToast.Commands
{
    [Name("일반")]
    [RequireCategoryEnable(CategoryType.Default)]
    public class Default : ModuleBase<SocketCommandContext>
    {
        [Command("안녕")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("올리브토스트와 인사를 할 수 있습니다")]
        public async Task Hello()
        {
            await Context.MsgReplyEmbedAsync("안녕하세요!");
        }
        
        [Command("핑")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("봇의 응답 속도를 확인합니다")]
        public async Task Ping()
        {
            await Context.MsgReplyEmbedAsync(Program.Client.Latency);
        }
        
        [Command("봇 초대 링크"), Alias("초대 링크", "초대", "봇 초대")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("올리브토스트를 초대할 수 있는 초대 링크입니다")]
        public async Task BotInvite()
        {
            await Context.MsgReplyEmbedAsync($"[올리브토스트 초대 링크](https://discord.com/oauth2/authorize?client_id={Program.Client.CurrentUser.Id}&scope=bot&permissions=2416241734)");
        }
    }
}
