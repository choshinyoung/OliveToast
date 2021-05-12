using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OliveToast.Managements.RequireCategoryEnable;
using static OliveToast.Managements.RequirePermission;

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
            await Context.MsgReplyEmbedAsync($"{new Random().Next(1, count + 1)}!");
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
                    await Context.MsgReplyEmbedAsync($"{input}! 무승부!");
                    break;
                case 1:
                    await Context.MsgReplyEmbedAsync($"{(Rcp)((int)(input + 1) % 3)}! 올리브토스트 승리!");
                    break;
                case 2:
                    await Context.MsgReplyEmbedAsync($"{(Rcp)((int)(input - 1 + 3) % 3)}! {Context.User.Username} 승리!");
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
                WordSession.Sessions.Add(Context.User.Id, (Context.Channel.Id, new List<string>()));
                await Context.MsgReplyEmbedAsync("끝말잇기 시작!\n`끝`을 입력해 게임을 끝낼 수 있어요");

                if (word != null)
                {
                    await WordRelay(Context, word);
                }
            }
            else
            {
                await Context.MsgReplyEmbedAsync("게임이 이미 진행중이에요");
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

                if (word == "끝")
                {
                    WordSession.Sessions.Remove(context.User.Id);
                    await context.MsgReplyEmbedAsync("게임 끝!");
                }
                else if (WordSession.Sessions[context.User.Id].channel == context.Channel.Id)
                {
                    if (!WordsManager.Words.Contains(word))
                    {
                        await context.MsgReplyEmbedAsync($"제 사전에 '{word.이("'")}란 없네요");
                        return true;
                    }
                    List<string> usedWords = WordSession.Sessions[context.User.Id].words;

                    if (usedWords.Count != 0 && !word.StartsWith(usedWords.Last().Last()))
                    {
                        await context.MsgReplyEmbedAsync($"'{usedWords.Last().Last().ToString().으로("'")} 시작해야돼요");
                        return true;
                    }

                    if (usedWords.Contains(word))
                    {
                        await context.MsgReplyEmbedAsync($"{word.은는()} 이미 사용한 단어에요");
                        return true;
                    }
                    usedWords.Add(word);

                    List<string> wordList = WordsManager.Words.Where(w => w.StartsWith(word.Last())).ToList();
                    if (wordList.Count == 0)
                    {
                        await context.MsgReplyEmbedAsync($"{context.User.Username} 승리!\n게임이 종료됐어요");
                        WordSession.Sessions.Remove(context.User.Id);
                        return true;
                    }

                    string nextWord;
                    do
                    {
                        if (wordList.Count == 0)
                        {
                            await context.MsgReplyEmbedAsync($"{Program.Client.CurrentUser.Username} 승리!\n게임이 종료됐어요");
                            WordSession.Sessions.Remove(context.User.Id);
                            return true;
                        }

                        nextWord = wordList[new Random().Next(wordList.Count)];
                        wordList.Remove(nextWord);
                    }
                    while (usedWords.Contains(nextWord));

                    usedWords.Add(nextWord);

                    await context.MsgReplyAsync(nextWord);

                    wordList = WordsManager.Words.Where(w => w.StartsWith(nextWord.Last())).ToList();
                    if (wordList.Count == 0)
                    {
                        await context.MsgReplyEmbedAsync($"{Program.Client.CurrentUser.Username} 승리!\n게임이 종료됐어요");
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
                await Context.MsgReplyEmbedAsync("2명 이상의 유저를 선택해주세요");
                return;
            }

            await Context.MsgReplyEmbedAsync($"||{users[new Random().Next(users.Length)].Mention}||님이 당첨됐어요! :tada:");
        }

        [Command("추첨")]
        [RequirePermission(PermissionType.UseBot), RequireContext(ContextType.Guild)]
        [Summary("주어진 역할을 가진 유저들 중 한명을 랜덤으로 선택합니다")]
        public async Task Rot([Remainder, Name("역할")] SocketRole role)
        {
            SocketGuildUser[] users = Context.Guild.Users.Where(u => u.Roles.Any(r => r.Id == role.Id)).ToArray();

            if (users.Length < 2)
            {
                await Context.MsgReplyEmbedAsync("2명 이상의 유저를 선택해주세요");
                return;
            }

            await Rot(users);
        }

        [Command("타자 연습"), Alias("타자", "타자연습")]
        [RequirePermission(PermissionType.UseBot), RequireBotPermission(ChannelPermission.AttachFiles)]
        [Summary("타자 연습을 할 수 있습니다")]
        public async Task StartTypingGame()
        {
            string sentence = SentenceManager.Sentences[new Random().Next(SentenceManager.Sentences.Count)];

            string text = "";
            int length = 0;
            foreach(string s in sentence.Split(' '))
            {
                if (length + s.Length > 15)
                {
                    text += $"\n{s} ";
                    length = 0;
                }
                else
                {
                    text += $"{s} ";
                }

                length += s.Length;
            }

            if (TypingSession.Sessions.ContainsKey(Context.User.Id))
            {
                await Context.MsgReplyEmbedAsync("게임이 이미 진행중이에요");
                return;
            }

            TypingSession.Sessions.Add(Context.User.Id, (Context.Channel.Id, sentence, DateTime.Now));

            Bitmap bmp = new Bitmap(512, 128);
            Graphics g = Graphics.FromImage(bmp); 
            
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.CompositingQuality = CompositingQuality.HighQuality;

            g.Clear(System.Drawing.Color.Transparent);

            StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            Pen pen = new Pen(System.Drawing.Color.FromArgb(54, 57, 64), 2.5f);
            Brush brush = Brushes.White;

            GraphicsPath path = new GraphicsPath();
            path.AddString(text, FontFamily.GenericSansSerif, (int)FontStyle.Bold, 25, new Rectangle(0, 0, bmp.Width, bmp.Height), format);

            g.DrawPath(pen, path);
            g.FillPath(brush, path);

            path.Dispose();
            g.Dispose();

            Stream stream = new MemoryStream();
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;

            await Context.Channel.SendFileAsync(stream, "typing.png");
            stream.Dispose();
        }

        public static async Task<bool> TypingGame(SocketCommandContext context)
        {
            if (TypingSession.Sessions.ContainsKey(context.User.Id))
            {
                var (channel, sentence, StartTime) = TypingSession.Sessions[context.User.Id];

                if (channel == context.Channel.Id) 
                {
                    string content = context.Message.Content;

                    int speed = (int)(content.Length / (DateTime.Now - StartTime).TotalSeconds * 60);

                    int accuracy = (int)((double)content.Where((c, i) => i < sentence.Length && c == sentence[i]).Count() / sentence.Length * 100);

                    EmbedBuilder emb = context.CreateEmbed(title: "타자 연습", description: $"타수: {speed}\n정확도: {accuracy}%");

                    await context.MsgReplyEmbedAsync(emb.Build());

                    TypingSession.Sessions.Remove(context.User.Id);
                }
            }

            return false;
        }
    }
}
