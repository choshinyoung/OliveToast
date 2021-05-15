using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static OliveToast.Managements.RequireCategoryEnable;
using static OliveToast.Managements.RequirePermission;

namespace OliveToast.Commands
{
    [Name("검색")]
    [RequireCategoryEnable(CategoryType.Search)]
    public class Search : ModuleBase<SocketCommandContext>
    {
        [Command("위키"), Alias("위키백과")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("위키백과에서 검색합니다")]
        public async Task Wiki([Remainder, Name("검색어")] string input)
        {
            WebRequest request = WebRequest.Create("http://aiopen.etri.re.kr:8000/WikiQA");
            string postData = "{ \"access_key\": \"" + ConfigManager.Get("AIOPEN_TOKEN") + "\", \"argument\": {\"question\": \"" + input + "\", \"type\": hybridqa } }";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            using WebResponse response = request.GetResponse();
            using StreamReader reader = new StreamReader(response.GetResponseStream());

            string responseFromServer = reader.ReadToEnd();

            List<WikiResult.AnswerInfo> answers = JsonConvert.DeserializeObject<WikiResult>(responseFromServer).return_object.WiKiInfo.AnswerInfo;

            if (answers.Count >= 1 && answers.First().answer != "정의를 찾지 못했습니다.")
            {
                EmbedBuilder emb = Context.CreateEmbed(title: "위키백과", description: answers.First().answer);
                await Context.MsgReplyEmbedAsync(emb.Build());
            }
            else
            {
                EmbedBuilder emb = Context.CreateEmbed(title: "검색 실패", description: $"\"그, `{input}` 혹시 보신 적이 있으십니까?\"\n\"아니, 잘 몰라요\"");
                await Context.MsgReplyEmbedAsync(emb.Build());
            }
        }

        [Command("사전"), Alias("단어", "어휘")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("사전에서 단어를 검색합니다")]
        public async Task Dic([Remainder, Name("검색어")] string input)
        {
            WebRequest request = WebRequest.Create("http://aiopen.etri.re.kr:8000/WiseWWN/Word");
            string postData = "{ \"access_key\": \"" + ConfigManager.Get("AIOPEN_TOKEN") + "\", \"argument\": {\"word\": \"" + input + "\" } }";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            using WebResponse response = request.GetResponse();
            using StreamReader reader = new StreamReader(response.GetResponseStream());

            string responseFromServer = reader.ReadToEnd();

            List<DicResult.WWNWordInfo> answers = JsonConvert.DeserializeObject<DicResult>(responseFromServer.Replace("WWN WordInfo", "WWN_WordInfo")).return_object.WWN_WordInfo;

            if (answers.Where(a => a.WordInfo.Count != 0).Any())
            {
                EmbedBuilder emb = Context.CreateEmbed(title: "사전", description: "");

                int index = 1;
                foreach (var wwn in answers)
                {
                    foreach (var word in wwn.WordInfo)
                    {
                        emb.Description += $"{index}. {word.Definition}\n";
                        index++;
                    }
                }

                await Context.MsgReplyEmbedAsync(emb.Build());
            }
            else
            {
                EmbedBuilder emb = Context.CreateEmbed(title: "검색 실패", description: $"어휘가 없네요");
                await Context.MsgReplyEmbedAsync(emb.Build());
            }
        }

        [Command("디스코드봇"), Alias("코리안봇", "디코봇")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("[한국 디스코드 봇 리스트](https://koreanbots.dev/)에서 봇을 검색합니다")]
        public async Task DiscordBot([Remainder, Name("봇")] string name)
        {
            using WebClient wc = new WebClient();
            KoreanBotsResult response = JsonConvert.DeserializeObject<KoreanBotsResult>(wc.DownloadString($"https://api.koreanbots.dev/v1/bots/search?q={name}"));

            EmbedBuilder emb;
            if (response.data.Count == 0)
            {
                emb = Context.CreateEmbed(title: "검색 실패", description: $"알 수 없는 봇이에요");
                await Context.MsgReplyEmbedAsync(emb.Build());
                return;
            }

            KoreanBotsResult.Bot b = response.data.First();

            emb = Context.CreateEmbed(title: b.name, thumbnailUrl: $"https://beta.koreanbots.dev/api/image/discord/avatars/{b.id}.gif?size=512");

            emb.AddField("상태", b.status switch 
            {
                "online" => "<:online:708147696879272027>",
                "idle" => "<:idle:708147696807968842>",
                "dnd" => "<:dnd:708147696976003092>",
                "offline" => "<:offline:708147696523018255>",
                _ => ":question:"
            }, true);
            emb.AddField("카테고리", string.Join(' ', b.category.Select(c => $"`{c}`")), true);

            emb.AddField("설명", b.intro);

            emb.AddField("디스코드 인증됨", (b.verified == 1).ToEmoji(), true);
            emb.AddField("신뢰함", (b.trusted == 1).ToEmoji(), true);
            emb.AddField("부스트", (b.boosted == 1).ToEmoji(), true);

            emb.AddField("서버 수", $"{b.servers} 서버", true);
            emb.AddField("하트 수", $":heart: {b.votes}", true);
            emb.AddField("초대하기", $"[초대 링크]({b.url})", true);

            await Context.MsgReplyEmbedAsync(emb.Build());
        }

        [Command("스크래치 유저")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("스크래치에서 주어진 유저를 검색합니다")]
        public async Task ScratchUser([Name("유저네임")] string name)
        {
            using WebClient wc = new WebClient();

            EmbedBuilder emb;

            string DbResponse;
            try
            {
                DbResponse = wc.DownloadString($"https://scratchdb.lefty.one/v3/user/info/{name}");
            }
            catch (WebException)
            {
                emb = Context.CreateEmbed(title: "검색 실패", description: "404. 오 이런! 올리브토스트가 머리를 스크래칭하고 있군요");
                await Context.MsgReplyEmbedAsync(emb.Build());
                return;
            }

            ScratchDbUserResult dbResult = JsonConvert.DeserializeObject<ScratchDbUserResult>(DbResponse);
            ScratchApiUserResult apiResult = JsonConvert.DeserializeObject<ScratchApiUserResult>(wc.DownloadString($"https://api.scratch.mit.edu/users/{name}"));

            emb = Context.CreateEmbed(title: apiResult.username, url: $"https://scratch.mit.edu/users/{name}", thumbnailUrl: $"https://cdn2.scratch.mit.edu/get_image/user/{apiResult.id}_90x90.png");

            emb.AddField("스크래쳐", (dbResult.status == "Scratcher").ToEmoji(), true);
            emb.AddField("가입일", ((DateTimeOffset)dbResult.joined).ToShortKSTString(), true);
            emb.AddField("국가", dbResult.country, true);

            emb.AddField("팔로잉", $"{dbResult.statistics.following}명", true);
            emb.AddField("팔로워", $"{dbResult.statistics.followers}명", true);
            emb.AddField("팔로워 순위", $"{dbResult.statistics.ranks.country.followers}위", true);

            emb.AddField("전체 조회수", $"{dbResult.statistics.views}번", true);
            emb.AddField("전체 :heart: 수", $"{dbResult.statistics.loves}개", true);
            emb.AddField("전체 :star: 수", $"{dbResult.statistics.favorites}개", true);

            emb.AddField("내 소개", $"```\n{apiResult.profile.bio}```");
            emb.AddField("내가 하고 있는 일", $"```\n{apiResult.profile.status}```");

            await Context.MsgReplyEmbedAsync(emb.Build());
        }
    }
}
