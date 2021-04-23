using Discord;
using Discord.Commands;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OliveToast.Commands
{
    [Name("일반")]
    public class Default : ModuleBase<SocketCommandContext>
    {
        [Command("도움"), Alias("도움말")]
        [Summary("커맨드 목록을 볼 수 있습니다.")]
        public async Task Help()
        {
            EmbedBuilder emb = Context.CreateEmbed(title: "도움말", description: $"`{EventHandler.prefix}카테고리/커맨드`로 자세한 사용법 보기");
            
            foreach(ModuleInfo module in Program.Command.Modules)
            {
                if (!module.HaveAttribute<HideInHelp>())
                {
                    List<CommandInfo> cmds = new();
                    foreach(CommandInfo cmd in module.Commands)
                    {
                        if (!cmd.HaveAttribute<HideInHelp>() && !cmds.Where(c => c.Name == cmd.Name).Any())
                            cmds.Add(cmd);
                    }

                    emb.AddField(module.Name, string.Join(' ', cmds.Select(c => $"`{EventHandler.prefix}{c.Name}`")));
                }
            }

            await Context.MsgReplyEmbedAsync(emb.Build());
        }

        [Command("도움"), Alias("도움말")]
        [Summary("특정 카테고리 또는 커맨드의 자세한 정보를 확인합니다.")]
        public async Task Help([Remainder, Name("카테고리/커맨드")]string name)
        {
            List<ModuleInfo> moduleInfos = Program.Command.Modules.Where(m => m.Name == name).ToList();
            if (moduleInfos.Any())
            {
                List<CommandInfo> cmds = new();
                foreach (CommandInfo cmd in moduleInfos.FirstOrDefault().Commands)
                {
                    if (!cmd.HaveAttribute<HideInHelp>() && !cmds.Where(c => c.Name == cmd.Name).Any())
                        cmds.Add(cmd);
                }

                EmbedBuilder emb = Context.CreateEmbed(title: moduleInfos.FirstOrDefault().Name);

                foreach (CommandInfo info in cmds)
                {
                    emb.AddField($"{EventHandler.prefix}{info.Name} {string.Join(' ', info.Parameters.Select(p => $"`{p.Name}`"))}", info.Summary);
                }
                
                await Context.MsgReplyEmbedAsync(emb.Build());
            }
            else 
            {
                List<CommandInfo> commandInfos = Program.Command.Commands.Where(c => c.Aliases.Contains(name) && !c.HaveAttribute<HideInHelp>()).ToList();
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

                        emb.AddField($"{EventHandler.prefix}{cmdName} {param}", info.Summary);
                    }

                    await Context.MsgReplyEmbedAsync(emb.Build());
                }
                else
                {
                    await Context.MsgReplyEmbedAsync("해당 커맨드 또는 카테고리는 존재하지 않습니다.");
                }
            }
        }

        [Command("안녕")]
        [Summary("올리브토스트와 인사를 해보아요!")]
        public async Task Hello()
        {
            await Context.MsgReplyEmbedAsync("안녕하세요!");
        }

        [Command("핑")]
        [Summary("봇의 응답 속도를 확인합니다.")]
        public async Task Ping()
        {
            await Context.MsgReplyEmbedAsync(Program.Client.Latency);
        }
    }
}
