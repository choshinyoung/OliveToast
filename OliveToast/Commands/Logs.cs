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
    [Name("로그")]
    [RequireCategoryEnable(CategoryType.Log)]
    public class Logs : ModuleBase<SocketCommandContext>
    {
        [Command("로그 채널 설정"), Alias("로그 채널")]
        [RequirePermission(PermissionType.ManageBotSetting), RequireContext(ContextType.Guild)]
        [Summary("로그 채널을 설정합니다")]
        public async Task SetLogChannel([Remainder, Name("채널")] SocketTextChannel channel)
        {

        }

        [Command("로그 채널 확인"), Alias("로그 채널 보기", "로그 채널")]
        [RequirePermission(PermissionType.ManageBotSetting), RequireContext(ContextType.Guild)]
        [Summary("설정된 로그 채널을 확인합니다")]
        public async Task SeeLogChannel()
        {

        }
    }
}
