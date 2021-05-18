using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
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
            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;

            setting.LogChannelId = channel.Id;

            OliveGuild.Set(Context.Guild.Id, g => g.Setting, setting);

            await Context.MsgReplyEmbedAsync($"로그 채널을 {channel.Name.으로()} 설정했어요");
        }

        [Command("로그 채널 확인"), Alias("로그 채널 보기", "로그 채널")]
        [RequirePermission(PermissionType.ManageBotSetting), RequireContext(ContextType.Guild)]
        [Summary("설정된 로그 채널을 확인합니다")]
        public async Task SeeLogChannel()
        {
            ulong? id = OliveGuild.Get(Context.Guild.Id).Setting.LogChannelId;

            if (!id.HasValue || !Context.Guild.Channels.Any(c => c.Id == id)) 
            {
                await Context.MsgReplyEmbedAsync("로그 채널이 설정되지 않았어요");
                return;
            }

            await Context.MsgReplyEmbedAsync($"현재 로그 채널은 {Context.Guild.GetTextChannel(id.Value).Name.이()}에요");
        }
    }
}
