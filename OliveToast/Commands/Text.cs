using Discord;
using Discord.Commands;
using HPark.Hangul;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

            await ReplyAsync(input, allowedMentions: AllowedMentions.None);

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

            await ReplyAsync(new string(input.Reverse().ToArray()), allowedMentions: AllowedMentions.None);
        }

        [Command("안드로어"), Alias("안", "dksemfhdj", "dks")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("영어 키보드로 입력된 글을 한글로 바꿔줍니다")]
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
                if (kInput.Length > i + 4 && new HangulChar(kInput[i]).IsOnset() && combinableJoong.Contains(new string(kInput[i + 1], kInput[i + 2])) && combinableJong.Contains(new string(kInput[i + 3], kInput[i + 4])) && (kInput.Length == i + 5 || !new HangulChar(kInput[i + 5]).IsNucleus()))
                {
                    bool isSuccess = HangulChar.TryJoinToSyllable(new char[] 
                    {
                        kInput[i],
                        combinedJoong[Array.IndexOf(combinableJoong, new string(kInput[i + 1], kInput[i + 2]))], 
                        combinedJong[Array.IndexOf(combinableJong, new string(kInput[i + 3], kInput[i + 4]))] 
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
                else if(kInput.Length > i + 3 && new HangulChar(kInput[i]).IsOnset() && new HangulChar(kInput[i + 1]).IsNucleus() && combinableJong.Contains(new string(kInput[i + 2], kInput[i + 3])) && (kInput.Length == i + 4 || !new HangulChar(kInput[i + 4]).IsNucleus()))
                {
                    bool isSuccess = HangulChar.TryJoinToSyllable(new char[]
                    {
                        kInput[i],
                        kInput[i + 1],
                        combinedJong[Array.IndexOf(combinableJong, new string(kInput[i + 2], kInput[i + 3]))]
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
                else if (kInput.Length > i + 3 && new HangulChar(kInput[i]).IsOnset() && combinableJoong.Contains(new string(kInput[i + 1], kInput[i + 2])) && new HangulChar(kInput[i + 3]).IsCoda() && (kInput.Length == i + 4 || !new HangulChar(kInput[i + 4]).IsNucleus()))
                {
                    bool isSuccess = HangulChar.TryJoinToSyllable(new char[]
                    {
                        kInput[i],
                        combinedJoong[Array.IndexOf(combinableJoong, new string(kInput[i + 1], kInput[i + 2]))],
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
                else if (kInput.Length > i + 2 && new HangulChar(kInput[i]).IsOnset() && combinableJoong.Contains(new string(kInput[i + 1], kInput[i + 2])))
                {
                    bool isSuccess = HangulChar.TryJoinToSyllable(new char[]
                    {
                        kInput[i],
                        combinedJoong[Array.IndexOf(combinableJoong, new string(kInput[i + 1], kInput[i + 2]))],
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

        [Command("역안드로어"), Alias("역안", "durdksemfhdj", "durdks")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("한글 키보드로 입력된 글을 영어로 바꿔줍니다")]
        public async Task RvsAndroLang([Remainder, Name("말")] string input)
        {
            string[] ko = { "ㄱ", "ㄴ", "ㄷ", "ㄹ", "ㅁ", "ㅂ", "ㅅ", "ㅇ", "ㅈ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ", "ㅃ", "ㅉ", "ㄸ", "ㄲ", "ㅆ", "ㅏ", "ㅑ", "ㅓ", "ㅕ", "ㅗ", "ㅛ", "ㅜ", "ㅠ", "ㅡ", "ㅣ", "ㅐ", "ㅔ", "ㅒ", "ㅖ",
                                                        "ㄳ", "ㄵ", "ㄶ", "ㄺ", "ㄻ", "ㄼ", "ㄽ", "ㄾ", "ㄿ", "ㅀ", "ㅄ", "ㅘ", "ㅙ", "ㅚ", "ㅝ", "ㅞ", "ㅟ", "ㅢ" };

            string[] en = { "r", "s", "e", "f", "a", "q", "t", "d", "w", "c", "z", "x", "v", "g", "Q", "W", "E", "R", "T", "k", "i", "j", "u", "h", "y", "n", "b", "m", "l", "o", "p", "O", "P",
                                                        "rt", "sw", "sg", "fr", "fa", "fq", "ft", "fx", "fv", "fg", "qt", "hk", "ho", "hl", "nj", "np", "nl", "ml" };

            string splited = HangulString.SplitToPhonemes(input);

            string result = string.Join("", splited.Select(c => ko.Contains(c.ToString()) ? en[Array.IndexOf(ko, c.ToString())] : c.ToString()));

            EmbedBuilder emb = Context.CreateEmbed(title: "역안드로어", description: result);
            await Context.MsgReplyEmbedAsync(emb.Build());
        }

        [Command("인코딩"), Alias("Base64")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("주어진 글을 Base64로 변환합니다")]
        public async Task Encode([Remainder, Name("입력")] string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            string result = Convert.ToBase64String(bytes);

            await Context.MsgReplyEmbedAsync(result);
        }

        [Command("디코딩")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("Base64로 인코딩된 글을 디코딩합니다")]
        public async Task Decode([Remainder, Name("입력")] string input)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(input);
                string result = Encoding.UTF8.GetString(bytes);

                await Context.MsgReplyEmbedAsync(result);
            }
            catch
            {
                EmbedBuilder emb = Context.CreateEmbed(title: "오류 발생!", description: "해당 문자열을 디코딩 할 수 없어요");
                await Context.MsgReplyEmbedAsync(emb.Build());
            }
        }

        [Command("진법 변환"), Alias("진수 변환", "진법", "진수")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("`from`진수 `수`를 `to`진수로 변환합니다\n0-9, a-z를 사용해 36진수까지 표현할 수 있습니다")]
        public async Task BaseConvert(int from, int to, [Remainder, Name("수")] string num)
        {
            if (from > 36 || to > 36 || from < 2 || to < 2)
            {
                await Context.MsgReplyEmbedAsync("36 이하, 2 이상의 진법을 입력해주세요");
                return;
            }

            char[] numLetters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            num = new string(num.Where(n => n != ' ').ToArray()).ToUpper();
            if (num.Any(c => !numLetters.Contains(c)))
            {
                await Context.MsgReplyEmbedAsync("0-9, a-z와 공백으로만 이루어진 문자열을 입력해주세요");
                return;
            }
            if (num.Any(c => Array.IndexOf(numLetters, c) >= from))
            {
                await Context.MsgReplyEmbedAsync($"{from}진법에서 사용할 수 없는 문자가 있어요");
                return;
            }

            BigInteger dec = 0;

            BigInteger a = 1;
            foreach (char c in num.Reverse())
            {
                dec += Array.IndexOf(numLetters, c) * a;
                a *= from;
            }

            string result = "";

            while (dec > 0)
            {
                int b = (int)(dec % to);
                dec /= to;
                result += numLetters[b];
            }
            result = new string(result.Reverse().ToArray());

            result = result.Slice(2048);

            EmbedBuilder emb = Context.CreateEmbed(title: $"{from}진수 => {to}진수", description: result);
            await Context.MsgReplyEmbedAsync(emb.Build());
        }
    }
}
