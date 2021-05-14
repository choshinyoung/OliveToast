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

            EmbedBuilder emb = Context.CreateEmbed(title: "위키백과");

            if (answers.Count >= 1 && answers.First().answer != "정의를 찾지 못했습니다.")
            {
                emb.Description = answers.First().answer;
            }
            else
            {
                emb.Description = $"\"그, `{input}` 혹시 보신 적이 있으십니까?\"\n\"아니, 잘 몰라요\"";
            }

            await Context.MsgReplyEmbedAsync(emb.Build());
        }
    }
}
