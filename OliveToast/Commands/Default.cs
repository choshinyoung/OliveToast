using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
            await Context.MsgReplyEmbedAsync(Program.Client.Latency);
        }
        
        [Command("봇 초대 링크"), Alias("초대 링크", "초대", "봇 초대")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("올리브토스트를 초대할 수 있는 초대 링크입니다")]
        public async Task BotInvite()
        {
            await Context.MsgReplyEmbedAsync($"[올리브토스트 초대 링크](https://discord.com/oauth2/authorize?client_id={Program.Client.CurrentUser.Id}&scope=bot&permissions=2416241734)");
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

        public class PingPongResult
        {
            public Response response { get; set; }
            public string version { get; set; }

            public class Response
            {
                public List<Reply> replies { get; set; }

                public class Reply
                {
                    public From from { get; set; }
                    public string type { get; set; }
                    public string text { get; set; }
                    public Image image { get; set; }

                    public class From
                    {
                        public double score { get; set; }
                        public string name { get; set; }
                        public string link { get; set; }
                        public string from { get; set; }
                    }

                    public class Image
                    {
                        public string url { get; set; }
                    }
                }
            }
        }
    }
}
