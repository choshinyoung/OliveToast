﻿using Discord;
using Discord.WebSocket;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.Net;

using Color = System.Drawing.Color;

namespace OliveToast.Utilities
{
    public class Utility
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

        public static Font GetFont(float size = 13f, FontStyle style = FontStyle.Regular)
        {
            return new Font(GetFontFamily(), size, style);
        }

        public static FontFamily GetFontFamily()
        {
            PrivateFontCollection collection = new();
            collection.AddFontFile("Configs/NotoSansKR.otf");
            return new FontFamily("Noto Sans KR", collection);
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
        public static string ToHex(this Color color)
        {
            return $"#{toHex(color.R)}{toHex(color.G)}{toHex(color.B)}";

            static string toHex(int dec)
            {
                string result = string.Format("{0:x2}", dec).ToUpper();
                    
                return result;
            }
        }

        public static string ToFormattedString(this Color color)
        {
            return $"rgb({color.R}, {color.G}, {color.B})";
        }

        public static (int hue, byte saturation, byte value) ToHSV(this Color color)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            int hue = (int)color.GetHue();
            double saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            double value = max / 255d;

            return (hue, (byte)Math.Round(saturation * 100), (byte)Math.Round(value * 100));
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
