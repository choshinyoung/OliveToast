using Discord;
using Discord.Commands;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OliveToast.Managements.RequirePermission;

namespace OliveToast.Commands
{
    [HideInHelp]
    public class Helps : ModuleBase<SocketCommandContext>
    {
        [Command("도움"), Alias("도움말")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커맨드 목록을 볼 수 있습니다")]
        public async Task Help()
        {
            EmbedBuilder emb = Context.CreateEmbed(title: "도움말", description: $"\"{EventHandler.prefix}도움 `카테고리/커맨드`\"로 자세한 사용법 보기");

            foreach (ModuleInfo module in Program.Command.Modules)
            {
                if (!module.HaveAttribute<HideInHelp>())
                {
                    List<CommandInfo> cmds = new();
                    foreach (CommandInfo cmd in module.Commands)
                    {
                        if (!cmd.HaveAttribute<HideInHelp>() && cmd.Summary != null && !cmds.Where(c => c.Name == cmd.Name).Any())
                            cmds.Add(cmd);
                    }

                    emb.AddField(module.Name, string.Join(' ', cmds.Select(c => $"`{EventHandler.prefix}{c.Name}`")));
                }
            }

            await Context.MsgReplyEmbedAsync(emb.Build());
        }

        [Command("도움"), Alias("도움말")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("특정 카테고리 또는 커맨드의 자세한 정보를 확인합니다")]
        public async Task Help([Remainder, Name("카테고리/커맨드")] string name)
        {
            List<ModuleInfo> moduleInfos = Program.Command.Modules.Where(m => m.Name == name && !m.HaveAttribute<HideInHelp>()).ToList();
            if (moduleInfos.Any())
            {
                List<CommandInfo> cmds = new();
                foreach (CommandInfo cmd in moduleInfos.FirstOrDefault().Commands)
                {
                    if (!cmd.HaveAttribute<HideInHelp>() && cmd.Summary != null && !cmds.Where(c => c.Name == cmd.Name).Any())
                        cmds.Add(cmd);
                }

                EmbedBuilder emb = Context.CreateEmbed(title: moduleInfos.FirstOrDefault().Name);

                foreach (CommandInfo info in cmds)
                {
                    emb.AddField($"{EventHandler.prefix}{info.Name} {string.Join(' ', info.Parameters.Select(p => $"`{p.Name}`"))}", info.Summary.Split('\n')[0]);
                }

                await Context.MsgReplyEmbedAsync(emb.Build());
            }
            else
            {
                List<CommandInfo> commandInfos = Program.Command.Commands.Where(c => c.Aliases.Contains(name) && c.Summary != null && !c.HaveAttribute<HideInHelp>()).ToList();
                if (commandInfos.Any())
                {
                    EmbedBuilder emb = Context.CreateEmbed();

                    foreach (CommandInfo info in commandInfos)
                    {
                        string cmdName = info.Aliases.Count > 1 ? string.Join("/", info.Aliases) : info.Name;

                        string param = "";
                        foreach (ParameterInfo paramInfo in info.Parameters)
                        {
                            param += $"`{paramInfo.Name}";

                            Type t = paramInfo.Type;
                            if (t == typeof(bool))
                                param += "<bool>";
                            else if (t == typeof(char))
                                param += "<문자>";
                            else if (t == typeof(byte) || t == typeof(sbyte) || t == typeof(short) || t == typeof(int) || t == typeof(long) || t == typeof(ushort) || t == typeof(uint) || t == typeof(ulong) || t == typeof(float) || t == typeof(double) || t == typeof(decimal))
                                param += "<숫자>";
                            else if (t == typeof(string))
                                param += "<텍스트>";
                            else if (t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(TimeSpan))
                                param += "<시간>";
                            else if (typeof(Enum).IsAssignableFrom(t))
                                param += "<enum>";
                            else if (typeof(IUser).IsAssignableFrom(t))
                                param += "<유저>";
                            else if (typeof(IChannel).IsAssignableFrom(t))
                                param += "<채널>";
                            else if (typeof(IRole).IsAssignableFrom(t))
                                param += "<역할>";
                            else if (typeof(IMessage).IsAssignableFrom(t))
                                param += "<메시지>";

                            param += "` ";
                        }

                        string permission = null;

                        if (info.HavePrecondition<RequirePermission>())
                        {
                            permission = ((RequirePermission)info.Preconditions.Where(p => p.GetType() == typeof(RequirePermission)).FirstOrDefault()).Permission switch
                            {
                                PermissionType.ManageCommand => "커맨드 관리",
                                PermissionType.ChangeAnnouncementChannel => "공지 채널 변경",
                                PermissionType.ManageBotSetting => "봇 설정 변경",
                                PermissionType.CreateVote => "투표",
                                PermissionType.SpeakByBot => "봇으로 말하기",
                                PermissionType.ServerAdmin => "서버 어드민",
                                PermissionType.BotAdmin => "봇 어드민",
                                _ => null
                            };
                        }
                        permission = permission != null ? $"\n - `<{permission}>` 권한이 필요합니다" : "";

                        emb.AddField($"{EventHandler.prefix}{cmdName} {param}", $"{info.Summary}{permission}");
                    }

                    await Context.MsgReplyEmbedAsync(emb.Build());
                }
                else
                {
                    await Context.MsgReplyEmbedAsync("해당 커맨드 또는 카테고리는 존재하지 않습니다");
                }
            }
        }
    }
}
