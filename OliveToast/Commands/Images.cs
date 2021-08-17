using Discord;
using Discord.Commands;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
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
            using MemoryStream stream = Context.GetFileStream(ref url);
            if (stream == null)
            {
                await Context.MsgReplyEmbedAsync("이미지 url이나 파일을 올려주세요");
                return;
            }

            EmbedBuilder emb = Context.CreateEmbed(imgUrl: "attachment://result.gif");

            await Context.Channel.SendFileAsync(stream, $"result.gif", embed: emb.Build());
        }

        [Command("팔레트"), Alias("팔레트 추출", "색추출")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("이미지에서 가장 많이 쓰인 색을 추출합니다")]
        public async Task Palette([Name("Url")] string url = null)
        {
            using MemoryStream stream = Context.GetFileStream(ref url);
            if (stream == null)
            {
                await Context.MsgReplyEmbedAsync("이미지 url이나 파일을 올려주세요");
                return;
            }

            // get all colors
            Dictionary<System.Drawing.Color, int> colors = new();
            using (Bitmap bmp = new(System.Drawing.Image.FromStream(stream), 128, 128))
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        System.Drawing.Color px = bmp.GetPixel(x, y);
                        if (colors.ContainsKey(px))
                        {
                            colors[px]++;
                        }
                        else
                        {
                            colors.Add(px, 1);
                        }
                    }
                }
            }

            // find simillar colors
            List<(System.Drawing.Color color, int count)> colorList = colors.Select(c => (c.Key, c.Value)).ToList();
            colorList.Sort((t1, t2) => t2.count.CompareTo(t1.count));
            for (int i = 0; i < colorList.Count; i++)
            {
                for (int o = 0; o < i; o++)
                {
                    System.Drawing.Color c1 = colorList[i].color;
                    System.Drawing.Color c2 = colorList[o].color;

                    if (Math.Abs(c1.R - c2.R) < 10 && Math.Abs(c1.G - c2.G) < 10 && Math.Abs(c1.B - c2.B) < 10)
                    {
                        colorList[o] = (colorList[o].color, colorList[o].count + colorList[i].count);
                        colorList.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }
            colorList.Sort((t1, t2) => t2.count.CompareTo(t1.count));

            var slicedColorList = colorList;
            if (slicedColorList.Count > 10)
            {
                slicedColorList = slicedColorList.GetRange(0, 10);
            }

            Bitmap output;

            int cellCount = 0;
            if (slicedColorList.Count <= 5)
            {
                cellCount = slicedColorList.Count;

                output = new Bitmap(slicedColorList.Count * 100, 110);
            }
            else
            {
                cellCount = (int)Math.Ceiling(slicedColorList.Count / 2f);

                output = new Bitmap(cellCount * 100, 210);
            }

            Graphics g = Graphics.FromImage(output);

            // draw palette
            PrivateFontCollection collection = new();
            collection.AddFontFile(@"Configs\NotoSansKR.otf");
            using (Font font = new(new FontFamily("Noto Sans KR", collection), 13f))
            using (StringFormat format = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far })
            {
                for (int i = 0; i < slicedColorList.Count; i++)
                {
                    bool isTop = i < cellCount;
                    Rectangle rect = new((i - (isTop ? 0 : cellCount)) * 100, isTop ? 0 : 100, 100, 100);

                    g.FillRectangle(new SolidBrush(slicedColorList[i].color), rect);
                    g.DrawString(slicedColorList[i].color.ToHex(), font, slicedColorList[i].color.GetBrightness() < .5f ? Brushes.White : new SolidBrush(System.Drawing.Color.FromArgb(50, 50, 50)), rect, format);
                }
            }

            // draw percent bar
            int percentY = colorList.Count <= 5 ? 100 : 200;
            int lastX = 0;
            foreach (var (color, count) in colorList)
            {
                Rectangle rect = new(lastX, percentY, (int)((float)count / (128 * 128) * output.Width), 10);

                g.FillRectangle(new SolidBrush(color), rect);
                g.DrawLine(Pens.White, rect.X + rect.Width, rect.Y, rect.X + rect.Width, rect.Y + 10);

                lastX += rect.Width;
            }

            // save image to stream
            MemoryStream outputStram = new();
            output.Save(outputStram, System.Drawing.Imaging.ImageFormat.Png);
            outputStram.Position = 0;

            // send
            EmbedBuilder emb = Context.CreateEmbed(title: "팔레트", imgUrl: "attachment://result.png", thumbnailUrl: url);
            for (int i = 0; i < slicedColorList.Count; i++)
            {
                emb.AddField(slicedColorList[i].color.ToHex(), slicedColorList[i].color.ToFormattedString(), true);
            }

            await Context.Channel.SendFileAsync(outputStram, "result.png", embed: emb.Build());

            outputStram.Dispose();
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

            await ReplyAsync(result, allowedMentions: AllowedMentions.None);
        }

        [Command("외부이모지")]
        [RequirePermission(PermissionType.SpeakByBot)]
        [Summary("다른 서버의 이모지를 사용할 수 있습니다\n(올리브토스트가 있는 서버의 이모지만 사용 가능합니다)")]
        public async Task CustomEmoji([Name("입력")] params string[] input)
        {
            List<GuildEmote> emojis = new();
            foreach(var g in Program.Client.Guilds)
            {
                emojis.AddRange(g.Emotes);
            }

            string result = "";

            Random r = new();

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

            await ReplyAsync(result, allowedMentions: AllowedMentions.None);
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
