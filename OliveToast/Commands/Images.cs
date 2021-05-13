using Discord;
using Discord.Commands;
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
    [Name("이미지")]
    [RequireCategoryEnable(CategoryType.Image)]
    public class Images : ModuleBase<SocketCommandContext>
    {
        [Command("움짤")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("이미지를 gif로 바꿔줍니다")]
        public async Task ToGif([Name("Url")] string url = null)
        {
            if (url == null)
            {
                if (Context.Message.Attachments.Any()) {
                    url = Context.Message.Attachments.First().Url;
                }
                else
                {
                    await Context.MsgReplyEmbedAsync("이미지 url이나 파일을 올려주세요");
                    return;
                }
            }

            WebClient wc = new WebClient();
            byte[] bytes = wc.DownloadData(url);

            MemoryStream stream = new MemoryStream(bytes);

            await Context.Channel.SendFileAsync(stream, $"{Context.User.Username}_{Context.User.Discriminator}.gif");

            stream.Dispose();
        }

        [Command("이모지 변환")]
        [RequirePermission(PermissionType.SpeakByBot)]
        [Summary("글자를 이모지로 바꿔줍니다")]
        public async Task ToEmoji([Remainder, Name("입력")] string input)
        {
            string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            string lower = "abcdefghijklmnopqrstuvwxyz";
            string[] lowerEmojis = { "<:a_:632153577992749056>", "<:b_:632153577564930069>", "<:c_:632153577745285151>",
                "<:d_:632153578009657364>", "<:e_:632153578076635149>", "<:f_:632153577984491520>","<:g_:632153578471030785>",
                "<:h_:632153578043080726>", "<:i_:632153577917382687>","<:j_:632153577942679555>", "<:k_:632153578018177025>",
                "<:l_:632153578081091594>", "<:m_:632153578039017472>", "<:n_:632153578101932032>", "<:o_:632153578030497793>",
                "<:p_:632153578101932042>", "<:q_:632153578076635136>", "<:r_:632153578206789632>", "<:s_:632153578290675712>",
                "<:t_:632153577711730695>", "<:u_:632153578177560586>", "<:v_:632153578185818112>", "<:w_:632153577892216863>",
                "<:x_:632153578026565644>", "<:y_:632153578253058048>", "<:z_:632153578286612481>"};

            string special = "0123456789!?+-/*$#<>";
            string[] specialEmojis = { ":zero:", ":one:", ":two:", ":three:", ":four:",":five:", ":six:", ":seven:", ":eight:",
                ":nine:", ":exclamation:", ":question:", ":heavy_plus_sign:", ":heavy_minus_sign:", ":heavy_division_sign:",
                ":heavy_multiplication_x:", ":heavy_dollar_sign:", ":hash:", ":arrow_backward:", ":arrow_forward:" };

            string result = "";

            for (int num = 0; num < input.Length; num++)
            {
                if (upper.Contains(input[num]))
                {
                    result += $":regional_indicator_{input[num]}:";
                }
                else if (lower.Contains(input[num]))
                {
                    result += lowerEmojis[lower.IndexOf(input[num])];
                }
                else if (input[num] == ' ')
                {
                    result += "    ";
                }
                else if (special.Contains(input[num]))
                {
                    result += specialEmojis[special.IndexOf(input[num])];
                }
                else
                {
                    result += ":no_entry_sign:";
                }
            }

            result = result.ToLower();

            await ReplyAsync(result);
        }

        [Command("외부이모지")]
        [RequirePermission(PermissionType.SpeakByBot)]
        [Summary("다른 서버의 이모지를 사용할 수 있습니다\n(올리브토스트가 있는 서버의 이모지만 사용 가능합니다)")]
        public async Task CustomEmoji([Name("입력")] params string[] input)
        {
            List<GuildEmote> emojis = new List<GuildEmote>();
            foreach(var g in Program.Client.Guilds)
            {
                emojis.AddRange(g.Emotes);
            }

            string result = "";

            Random r = new Random();

            foreach(string s in input)
            {
                List<GuildEmote> inputEmojis = emojis.Where(e => e.Name == s).ToList();
                if (inputEmojis.Any())
                {
                    GuildEmote e = inputEmojis[r.Next(inputEmojis.Count)];
                    result += $"<{(e.Animated ? "a" : "")}:{e.Name}:{e.Id}>";
                }
                else
                {
                    result += $":{s}:";
                }
            }

            await ReplyAsync(result);
        }

        [Command("이모지아이디")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("이모지의 아이디를 얻을 수 있습니다")]
        public async Task EmojiID([Name("이름")] string name)
        {
            List<GuildEmote> emojis = Context.Guild.Emotes.Where(e => e.Name == name).ToList();
            if (emojis.Any())
            {
                await Context.MsgReplyEmbedAsync(emojis.First().Id);
            }
            else
            {
                await Context.MsgReplyEmbedAsync("존재하지 않는 이모지에요");
            }
        }
    }
}
