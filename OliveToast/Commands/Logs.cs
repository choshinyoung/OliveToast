using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OliveToast.Managements.OliveGuild.GuildSetting;
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

        [Command("로그 타입 설정"), Alias("로그 타입")]
        [RequirePermission(PermissionType.ManageBotSetting), RequireContext(ContextType.Guild)]
        [Summary("지정된 로그 타입을 활성화하거나 비활성화합니다\n여러 타입을 한번에 입력할 수 있습니다\n\n로그 타입: `메시지수정`, `메시지삭제`, `채널생성`, `채널삭제`, `채널수정`, `서버수정`, `초대링크생성`, `초대링크제거`, `반응추가`, `반응삭제`, `모든반응삭제`, `역할추가`, `역할삭제`, `역할수정`, `차단`, `차단해제`, `입장`, `퇴장`, `유저수정`, `음성상태수정`, `음성서버수정`")]
        public async Task SetLogType([Name("타입")] params string[] types)
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;

            string[] names = Enum.GetNames(typeof(LogTypes));

            List<string> r = new List<string>(), a = new List<string>(), u = new List<string>();
            foreach(string type in types)
            {
                if (!names.Contains(type))
                {
                    u.Add(type);
                    continue;
                }

                LogTypes t = (LogTypes)Array.IndexOf(names, type);
                if (setting.LogType.Contains(t))
                {
                    setting.LogType.Remove(t);
                    r.Add(t.ToString());
                }
                else
                {
                    setting.LogType.Add(t);
                    a.Add(t.ToString());
                }
            }

            OliveGuild.Set(Context.Guild.Id, g => g.Setting, setting);

            string content = "";
            if (r.Count > 0 && a.Count > 0)
            {
                content = $"{string.Join("`, ", a.Select(s => $"`{s}")).을를("`")} 활성화하고 {string.Join("`, ", r.Select(s => $"`{s}")).을를("`")} 비활성화했어요\n";
            }
            else if (a.Count > 0)
            {
                content = $"{string.Join("`, ", a.Select(s => $"`{s}")).을를("`")} 활성화했어요\n";
            }
            else if (r.Count > 0)
            {
                content = $"{string.Join("`, ", r.Select(s => $"`{s}")).을를("`")} 비활성화했어요\n";
            }

            if (u.Count > 0)
            {
                content += $"\n{string.Join("`, ", u.Select(s => $"`{s}")).은는("`")} 알 수 없는 타입이에요\n{string.Join(", ", names.Select(s => $"`{s}`"))} 중 하나를 선택할 수 있어요";
            }

            await Context.MsgReplyEmbedAsync(content);
        }

        [Command("로그 설정 확인"), Alias("로그 설정 보기", "로그 채널 보기", "로그 채널", "로그 타입 확인", "로그 타입 보기", "로그 타입")]
        [RequirePermission(PermissionType.ManageBotSetting), RequireContext(ContextType.Guild)]
        [Summary("설정된 로그 채널을 확인합니다")]
        public async Task SeeLogChannel()
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;

            EmbedBuilder emb = Context.CreateEmbed(title: "로그 타입");

            ulong? id = setting.LogChannelId;
            if (!id.HasValue || !Context.Guild.Channels.Any(c => c.Id == id))
            {
                emb.AddField("로그 채널", "로그 채널이 설정되지 않았어요");
            }
            else
            {
                emb.AddField("로그 채널", $"현재 로그 채널은 {Context.Guild.GetTextChannel(id.Value).Name.이()}에요");
            }

            string[] names = Enum.GetNames(typeof(LogTypes));

            for (int i = 0; i < names.Length; i++)
            {
                emb.AddField(names[i], setting.LogType.Contains((LogTypes)i).ToEmoji(), true);
            }

            await Context.MsgReplyEmbedAsync(emb.Build());
        }

        [Command("로그 설정 초기화"), Alias("로그 초기화")]
        [RequirePermission(PermissionType.ManageBotSetting), RequireContext(ContextType.Guild)]
        [Summary("로그 설정을 초기화합니다")]
        public async Task ResetLogType()
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;

            setting.LogChannelId = null;
            setting.LogType = new List<LogTypes> { LogTypes.메시지수정, LogTypes.메시지삭제 };

            OliveGuild.Set(Context.Guild.Id, g => g.Setting, setting);

            await Context.MsgReplyEmbedAsync("로그 설정을 초기화했어요");
        }
    }
}
