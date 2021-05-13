using Discord.Commands;
using Discord.WebSocket;
using HPark.Hangul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public class TimeOutWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest wr = base.GetWebRequest(address);
                wr.Timeout = 10000;
                return wr;
            }
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

    static class HangulExtension
    {
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
        public static string GetAvatar(this SocketUser user)
        {
            return user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
        }

        public static string GetName(this SocketUser user, bool isPrivate)
        {
            return isPrivate ? user.Username : (user as SocketGuildUser).Nickname ?? user.Username;
        }
    }

    static class DateTimeOffsetExtension
    {
        public static DateTimeOffset ToKST(this DateTimeOffset time)
        {
            return time.ToUniversalTime().AddHours(9);
        }

        public static string ToKSTString(this DateTimeOffset time)
        {
            return time.ToKST().ToString("yyyy년 MM월 dd일 HH시 mm분 ss초");
        }
    }

    static class BooleanExtension
    {
        public static string ToEmoji(this bool b)
        {
            return b ? ":white_check_mark:" : ":negative_squared_cross_mark:";
        }
    }
}
