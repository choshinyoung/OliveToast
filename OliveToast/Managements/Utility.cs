using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HPark.Hangul;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast.Managements
{
    class Utility
    {
        public static string GetInvite()
        {
            return $"https://discord.com/oauth2/authorize?client_id={Program.Client.CurrentUser.Id}&scope=bot&permissions=2416241734";
        }

        public static int GetLevelXp(int level)
        {
            return level * 20 + 100;
        }

        public static DateTime TimestampToDateTime(long timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestamp).AddHours(9);
        }

        public class TimeOutWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest wr = base.GetWebRequest(address);
                wr.Timeout = 5000;
                return wr;
            }
        }
    }

    class KoreanBots
    {
        public static async Task UpdateServerCountAsync(int count)
        {
            if (Program.Client.CurrentUser.Id == 515688863474253824)
            {
                return;
            }

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://koreanbots.dev/api/v2/bots/495209098929766400/stats");

            request.Headers.TryAddWithoutValidation("content-type", "application/json");
            request.Headers.TryAddWithoutValidation("Authorization", ConfigManager.Get("KOREANBOTS_TOKEN"));

            request.Content = new StringContent("{\"servers\":" + count + "}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            await httpClient.SendAsync(request);
        }

        public static async Task<bool> IsVotedAsync(ulong userId)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://koreanbots.dev/api/v2/bots/495209098929766400/vote?userID={userId}");

            request.Headers.TryAddWithoutValidation("content-type", "application/json");
            request.Headers.TryAddWithoutValidation("Authorization", ConfigManager.Get("KOREANBOTS_TOKEN"));

            var response = await httpClient.SendAsync(request);

            var result = JsonConvert.DeserializeObject<Voted>(await response.Content.ReadAsStringAsync());

            return (DateTimeOffset.Now.ToKST() - Utility.TimestampToDateTime(result.data.lastVote)).Days > 7;
        }
    }

    static class ColorExtension
    {
        public static string ToHex(this System.Drawing.Color color)
        {
            return $"#{toHex(color.R)}{toHex(color.G)}{toHex(color.B)}";

            static string toHex(int dec)
            {
                string result = string.Format("{0:x2}", dec).ToUpper();
                    
                return result;
            }
        }

        public static string ToFormattedString(this System.Drawing.Color color)
        {
            return $"rgb({color.R}, {color.G}, {color.B})";
        }
    }

    static class StringExtension
    {
        public static string Slice(this string content, int length)
        {
            if (content.Length > length)
            {
                content = content.Substring(0, length - 3) + "...";
            }

            return content;
        }

        public static MemoryStream ToStream(this string content)
        {
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            return stream;
        }

        public static string Slice(this string content, int length, out bool isApplied)
        {
            if (content.Length > length)
            {
                content = content.Substring(0, length - 3) + "...";

                isApplied = true;
            }
            else
            {
                isApplied = false;
            }

            return content;
        }

        public static string 이(this string content, string suffix = "")
        {
            return content + suffix + (HaveJongsung(content.Last()) ? "이" : "");
        }

        public static string 을를(this string content, string suffix = "")
        {
            return content + suffix + (HaveJongsung(content.Last()) ? "을" : "를");
        }

        public static string 은는(this string content, string suffix = "")
        {
            return content + suffix + (HaveJongsung(content.Last()) ? "은" : "는");
        }

        public static string 이가(this string content, string suffix = "")
        {
            return content + suffix + (HaveJongsung(content.Last()) ? "이" : "가");
        }

        public static string 으로(this string content, string suffix = "")
        {
            HangulChar hc = new HangulChar(content.Last());
            bool canSplit = hc.TrySplitSyllable(out char[] syllables);

            return content + suffix + (canSplit && syllables[2] != '\u0000' && syllables[2] != 'ㄹ' ? "으로" : "로");
        }

        private static bool HaveJongsung(char c)
        {
            HangulChar hc = new HangulChar(c);
            bool canSplit = hc.TrySplitSyllable(out char[] syllables);

            if (canSplit)
            {
                return syllables[2] != '\u0000';
            }
            else
            {
                return false;
            }
        }
    }

    static class DiscordUserExtension
    {
        public static string GetAvatar(this IUser user)
        {
            return user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
        }

        public static string GetName(this IUser user, bool isPrivate)
        {
            return isPrivate ? user.Username : (user as SocketGuildUser).Nickname ?? user.Username;
        }
    }

    static class DateTimeOffsetExtension
    {
        public static DateTimeOffset ToKST(this DateTimeOffset time)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(time, "Korea Standard Time");
        }

        public static string ToKSTString(this DateTimeOffset time)
        {
            return time.ToKST().ToString("yyyy년 MM월 dd일 HH시 mm분 ss초");
        }

        public static string ToShortKSTString(this DateTimeOffset time)
        {
            return time.ToKST().ToString("yyyy년 MM월 dd일");
        }
    }

    static class BooleanExtension
    {
        public static string ToEmoji(this bool b)
        {
            return b ? ":white_check_mark:" : ":negative_squared_cross_mark:";
        }
    }

    static class IntExtension
    {
        public static string ToTimeString(this int a)
        {
            return a < 60 ? $"{a}초" : a < 3600 ? $"{a / 60}분" : $"{a / 3600}시간";
        }
    }
}
