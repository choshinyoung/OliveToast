using Discord;
using Discord.WebSocket;
using HPark.Hangul;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast.Utilities
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
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp).AddHours(9);
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

    static class DiscordUserExtension
    {
        public static string GetAvatar(this IUser user)
        {
            if (user is SocketGuildUser guildUser)
            {
                string avatar = guildUser.GetGuildAvatarUrl();

                if (avatar != null)
                {
                    return avatar;
                }
            }

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
            return time.UtcDateTime.AddHours(9);
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
