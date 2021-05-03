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
