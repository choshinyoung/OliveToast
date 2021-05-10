using Discord;
using Discord.Commands;
using HPark.Hangul;
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
    [Name("텍스트")]
    [RequireCategoryEnable(CategoryType.Text)]
    public class Text : ModuleBase<SocketCommandContext>
    {
        [Command("말하기")]
        [RequirePermission(PermissionType.SpeakByBot)]
        [Summary("올리브토스트로 말을 할 수 있습니다\n`말`은 500자 이하여야 합니다")]
        public async Task Say([Remainder, Name("말")] string input)
        {
            if (input.Length > 500)
            {
                await Context.MsgReplyEmbedAsync("도배 방지를 위해 500자 이하만 입력할 수 있어요");
                return;
            }

            await ReplyAsync(input);

            if (!Context.IsPrivate && Context.Guild.CurrentUser.GetPermissions((IGuildChannel)Context.Channel).ManageMessages)
            {
                await Context.Message.DeleteAsync();
            }
        }

        [Command("거꾸로"), Alias("로꾸거")]
        [RequirePermission(PermissionType.SpeakByBot)]
        [Summary("말을 거꾸로 합니다\n`말`은 500자 이하여야 합니다")]
        public async Task Reverse([Remainder, Name("말")] string input)
        {
            if (input.Length > 500)
            {
                await Context.MsgReplyEmbedAsync("도배 방지를 위해 500자 이하만 입력할 수 있어요");
                return;
            }

            await ReplyAsync(string.Join("", input.Reverse()));
        }

        [Command("안드로어"), Alias("안", "dksemfhdj", "dks")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("영어 키보드로 입력한 글을 한글로 바꿔줍니다")]
        public async Task AndroLang([Remainder, Name("말")] string input)
        {
            char[] en = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            char[] ko = "ㅁㅠㅊㅇㄷㄹㅎㅗㅑㅓㅏㅣㅡㅜㅐㅔㅂㄱㄴㅅㅕㅍㅈㅌㅛㅋㅁㅠㅊㅇㄸㄹㅎㅗㅑㅓㅏㅣㅡㅜㅒㅖㅃㄲㄴㅆㅕㅍㅉㅌㅛㅋ".ToCharArray();

            string[] combinableJong = 
            { 
                "ㄱㅅ", "ㄴㅈ", "ㄴㅎ", "ㄹㄱ", "ㄹㅁ", "ㄹㅂ", "ㄹㅅ", "ㄹㅌ", "ㄹㅍ", "ㄹㅎ", "ㅂㅅ"
            };
            string[] combinableJoong =
            {
                "ㅗㅏ", "ㅗㅐ", "ㅗㅣ", "ㅜㅓ", "ㅜㅔ", "ㅜㅣ", "ㅡㅣ"
            };
            char[] combinedJong = "ㄳㄵㄶㄺㄻㄼㄽㄾㄿㅀㅄ".ToCharArray();
            char[] combinedJoong = "ㅘㅙㅚㅝㅞㅟㅢ".ToCharArray();

            string kInput = "";
            foreach(char c in input)
            {
                kInput += en.Contains(c) ? ko[Array.IndexOf(en, c)] : c;
            }

            string result = "";
            for(int i = 0; i < kInput.Length; i++)
            {
                if (kInput.Length > i + 4 && new HangulChar(kInput[i]).IsOnset() && combinableJoong.Contains(string.Join("", kInput[i + 1], kInput[i + 2])) && combinableJong.Contains(string.Join("", kInput[i + 3], kInput[i + 4])) && (kInput.Length == i + 5 || !new HangulChar(kInput[i + 5]).IsNucleus()))
                {
                    bool isSuccess = HangulChar.TryJoinToSyllable(new char[] 
                    {
                        kInput[i],
                        combinedJoong[Array.IndexOf(combinableJoong, string.Join("", kInput[i + 1], kInput[i + 2]))], 
                        combinedJong[Array.IndexOf(combinableJong, string.Join("", kInput[i + 3], kInput[i + 4]))] 
                    }, out char res);
                     
                    if (isSuccess)
                    {
                        result += res;
                        i += 4;
                    }
                    else
                    {
                        result += kInput[i];
                    }
                }
                else if(kInput.Length > i + 3 && new HangulChar(kInput[i]).IsOnset() && new HangulChar(kInput[i + 1]).IsNucleus() && combinableJong.Contains(string.Join("", kInput[i + 2], kInput[i + 3])) && (kInput.Length == i + 4 || !new HangulChar(kInput[i + 4]).IsNucleus()))
                {
                    bool isSuccess = HangulChar.TryJoinToSyllable(new char[]
                    {
                        kInput[i],
                        kInput[i + 1],
                        combinedJong[Array.IndexOf(combinableJong, string.Join("", kInput[i + 2], kInput[i + 3]))]
                    }, out char res);

                    if (isSuccess)
                    {
                        result += res;
                        i += 3;
                    }
                    else
                    {
                        result += kInput[i];
                    }
                }
                else if (kInput.Length > i + 3 && new HangulChar(kInput[i]).IsOnset() && combinableJoong.Contains(string.Join("", kInput[i + 1], kInput[i + 2])) && new HangulChar(kInput[i + 3]).IsCoda() && (kInput.Length == i + 4 || !new HangulChar(kInput[i + 4]).IsNucleus()))
                {
                    bool isSuccess = HangulChar.TryJoinToSyllable(new char[]
                    {
                        kInput[i],
                        combinedJoong[Array.IndexOf(combinableJoong, string.Join("", kInput[i + 1], kInput[i + 2]))],
                        kInput[i + 3]
                    }, out char res);

                    if (isSuccess)
                    {
                        result += res;
                        i += 3;
                    }
                    else
                    {
                        result += kInput[i];
                    }
                }
                else if (kInput.Length > i + 2 && new HangulChar(kInput[i]).IsOnset() && combinableJoong.Contains(string.Join("", kInput[i + 1], kInput[i + 2])))
                {
                    bool isSuccess = HangulChar.TryJoinToSyllable(new char[]
                    {
                        kInput[i],
                        combinedJoong[Array.IndexOf(combinableJoong, string.Join("", kInput[i + 1], kInput[i + 2]))],
                        '\u0000'
                    }, out char res);

                    if (isSuccess)
                    {
                        result += res;
                        i += 2;
                    }
                    else
                    {
                        result += kInput[i];
                    }
                }
                else if (kInput.Length > i + 2 && new HangulChar(kInput[i]).IsOnset() && new HangulChar(kInput[i + 1]).IsNucleus() && new HangulChar(kInput[i + 2]).IsCoda() && (kInput.Length == i + 3 || !new HangulChar(kInput[i + 3]).IsNucleus()))
                {
                    bool isSuccess = HangulChar.TryJoinToSyllable(new char[]
                    {
                        kInput[i],
                        kInput[i + 1],
                        kInput[i + 2]
                    }, out char res);

                    if (isSuccess)
                    {
                        result += res;
                        i += 2;
                    }
                    else
                    {
                        result += kInput[i];
                    }
                }
                else if (kInput.Length > i + 1 && new HangulChar(kInput[i]).IsOnset() && new HangulChar(kInput[i + 1]).IsNucleus())
                {
                    bool isSuccess = HangulChar.TryJoinToSyllable(new char[]
                    {
                        kInput[i],
                        kInput[i + 1],
                        '\u0000'
                    }, out char res);

                    if (isSuccess)
                    {
                        result += res;
                        i += 1;
                    }
                    else
                    {
                        result += kInput[i];
                    }
                }
                else
                {
                    result += kInput[i];
                }
            }

            EmbedBuilder emb = Context.CreateEmbed(title: "안드로어", description: HangulString.JoinPhonemes(result));
            await Context.MsgReplyEmbedAsync(emb.Build());
        }
    }
}
