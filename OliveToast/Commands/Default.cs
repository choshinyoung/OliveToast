using Discord;
using Discord.Commands;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Text;
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
