using Discord;
using Discord.Commands;
using NCalc;
using Newtonsoft.Json;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
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
            await Context.MsgReplyEmbedAsync($"{Program.Client.Latency}ms");
        }
        
        [Command("봇 초대 링크"), Alias("초대 링크", "초대", "봇 초대")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("올리브토스트를 초대할 수 있는 초대 링크입니다")]
        public async Task BotInvite()
        {
            await Context.MsgReplyEmbedAsync($"[올리브토스트 초대 링크]({Utility.GetInvite()})");
        }

        [Command("핑퐁"), Alias("올토야", "올토님")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("인공지능 올리브토스트와 대화할 수 있습니다")]
        public async Task PingPong([Remainder, Name("문장")] string text)
        {
            using HttpClient httpClient = new HttpClient();
            using HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("POST"), $"https://builder.pingpong.us/api/builder/6083d376e4b078d873a93669/integration/v0.2/custom/{Context.User.Id}");

            request.Headers.TryAddWithoutValidation("Authorization", ConfigManager.Get("PINGPONG_TOKEN"));
            request.Content = new StringContent("{\"request\": {\"query\": \"" + text + "\"}}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            await ReplyAsync(await response.Content.ReadAsStringAsync());
            PingPongResult result = JsonConvert.DeserializeObject<PingPongResult>(await response.Content.ReadAsStringAsync());

            string resTxt = null;
            string resImg = null;

            foreach(var reply in result.response.replies)
            {
                switch (reply.type)
                {
                    case "text":
                        if (resTxt == null)
                            resTxt = reply.text;
                        break;
                    case "image":
                        if (resImg == null)
                            resImg = reply.image.url;
                        break;
                }
            }

            await Context.MsgReplyEmbedAsync(Context.CreateEmbed(description: resTxt, imgUrl: resImg).Build());
        }

        [Command("계산"), Alias("계산기")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("주어진 수식을 계산합니다\n[이곳](https://github.com/ncalc/ncalc/wiki)에서 사용방법을 확인할 수 있습니다")]
        public async Task Calc([Remainder, Name("수식")] string input)
        {
            Expression exp = new Expression(input, EvaluateOptions.IgnoreCase);

            if (exp.HasErrors())
            {
                EmbedBuilder emb = Context.CreateEmbed();
                emb.AddField("오류 발생!", $"{exp.Error}\n\n[사용방법 보기](https://github.com/ncalc/ncalc/wiki)");

                await Context.MsgReplyEmbedAsync(emb.Build());

                return;
            }

            await Context.MsgReplyEmbedAsync(exp.Evaluate());
        }

        [Command("팩토리얼"), Alias("팩")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("주어진 수의 팩토리얼을 계산합니다\nn에는 807이하의 정수만 입력할 수 있습니다")]
        public async Task Factorial(int n)
        {
            if (n > 807 || n < 0)
            {
                await Context.MsgReplyEmbedAsync("0 이상, 807 이하의 정수만 입력할 수 있습니다");
                return;
            }

            BigInteger a = 1;
            for (int i = n; i > 0; i--)
            {
                a *= i;
            }

            await Context.MsgReplyEmbedAsync($"{n}! = **{a}**");
        }
    }
}
