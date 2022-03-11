using AnimatedGif;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using HPark.Hangul;
using OliveToast.Managements;
using OliveToast.Managements.Data;
using OliveToast.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static OliveToast.Utilities.RequireCategoryEnable;
using static OliveToast.Utilities.RequirePermission;

using Color = System.Drawing.Color;

namespace OliveToast.Commands
{
    [Name("게임")]
    [RequireCategoryEnable(CategoryType.Game)]
    public class Games : ModuleBase<SocketCommandContext>
    {
        [Command("주사위"), Alias("주사위 굴리기")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("주사위를 굴릴 수 있는 커맨드예요\n`면 수`는 생략할 수 있어요")]
        public async Task Dice([Name("면 수")] int count = 6)
        {
            await Context.ReplyEmbedAsync($"{new Random().Next(1, count + 1)}!");
        }

        public enum Rcp { 가위, 바위, 보 }
        [Command("가위바위보")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("올리브토스트와 가위바위보를 해보세요\n`가위`, `바위`, `보` 중 하나를 입력할 수 있어요")]
        public async Task RockScissorsPaper([Name("입력")] Rcp input)
        {
            switch (new Random().Next(3))
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
        [Summary("다른 유저들과 끝말잇기를 해보세요")]
        public async Task ReadyWordGame()
        {
            if (WordSession.Sessions.Any(s => s.Value.Players.Contains(Context.User.Id)))
            {
                await Context.ReplyEmbedAsync("이미 다른 게임에 참가중이에요");

                return;
            }

            if (WordSession.Sessions.Any(s => s.Value.Context.Channel.Id == Context.Channel.Id))
            {
                await Context.ReplyEmbedAsync("이 채널에서 진행중인 게임이 있어요\n다른 채널을 사용해주세요");

                return;
            }

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("참가하기", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.JoinWordGame), ButtonStyle.Primary)
                .WithButton("게임 시작", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.StartWordGame), ButtonStyle.Success)
                .WithButton("취소", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CancelWordGame), ButtonStyle.Danger);
            EmbedBuilder emb = Context.CreateEmbed($"현재 참가자: {Context.User.Mention}", "참가자를 모집중이에요");

            var joinMessage = await Context.ReplyEmbedAsync(emb.Build(), component: component.Build());
            WordSession session = new(Context, true, joinMessage, DateTime.Now);
            session.Players.Add(Context.User.Id);

            WordSession.Sessions.Add(joinMessage.Id, session);
        }

        [Command("영어끝말잇기"), Alias("영어 끝말잇기")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("다른 유저들과 영어로 끝말잇기를 해보세요")]
        public async Task ReadyEnWordGame()
        {
            if (WordSession.Sessions.Any(s => s.Value.Players.Contains(Context.User.Id)))
            {
                await Context.ReplyEmbedAsync("이미 다른 게임에 참가중이에요");

                return;
            }

            if (WordSession.Sessions.Any(s => s.Value.Context.Channel.Id == Context.Channel.Id))
            {
                await Context.ReplyEmbedAsync("이 채널에서 진행중인 게임이 있어요\n다른 채널을 사용해주세요");

                return;
            }

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("참가하기", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.JoinWordGame), ButtonStyle.Primary)
                .WithButton("게임 시작", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.StartWordGame), ButtonStyle.Success)
                .WithButton("취소", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CancelWordGame), ButtonStyle.Danger);
            EmbedBuilder emb = Context.CreateEmbed($"현재 참가자: {Context.User.Mention}", "참가자를 모집중이에요");

            var joinMessage = await Context.ReplyEmbedAsync(emb.Build(), component: component.Build());
            WordSession session = new(Context, false, joinMessage, DateTime.Now);
            session.Players.Add(Context.User.Id);

            WordSession.Sessions.Add(joinMessage.Id, session);
        }

        public static async Task StartWordGame(WordSession session)
        {
            session.IsStarted = true;
            session.CurrentTurn = session.Players[0];
            session.LastActiveTime = DateTime.Now;
            if (session.IsKorean)
            {
                session.Words.Add(WordsManager.Words[new Random().Next(0, WordsManager.Words.Count)]);
            }
            else
            {
                session.Words.Add(WordsManager.WordsEn[new Random().Next(0, WordsManager.WordsEn.Count)]);
            }

            if (session.Players.Count == 1)
            {
                session.Players.Add(Program.Client.CurrentUser.Id);
            }

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("취소", InteractionHandler.GenerateCustomId(session.Context.User.Id, InteractionHandler.InteractionType.CancelWordGame), ButtonStyle.Danger);

            EmbedBuilder emb = session.JoinMessage.Embeds.First().ToEmbedBuilder()
                .WithTitle(null)
                .WithDescription($"게임이 시작됐어요\n현재 참가자: {string.Join(" ", session.Players.Select(p => session.Context.Guild.GetUser(p).Mention))}");


            await session.JoinMessage.ModifyAsync(m =>
            {
                m.Components = component.Build();
                m.Embed = emb.Build();
            });

            await NextTurn(session);
        }

        public static async Task GameOverUserInWordGame(WordSession session)
        {
            ulong gameOveredPlayer = session.CurrentTurn;

            if (session.Players.Last() == gameOveredPlayer)
            {
                session.CurrentTurn = session.Players[0];
            }
            else
            {
                session.CurrentTurn = session.Players[session.Players.IndexOf(gameOveredPlayer) + 1];
            }

            session.Players.Remove(gameOveredPlayer);

            EmbedBuilder emb = session.Context.CreateEmbed($"{session.Context.Guild.GetUser(gameOveredPlayer).Mention} 탈락!", "시간 초과");
            await session.Context.ReplyEmbedAsync(emb.Build());

            if (session.Players.Count <= 1)
            {
                await session.Context.ReplyEmbedAsync($"{session.Context.Guild.GetUser(session.Players[0]).Username} 승리!\n게임이 종료됐어요");
                WordSession.Sessions.Remove(session.JoinMessage.Id);

                return;
            }

            if (session.IsKorean)
            {
                session.Words.Add(WordsManager.Words[new Random().Next(0, WordsManager.Words.Count)]);
            }
            else
            {
                session.Words.Add(WordsManager.WordsEn[new Random().Next(0, WordsManager.WordsEn.Count)]);
            }

            session.LastActiveTime = DateTime.Now;

            await NextTurn(session);
        }

        public static async Task<bool> WordGame(SocketCommandContext context)
        {
            if (WordSession.Sessions.Any(w => w.Value.CurrentTurn == context.User.Id))
            {
                string word = context.Message.Content.ToLower();

                WordSession session = WordSession.Sessions.Where(w => w.Value.CurrentTurn == context.User.Id).First().Value;

                if (!session.IsStarted)
                {
                    return false;
                }

                if (session.Context.Channel.Id == context.Channel.Id)
                {
                    if (session.IsKorean)
                    {
                        if (!WordsManager.Words.Contains(word))
                        {
                            await context.ReplyEmbedAsync($"제 사전에 '{word.이("'")}란 없네요");

                            return true;
                        }

                        if (!getEndableLetters(session.Words.Last().Last()).Contains(word.First()))
                        {
                            await context.ReplyEmbedAsync($"'{session.Words.Last().Last().ToString().으로("'")} 시작해야돼요");

                            return true;
                        }

                        if (!WordsManager.Words.Any(w => w.StartsWith(word.Last())))
                        {
                            await context.ReplyEmbedAsync($"{word.은는()} 한방단어예요\n다른 단어를 사용해주세요");

                            return true;
                        }
                    }
                    else
                    {
                        if (word.Length < session.letterLength + 1)
                        {
                            await context.ReplyEmbedAsync($"{session.letterLength + 1}글자 이상의 단어를 사용해야돼요");

                            return true;
                        }

                        if (!WordsManager.WordsEn.Contains(word))
                        {
                            await context.ReplyEmbedAsync($"제 사전에 '{word.이("'")}란 없네요");

                            return true;
                        }

                        if (session.Words.Last()[^session.letterLength..] != word[..session.letterLength])
                        {
                            await context.ReplyEmbedAsync($"'{session.Words.Last()[^session.letterLength..]}'으로 시작해야돼요");

                            return true;
                        }

                        if (!WordsManager.WordsEn.Any(w => w.StartsWith(word[^session.letterLength..])))
                        {
                            await context.ReplyEmbedAsync($"{word.은는()} 한방단어예요\n다른 단어를 사용해주세요");

                            return true;
                        }
                    }

                    if (session.Words.Contains(word))
                    {
                        await context.ReplyEmbedAsync($"{word.은는()} 이미 사용한 단어예요");

                        return true;
                    }

                    session.LastActiveTime = DateTime.Now;

                    session.Words.Add(word);

                    if (session.Players.Last() == session.CurrentTurn)
                    {
                        session.CurrentTurn = session.Players[0];
                    }
                    else
                    {
                        session.CurrentTurn = session.Players[session.Players.IndexOf(session.CurrentTurn) + 1];
                    }

                    if (session.CurrentTurn == Program.Client.CurrentUser.Id)
                    {
                        List<string> wordList = session.IsKorean ? WordsManager.Words.Where(w => w.StartsWith(word.Last())).ToList() : WordsManager.WordsEn.Where(w => w[..session.letterLength] == word[^session.letterLength..]).ToList();
                        if (wordList.Count == 0)
                        {
                            await context.ReplyEmbedAsync($"{context.User.Username} 승리!\n게임이 종료됐어요");
                            WordSession.Sessions.Remove(session.JoinMessage.Id);

                            return true;
                        }

                        string nextWord;
                        do
                        {
                            if (wordList.Count == 0)
                            {
                                await context.ReplyEmbedAsync($"{context.User.Username} 승리!\n게임이 종료됐어요");
                                WordSession.Sessions.Remove(session.JoinMessage.Id);

                                return true;
                            }

                            nextWord = wordList[new Random().Next(wordList.Count)];
                            wordList.Remove(nextWord);
                        }
                        while (session.Words.Contains(nextWord) || (session.IsKorean ? !WordsManager.Words.Any(w => w.StartsWith(nextWord.Last())) : !WordsManager.WordsEn.Any(w => w[..session.letterLength] == nextWord[^session.letterLength..])));

                        session.Words.Add(nextWord);

                        await context.ReplyAsync(nextWord);

                        session.CurrentTurn = session.Players[0];
                    }

                    await NextTurn(session);

                    return true;
                }
            }

            return false;

            static char[] getEndableLetters(char endLetter)
            {
                List<char> endableLetters = new()
                {
                    endLetter
                };
                char[] syllables = new HangulChar(endLetter).SplitSyllable();
                if (syllables[0] == 'ㄹ')
                {
                    syllables[0] = 'ㄴ';
                    endableLetters.Add(HangulChar.JoinToSyllable(syllables));
                }
                if (syllables[0] == 'ㄴ' && "ㅣㅑㅕㅛㅠㅒㅖ".Contains(syllables[1]))
                {
                    syllables[0] = 'ㅇ';
                    endableLetters.Add(HangulChar.JoinToSyllable(syllables));
                }

                return endableLetters.ToArray();
            }
        }

        public static async Task NextTurn(WordSession session)
        {
            if (session.LastBotMessage != 0)
            {
                try
                {
                    await session.Context.Channel.DeleteMessageAsync(session.LastBotMessage);
                }
                catch { }
            }

            if (session.IsKorean)
            {
                session.LastBotMessage = (await session.Context.Channel.SendMessageAsync($"{session.Context.Guild.GetUser(session.CurrentTurn).Mention}님의 차례예요\n다음 단어를 이어주세요: {session.Words.Last()[0..^1]}__{session.Words.Last().Last()}__")).Id;
            }
            else
            {
                session.LastBotMessage = (await session.Context.Channel.SendMessageAsync($"{session.Context.Guild.GetUser(session.CurrentTurn).Mention}님의 차례예요\n다음 단어를 이어주세요: {session.Words.Last()[0..^session.letterLength]}__{session.Words.Last()[^session.letterLength..]}__")).Id;
            }
        }

        [Command("추첨")]
        [RequirePermission(PermissionType.UseBot), RequireContext(ContextType.Guild)]
        [Summary("주어진 유저들 중 한명을 랜덤으로 추첨하는 커맨드예요\n`유저`를 입력하지 않으면 서버의 모든 유저 중 한 명을 선택해요")]
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
        [Summary("주어진 역할을 가진 유저들 중 한명을 랜덤으로 추첨하는 커맨드예요")]
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
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("를렛을 돌릴 수 있는 커맨드예요")]
        public async Task Roulette([Name("항목")] params string[] items)
        {
            if (items.Length is < 2 or > 30)
            {
                await Context.ReplyEmbedAsync("두 개 이상, 30개 이하의 항목을 적어주세요");
                return;
            }

            RestUserMessage msg = await Context.ReplyEmbedAsync("렌더 중이에요...");

            const int fps = 10;

            int frameCount = fps * 5;
            float r = 2 * MathF.PI / items.Length;
            float radius = 240;

            Size size = new(512, 512);

            using Font font = Utility.GetFont(MathF.Min(r * 20, 30));
            using StringFormat format = new()
            {
                Alignment = StringAlignment.Center,
                Trimming = StringTrimming.Character,
            };

            Color[] colors = new[]
            {
                Color.FromArgb(252, 92, 101),
                Color.FromArgb(253, 150, 68),
                Color.FromArgb(254, 211, 48),
                Color.FromArgb(38, 222, 129),
                Color.FromArgb(43, 203, 186),
                Color.FromArgb(69, 170, 242),
                Color.FromArgb(75, 123, 236),
                Color.FromArgb(209, 216, 224),
                Color.FromArgb(119, 140, 163),
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
                g.Clear(Color.FromArgb(47, 49, 54));

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

                    SizeF textSize = g.MeasureString(items[index], Utility.GetFont(50));
                    textSize = new(MathF.Min(textSize.Width, 400), 200);

                    path.AddString(items[index], Utility.GetFontFamily(), (int)FontStyle.Bold, 50, new RectangleF(size.Width / 2 - textSize.Width / 2, 300, textSize.Width, textSize.Height), new()
                    {
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.Character,
                    });
                    g.DrawPath(new Pen(Color.White, 7), path);
                    g.FillPath(new SolidBrush(colors[index % colors.Length]), path);

                    delay = 1000 * 5;
                }

                await gif.AddFrameAsync(bmp, delay, GifQuality.Bit8);
            }

            gif.Dispose();
            stream.Position = 0;

            await msg.ModifyAsync(x =>
            {
                x.Embed = Context.CreateEmbed(imgUrl: "attachment://result.gif").Build();
                x.Attachments = new List<FileAttachment>()
                {
                    new(stream, "result.gif"),
                };
            });
        }

        [Command("타자 연습"), Alias("타자", "타자연습")]
        [RequirePermission(PermissionType.UseBot), RequireBotPermission(ChannelPermission.AttachFiles)]
        [Summary("타자 연습을 할 수 있는 커맨드예요")]
        public async Task StartTypingGame()
        {
            Random rand = new();

            string sentence;
            if (rand.Next(0, 10) == 0)
            {
                sentence = string.Join(' ', Enumerable.Range(0, 5).Select(i => WordsManager.Words[rand.Next(0, WordsManager.Words.Count)]));
            }
            else
            {
                sentence = SentenceManager.Sentences[rand.Next(SentenceManager.Sentences.Count)];
            }

            if (TypingSession.Sessions.ContainsKey(Context.User.Id))
            {
                await Context.ReplyEmbedAsync("게임이 이미 진행중이에요");
                return;
            }

            ComponentBuilder component = new ComponentBuilder().WithButton("취소", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CancelTypingGame), ButtonStyle.Danger);
            DateTimeOffset startTime = (await Context.ReplyAsync($"> {string.Join("\u200B", sentence.ToCharArray())}", component: component.Build())).CreatedAt;

            TypingSession.Sessions.Add(Context.User.Id, new(Context, sentence, startTime));
        }

        [Command("영어 타자 연습"), Alias("영타", "entyping", "영어 타자연습")]
        [RequirePermission(PermissionType.UseBot), RequireBotPermission(ChannelPermission.AttachFiles)]
        [Summary("영어로 타자 연습을 할 수 있는 커맨드예요")]
        public async Task StartEnTypingGame()
        {
            string sentence = SentenceManager.EnSentences[new Random().Next(SentenceManager.EnSentences.Count)];

            if (TypingSession.Sessions.ContainsKey(Context.User.Id))
            {
                await Context.ReplyEmbedAsync("게임이 이미 진행중이에요");
                return;
            }

            ComponentBuilder component = new ComponentBuilder().WithButton("취소", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CancelTypingGame), ButtonStyle.Danger);
            DateTimeOffset startTime = (await Context.ReplyAsync($"> {string.Join("\u200B", sentence.ToCharArray())}", component: component.Build())).CreatedAt;

            TypingSession.Sessions.Add(Context.User.Id, new(Context, sentence, startTime));
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

                    int speed = (int)(HangulString.SplitToPhonemes(content).Length / (context.Message.CreatedAt - session.LastActiveTime).TotalSeconds * 60);

                    float accuracy = GetAccuracy(session.Sentence, content);

                    emb.Description = $"타수: {speed}\n정확도: {accuracy:.00}%";
                    await context.ReplyEmbedAsync(emb.Build());

                    TypingSession.Sessions.Remove(context.User.Id);

                    return true;
                }
            }

            return false;
        }

        public static float GetAccuracy(string s1, string s2)
        {
            int[,] map = new int[s1.Length + 1, s2.Length + 1];
            int lcs = 0;

            for (int i = 0; i <= s1.Length; i++)
            {
                map[i, 0] = 0;
            }
            for (int i = 0; i <= s2.Length; i++)
            {
                map[0, i] = 0;
            }

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    if (s1[i - 1] == s2[j - 1])
                    {
                        map[i, j] = map[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        map[i, j] = Math.Max(map[i - 1, j], map[i, j - 1]);
                    }

                    lcs = Math.Max(lcs, map[i, j]);
                }
            }

            return (float)lcs / Math.Max(s1.Length, s2.Length) * 100;
        }
    }
}
