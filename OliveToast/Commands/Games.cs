using AnimatedGif;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using HPark.Hangul;
using OliveToast.Managements.Data;
using OliveToast.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static OliveToast.Utilities.RequireCategoryEnable;
using static OliveToast.Utilities.RequirePermission;

namespace OliveToast.Commands
{
    [Name("게임")]
    [RequireCategoryEnable(CategoryType.Game)]
    public class Games : ModuleBase<SocketCommandContext>
    {
        [Command("주사위"), Alias("주사위 굴리기")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("주사위를 굴립니다\n`면 수`는 생략할 수 있습니다")]
        public async Task Dice([Name("면 수")] int count = 6)
        {
            await Context.ReplyEmbedAsync($"{new Random().Next(1, count + 1)}!");
        }

        public enum Rcp { 가위, 바위, 보 }
        [Command("가위바위보")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("가위바위보입니다\n`가위`, `바위`, `보` 중 하나를 입력할 수 있습니다")]
        public async Task RockScissorsPaper([Name("입력")] Rcp input)
        {
            switch(new Random().Next(3))
            {
                case 0:
                    await Context.ReplyEmbedAsync($"{input}! 무승부!");
                    break;
                case 1:
                    await Context.ReplyEmbedAsync($"{(Rcp)((int)(input + 1) % 3)}! 올리브토스트 승리!");
                    break;
                case 2:
                    await Context.ReplyEmbedAsync($"{(Rcp)((int)(input - 1 + 3) % 3)}! {Context.User.Username} 승리!");
                    break;
            }
        }

        [Command("끝말잇기")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("올리브토스트와 끝말잇기를 할 수 있습니다\n`단어`는 생략할 수 있습니다")]
        public async Task StartWordRelay([Name("단어")] string word = null)
        {
            if (!WordSession.Sessions.ContainsKey(Context.User.Id))
            {
                WordSession.Sessions.Add(Context.User.Id, new(Context, new List<string>(), DateTime.Now));

                ComponentBuilder component = new ComponentBuilder().WithButton("취소", $"{Context.User.Id}.{(int)InteractionHandler.InteractionType.CancelWordGame}", ButtonStyle.Danger);
                await Context.ReplyEmbedAsync("끝말잇기 시작!", component: component.Build());

                if (word != null)
                {
                    await WordRelay(Context, word);
                }
            }
            else
            {
                await Context.ReplyEmbedAsync("게임이 이미 진행중이에요");
            }
        }

        public static async Task<bool> WordRelay(SocketCommandContext context, string word = null)
        {
            if (WordSession.Sessions.ContainsKey(context.User.Id))
            {
                if (word == null)
                {
                    word = context.Message.Content;
                }

                if (WordSession.Sessions[context.User.Id].Context.Channel.Id == context.Channel.Id)
                {
                    WordSession.Sessions[context.User.Id].LastActiveTime = DateTime.Now;

                    if (!WordsManager.Words.Contains(word))
                    {
                        await context.ReplyEmbedAsync($"제 사전에 '{word.이("'")}란 없네요");
                        return true;
                    }
                    List<string> usedWords = WordSession.Sessions[context.User.Id].Words;

                    if (usedWords.Count != 0 && !word.StartsWith(usedWords.Last().Last()))
                    {
                        await context.ReplyEmbedAsync($"'{usedWords.Last().Last().ToString().으로("'")} 시작해야돼요");
                        return true;
                    }

                    if (usedWords.Contains(word))
                    {
                        await context.ReplyEmbedAsync($"{word.은는()} 이미 사용한 단어에요");
                        return true;
                    }
                    usedWords.Add(word);

                    List<string> wordList = WordsManager.Words.Where(w => w.StartsWith(word.Last())).ToList();
                    if (wordList.Count == 0)
                    {
                        await context.ReplyEmbedAsync($"{context.User.Username} 승리!\n게임이 종료됐어요");
                        WordSession.Sessions.Remove(context.User.Id);
                        return true;
                    }

                    string nextWord;
                    do
                    {
                        if (wordList.Count == 0)
                        {
                            await context.ReplyEmbedAsync($"{Program.Client.CurrentUser.Username} 승리!\n게임이 종료됐어요");
                            WordSession.Sessions.Remove(context.User.Id);
                            return true;
                        }

                        nextWord = wordList[new Random().Next(wordList.Count)];
                        wordList.Remove(nextWord);
                    }
                    while (usedWords.Contains(nextWord));

                    usedWords.Add(nextWord);

                    await context.ReplyAsync(nextWord);

                    wordList = WordsManager.Words.Where(w => w.StartsWith(nextWord.Last())).ToList();
                    if (wordList.Count == 0)
                    {
                        await context.ReplyEmbedAsync($"{Program.Client.CurrentUser.Username} 승리!\n게임이 종료됐어요");
                        WordSession.Sessions.Remove(context.User.Id);
                    }

                    return true;
                }
            }

            return false;
        }

        [Command("추첨")]
        [RequirePermission(PermissionType.UseBot), RequireContext(ContextType.Guild)]
        [Summary("주어진 유저들 중 한명을 랜덤으로 선택합니다\n`유저`를 생략하면 서버의 모든 유저 중 한명을 선택합니다")]
        public async Task Rot([Name("유저")] params SocketGuildUser[] users)
        {
            if (users.Length == 0)
            {
                users = Context.Guild.Users.ToArray();
            }
            else if (users.Length < 2)
            {
                await Context.ReplyEmbedAsync("2명 이상의 유저를 선택해주세요");
                return;
            }

            await Context.ReplyEmbedAsync($"||{users[new Random().Next(users.Length)].Mention}||님이 당첨됐어요! :tada:");
        }

        [Command("추첨")]
        [RequirePermission(PermissionType.UseBot), RequireContext(ContextType.Guild)]
        [Summary("주어진 역할을 가진 유저들 중 한명을 랜덤으로 선택합니다")]
        public async Task Rot([Remainder, Name("역할")] SocketRole role)
        {
            SocketGuildUser[] users = Context.Guild.Users.Where(u => u.Roles.Any(r => r.Id == role.Id)).ToArray();

            if (users.Length < 2)
            {
                await Context.ReplyEmbedAsync("2명 이상의 유저를 선택해주세요");
                return;
            }

            await Rot(users);
        }

        [Command("룰렛"), Alias("돌림판")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("를렛을 돌립니다")]
        public async Task Roulette([Name("항목")] params string[] items)
        {
            if (items.Length is < 2 or > 30)
            {
                await Context.ReplyEmbedAsync("두 개 이상, 30개 이하의 항목을 적어주세요");
                return;
            }

            RestUserMessage msg = await Context.ReplyAsync("렌더 중이에요...");

            const int fps = 10;

            int frameCount = fps * 5;
            float r = 2 * MathF.PI / items.Length;
            float radius = 240;

            Size size = new(512, 512);

            using Font font = Utility.GetFont(MathF.Min(r * 25, 40));
            using StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
            };

            System.Drawing.Color[] colors = new System.Drawing.Color[]
            {
                System.Drawing.Color.FromArgb(252, 92, 101),
                System.Drawing.Color.FromArgb(253, 150, 68),
                System.Drawing.Color.FromArgb(254, 211, 48),
                System.Drawing.Color.FromArgb(38, 222, 129),
                System.Drawing.Color.FromArgb(43, 203, 186),
                System.Drawing.Color.FromArgb(69, 170, 242),
                System.Drawing.Color.FromArgb(75, 123, 236),
                System.Drawing.Color.FromArgb(209, 216, 224),
                System.Drawing.Color.FromArgb(119, 140, 163),
            };

            using MemoryStream stream = new();
            AnimatedGifCreator gif = new(stream, 1000 / fps);

            using Bitmap baseImage = new(size.Width, size.Height);
            using Graphics baseG = Graphics.FromImage(baseImage);

            for (int i = 0; i < items.Length; i++)
            {
                List<PointF> points = new()
                {
                    new(size.Width / 2, size.Height / 2),
                };

                const int roundness = 15;
                for (int j = 0; j <= roundness; j++)
                {
                    points.Add(new(MathF.Cos(r * i + r / roundness * j) * radius + size.Width / 2, MathF.Sin(r * i + r / roundness * j) * radius + size.Height / 2));
                }

                using SolidBrush brush = new(colors[i % colors.Length]);
                baseG.FillPolygon(brush, points.ToArray());

                baseG.TranslateTransform(size.Width / 2, size.Height / 2);
                baseG.RotateTransform((r * i + r / 2) * (180 / MathF.PI) + 90);
                baseG.TranslateTransform(-size.Width / 2, -size.Height / 2);

                float width = MathF.Min(250, 110 * r);
                RectangleF rect = new(size.Width / 2 - width / 2, 40 + r * 6, width, 75 + r * 35);
                baseG.DrawString(items[i], font, Brushes.White, rect, format);

                baseG.ResetTransform();
            }

            List<PointF> pinPoints = new()
            {
                new(size.Width / 2 + -12.5f, 5),
                new(size.Width / 2 + 12.5f, 5),
                new(size.Width / 2 + 12.5f, 35),
                new(size.Width / 2 + 0, 50),
                new(size.Width / 2 + -12.5f, 35)
            };

            float prvAngle = 0;
            float speed = new Random().Next(20, 50) * 10 / fps;

            for (int i = 0; i < frameCount; i++)
            {
                using Bitmap bmp = new(size.Width, size.Height);
                using Graphics g = Graphics.FromImage(bmp);
                g.Clear(System.Drawing.Color.FromArgb(47, 49, 54));

                prvAngle += MathF.Sin(MathF.PI / frameCount * i) * speed;

                g.TranslateTransform(size.Width / 2, size.Height / 2);
                g.RotateTransform(prvAngle - 90);
                g.TranslateTransform(-size.Width / 2, -size.Height / 2);

                g.DrawImage(baseImage, Point.Empty);

                g.ResetTransform();

                g.FillPolygon(Brushes.LightGray, pinPoints.ToArray());

                int delay = -1;
                if (i == frameCount - 1)
                {
                    int index = (int)MathF.Floor((prvAngle % 360 * -1 + 360) / (360f / items.Length));

                    GraphicsPath path = new();
                    path.AddString(items[index], Utility.GetFontFamily(), (int)FontStyle.Bold, 50, new Rectangle(size.Width / 2 - 200, 300, 400, 200), new()
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                    });
                    g.DrawPath(new Pen(System.Drawing.Color.White, 7), path);
                    g.FillPath(new SolidBrush(colors[index % colors.Length]), path);

                    delay = 1000 * 5;
                }

                await gif.AddFrameAsync(bmp, delay, GifQuality.Bit8);
            }

            gif.Dispose();
            stream.Position = 0;

            await msg.DeleteAsync();

            EmbedBuilder emb = Context.CreateEmbed(imgUrl: "attachment://result.gif");
            await Context.Channel.SendFileAsync(stream, "result.gif", embed: emb.Build());
        }

        [Command("타자 연습"), Alias("타자", "타자연습")]
        [RequirePermission(PermissionType.UseBot), RequireBotPermission(ChannelPermission.AttachFiles)]
        [Summary("타자 연습을 할 수 있습니다")]
        public async Task StartTypingGame()
        {
            string sentence = SentenceManager.Sentences[new Random().Next(SentenceManager.Sentences.Count)];

            if (TypingSession.Sessions.ContainsKey(Context.User.Id))
            {
                await Context.ReplyEmbedAsync("게임이 이미 진행중이에요");
                return;
            }

            ComponentBuilder component = new ComponentBuilder().WithButton("취소", $"{Context.User.Id}.{(int)InteractionHandler.InteractionType.CancelTypingGame}", ButtonStyle.Danger);
            await Context.ReplyAsync($"> {string.Join("\u200B", sentence.ToCharArray())}", component: component.Build());

            TypingSession.Sessions.Add(Context.User.Id, new(Context, sentence, DateTime.Now));
        }

        [Command("영어 타자 연습"), Alias("영타", "entyping")]
        [RequirePermission(PermissionType.UseBot), RequireBotPermission(ChannelPermission.AttachFiles)]
        [Summary("타자 연습을 할 수 있습니다")]
        public async Task StartEnTypingGame()
        {
            string sentence = SentenceManager.EnSentences[new Random().Next(SentenceManager.EnSentences.Count)];

            if (TypingSession.Sessions.ContainsKey(Context.User.Id))
            {
                await Context.ReplyEmbedAsync("게임이 이미 진행중이에요");
                return;
            }

            ComponentBuilder component = new ComponentBuilder().WithButton("취소", $"{Context.User.Id}.{(int)InteractionHandler.InteractionType.CancelTypingGame}", ButtonStyle.Danger);
            await Context.ReplyAsync($"> {string.Join("\u200B", sentence.ToCharArray())}", component: component.Build());

            TypingSession.Sessions.Add(Context.User.Id, new(Context, sentence, DateTime.Now));
        }

        public static async Task<bool> TypingGame(SocketCommandContext context)
        {
            if (TypingSession.Sessions.ContainsKey(context.User.Id))
            {
                var session = TypingSession.Sessions[context.User.Id];

                if (session.Context.Channel.Id == context.Channel.Id)
                {
                    string content = context.Message.Content;
                    EmbedBuilder emb = context.CreateEmbed(title: "타자 연습");

                    if (content.Contains("\u200B"))
                    {
                        emb.Description = $"복붙이 감지되었어요\n게임을 정정당당하게 플레이해주세요!";
                        await context.ReplyEmbedAsync(emb.Build());

                        TypingSession.Sessions.Remove(context.User.Id);
                        return true;
                    }

                    int speed = (int)(HangulString.SplitToPhonemes(content).Length / (DateTime.Now - session.LastActiveTime).TotalSeconds * 60);

                    double a = content.Length < session.Sentence.Length ?
                        (double)content.Where((c, i) => i < session.Sentence.Length && c == session.Sentence[i]).Count() / session.Sentence.Length :
                        (double)session.Sentence.Where((c, i) => i < content.Length && c == content[i]).Count() / content.Length;
                    int accuracy = (int)(a * 100);

                    emb.Description = $"타수: {speed}\n정확도: {accuracy}%";
                    await context.ReplyEmbedAsync(emb.Build());

                    TypingSession.Sessions.Remove(context.User.Id);

                    return true;
                }
            }

            return false;
        }
    }
}
