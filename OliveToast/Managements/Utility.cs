using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast.Managements
{
    class Utility
    {

    }

    class DiscordUserUtility
    {
        public static string GetAvatar(SocketUser user)
        {
            return user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
        }

        public static string GetName(SocketUser user, bool isPrivate)
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
