using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
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
            switch(new Random().Next(0, 3))
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
                WordSession.Sessions.Add(Context.User.Id, new List<string>());

                if (word == null)
                {
                    await Context.MsgReplyEmbedAsync("끝말잇기 시작!");
                }
                else
                {
                    await WordRelay(Context, word);
                }
            }
            else
            {
                await Context.MsgReplyEmbedAsync("게임이 이미 진행중이에요");
            }
        }

        public static async Task<bool> WordRelay(SocketCommandContext context, string word)
        {
            if (WordSession.Sessions.ContainsKey(context.User.Id))
            {
                if (word == "끝")
                {
                    WordSession.Sessions.Remove(context.User.Id);
                    await context.MsgReplyEmbedAsync("게임 끝!");
                }
                else
                {
                    if (!WordsManager.Words.Contains(word))
                    {
                        await context.MsgReplyEmbedAsync($"제 사전에 {word}이란 없네요");
                        return true;
                    }
                    List<string> usedWords = WordSession.Sessions[context.User.Id];

                    if (usedWords.Count != 0 && !word.StartsWith(usedWords.Last().Last()))
                    {
                        await context.MsgReplyEmbedAsync($"'{usedWords.Last().Last()}'로 시작해야돼요");
                        return true;
                    }

                    if (usedWords.Contains(word))
                    {
                        await context.MsgReplyEmbedAsync($"{word}는 이미 사용한 단어에요");
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

                        nextWord = wordList[new Random().Next(0, wordList.Count)];
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

            await Context.MsgReplyEmbedAsync($"||{users[new Random().Next(0, users.Length)].Mention}||님이 당첨됐어요! :tada:");
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
    }
}
