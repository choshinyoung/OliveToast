using HPark.Hangul;
using System.IO;
using System.Linq;
using System.Text;

namespace OliveToast.Utilities
{
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
            MemoryStream stream = new(Encoding.UTF8.GetBytes(content));

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
            HangulChar hc = new(content.Last());
            bool canSplit = hc.TrySplitSyllable(out char[] syllables);

            return content + suffix + (canSplit && syllables[2] != '\u0000' && syllables[2] != 'ㄹ' ? "으로" : "로");
        }

        private static bool HaveJongsung(char c)
        {
            HangulChar hc = new(c);
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
}
