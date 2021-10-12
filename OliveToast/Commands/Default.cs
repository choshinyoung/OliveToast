using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using OliveToast.Managements.Data;
using OliveToast.Utilities;
using org.mariuszgromada.math.mxparser;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Threading.Tasks;
using static OliveToast.Utilities.RequireCategoryEnable;
using static OliveToast.Utilities.RequirePermission;

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
            await Context.ReplyEmbedAsync("안녕하세요!");
        }
        
        [Command("핑")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("봇의 응답 속도를 확인합니다")]
        public async Task Ping()
        {
            await Context.ReplyEmbedAsync($"{Program.Client.Latency}ms");
        }

        [Command("업타임")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("봇의 업타임을 확인합니다")]
        public async Task Uptime()
        {
            TimeSpan t = DateTime.Now - Program.Uptime;
            await Context.ReplyEmbedAsync($"{t.Days}일 {t.Hours}시간 {t.Minutes}분 {t.Seconds}초");
        }

        [Command("봇 초대 링크"), Alias("초대 링크", "초대", "봇 초대")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("올리브토스트를 초대할 수 있는 초대 링크입니다")]
        public async Task BotInvite()
        {
            await Context.ReplyEmbedAsync($"[올리브토스트 초대 링크]({Utility.GetInvite()})");
        }

        [Command("인공지능"), Alias("핑퐁", "올토야", "올토님")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("인공지능 올리브토스트와 대화할 수 있습니다")]
        public async Task PingPong([Remainder, Name("문장")] string text)
        {
            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(new HttpMethod("POST"), $"https://builder.pingpong.us/api/builder/60a1d801e4b091a94bc5294d/integration/v0.2/custom/{Context.User.Id}");

            request.Headers.TryAddWithoutValidation("Authorization", ConfigManager.Get("PINGPONG_TOKEN"));
            request.Content = new StringContent("{\"request\": {\"query\": \"" + text + "\"}}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            using var response = await httpClient.SendAsync(request);
            PingPongResult result = JsonConvert.DeserializeObject<PingPongResult>(await response.Content.ReadAsStringAsync());

            string resTxt = null;
            string resImg = null;

            foreach (var reply in result.response.replies)
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

            await Context.ReplyEmbedAsync(Context.CreateEmbed(description: resTxt, imgUrl: resImg).Build());
        }

        [Command("계산"), Alias("계산기")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("주어진 수식을 계산합니다")]
        public async Task Calc([Remainder, Name("수식")] string input)
        {
            Expression exp = new(input);

            if (!exp.checkSyntax())
            {
                EmbedBuilder emb = Context.CreateEmbed();
                emb.AddField("오류 발생!", exp.getErrorMessage().Slice(100));

                await Context.ReplyEmbedAsync(emb.Build());

                return;
            }

            object result = exp.calculate();
            result = Math.Round((double)result, 10, MidpointRounding.AwayFromZero);

            await Context.ReplyEmbedAsync(result);
        }

        [Command("팩토리얼"), Alias("팩")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("주어진 수의 팩토리얼을 계산합니다\n`n`에는 807이하의 정수만 입력할 수 있습니다")]
        public async Task Factorial(int n)
        {
            if (n > 807 || n < 0)
            {
                await Context.ReplyEmbedAsync("0 이상, 807 이하의 정수만 입력할 수 있어요");
                return;
            }

            BigInteger a = 1;
            for (int i = n; i > 0; i--)
            {
                a *= i;
            }

            await Context.ReplyEmbedAsync($"{n}! = {a}");
        }
    }
}
