using Discord;
using Discord.Commands;
using OliveToast.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OliveToast.Utilities.RequirePermission;

namespace OliveToast.Commands
{
    public class Helps : ModuleBase<SocketCommandContext>
    {
        [Command("도움"), Alias("도움말")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커맨드 목록을 볼 수 있습니다")]
        public async Task Help()
        {
            EmbedBuilder emb = Context.CreateEmbed(title: "도움말", description: $"\"{CommandEventHandler.prefix}도움 `카테고리/커맨드`\"로 자세한 사용법 보기");

            List<ModuleInfo> modules = Program.Command.Modules.Where(m => m.HavePrecondition<RequireCategoryEnable>()).ToList();
            modules.Sort((m1, m2) => RequireCategoryEnable.GetCategory(m1).CompareTo(RequireCategoryEnable.GetCategory(m2)));

            foreach (ModuleInfo module in modules.Where(m => m.Commands.Count > 0 && m.GetType() != typeof(Admin)))
            {
                List<CommandInfo> cmds = new();
                foreach (CommandInfo cmd in module.Commands)
                {
                    if (cmd.Summary != null && !cmds.Any(c => c.Name == cmd.Name))
                        cmds.Add(cmd);
                }

                emb.AddField(module.Name, string.Join(' ', cmds.Select(c => $"`{CommandEventHandler.prefix}{c.Name}`")));
            }

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("도움"), Alias("도움말")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("특정 카테고리 또는 커맨드의 자세한 정보를 확인합니다")]
        public async Task Help([Remainder, Name("카테고리/커맨드")] string name)
        {
            List<ModuleInfo> moduleInfos = Program.Command.Modules.Where(m => m.Name == name).ToList();
            if (moduleInfos.Any())
            {
                List<CommandInfo> cmds = new();
                foreach (CommandInfo cmd in moduleInfos.FirstOrDefault().Commands)
                {
                    if (cmd.Summary != null && !cmds.Any(c => c.Name == cmd.Name))
                        cmds.Add(cmd);
                }

                EmbedBuilder emb = Context.CreateEmbed(title: moduleInfos.FirstOrDefault().Name);

                foreach (CommandInfo info in cmds)
                {
                    emb.AddField($"{CommandEventHandler.prefix}{info.Name} {string.Join(' ', info.Parameters.Where(p => p.Name != "").Select(p => $"`{p.Name}`"))}", info.Summary.Split('\n')[0]);
                }

                await Context.ReplyEmbedAsync(emb.Build());
            }
            else
            {
                List<CommandInfo> commandInfos = Program.Command.Commands.Where(c => c.Aliases.Contains(name) && c.Summary != null).ToList();
                if (commandInfos.Any())
                {
                    EmbedBuilder emb = Context.CreateEmbed();

                    foreach (CommandInfo info in commandInfos)
                    {
                        string aliases = info.Aliases.Count > 1 ? string.Join(" ", info.Aliases.Where(a => a != info.Name).Select(a => $"`-{a}`")) + "\n" : "";

                        string param = "";
                        foreach (ParameterInfo paramInfo in info.Parameters)
                        {
                            if (paramInfo.Name == "")
                            {
                                continue;
                            }

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
                            PermissionType pms = ((RequirePermission)info.Preconditions.Where(p => p.GetType() == typeof(RequirePermission)).FirstOrDefault()).Permission;

                            if (pms != PermissionType.UseBot)
                            {
                                permission = PermissionToString(pms);
                            }
                        }
                        permission = permission != null ? $"\n - 이 커맨드를 실행하려면 `<{permission}>` 권한이 필요해요" : "";

                        emb.AddField($"{CommandEventHandler.prefix}{info.Name} {param}", $"{aliases}\n{info.Summary}{permission}");
                    }

                    await Context.ReplyEmbedAsync(emb.Build());
                }
                else
                {
                    await Context.ReplyEmbedAsync("해당 커맨드 또는 카테고리가 없어요");
                }
            }
        }
    }
}
