using Discord;
using Discord.Commands;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OliveToast.Commands
{
    public class Default : ModuleBase<SocketCommandContext>
    {
        [Command("도움"), Alias("도움말")]
        [Summary("커맨드 목록을 볼 수 있습니다.")]
        public async Task Help()
        {
            EmbedBuilder emb = Context.CreateEmbed(title: "도움말")
                .AddField("+일반", "`+도움` `+안녕` `+핑`")
                .AddField("+정보", "`+서버 정보` `+채널 정보` `+역할 정보` `+유저 정보` `+봇 정보`");

            await Context.MsgReplyEmbedAsync(emb.Build());
        }

        [Command("도움"), Alias("도움말")]
        [Summary("특정 커맨드의 자세한 정보를 확인힙니다.")]
        public async Task Help([Name("커맨드"), Remainder]string cmdName)
        {
            List<CommandInfo> infos = Program.Command.Commands.Where(cmd => (cmd.Name == cmdName || cmd.Aliases.Contains(cmdName)) && cmd.Summary != null).ToList();

            if (infos.Count == 0)
            {
                await Context.MsgReplyEmbedAsync("해당 커맨드는 존재하지 않습니다.");
                return;
            }

            EmbedBuilder emb = Context.CreateEmbed();

            foreach (CommandInfo info in infos)
            {
                string name = info.Aliases.Count > 1 ? $"[{string.Join(", ", info.Aliases)}]" : info.Name;

                string param = "";
                foreach (ParameterInfo paramInfo in info.Parameters)
                {
                    param += $"`{paramInfo.Summary ?? paramInfo.Name}";

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

                emb.AddField($"{EventHandler.prefix}{name} {param}", info.Summary);
            }

            await Context.MsgReplyEmbedAsync(emb.Build());
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

        [Command("hellothisisverification")]
        [Summary("Koreanbots 확인용 커맨드")]
        public async Task Verification()
        {
            await Context.MsgReplyEmbedAsync("choshinyoung#1795");
        }
    }
}
