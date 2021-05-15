using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast.Managements
{
    public class PingPongResult
    {
        public Response response { get; set; }
        public string version { get; set; }

        public class From
        {
            public double score { get; set; }
            public string name { get; set; }
            public string link { get; set; }
            public string from { get; set; }
        }

        public class Image
        {
            public string url { get; set; }
        }

        public class Reply
        {
            public From from { get; set; }
            public string type { get; set; }
            public string text { get; set; }
            public Image image { get; set; }
        }

        public class Response
        {
            public List<Reply> replies { get; set; }
        }
    }

    public class WikiResult
    {
        public int result { get; set; }
        public ReturnObject return_object { get; set; }

        public class IRInfo
        {
            public string wiki_title { get; set; }
            public string sent { get; set; }
            public string url { get; set; }
        }

        public class AnswerInfo
        {
            public double rank { get; set; }
            public string answer { get; set; }
            public double confidence { get; set; }
            public List<string> url { get; set; }
        }

        public class WiKiInfo
        {
            public List<IRInfo> IRInfo { get; set; }
            public List<AnswerInfo> AnswerInfo { get; set; }
        }

        public class ReturnObject
        {
            public WiKiInfo WiKiInfo { get; set; }
        }
    }

    public class DicResult
    {
        public int result { get; set; }
        public ReturnObject return_object { get; set; }

        public class MetaInfo
        {
            public string Title { get; set; }
            public string Link { get; set; }
        }

        public class WordInfo
        {
            public double PolysemyCode { get; set; }
            public string Definition { get; set; }
            public string POS { get; set; }
            public List<string> Hypernym { get; set; }
            public List<object> Hypornym { get; set; }
        }

        public class WWNWordInfo
        {
            public string Word { get; set; }
            public double HomonymCode { get; set; }
            public List<WordInfo> WordInfo { get; set; }
            public List<object> Synonym { get; set; }
            public List<object> Antonym { get; set; }
        }

        public class ReturnObject
        {
            public MetaInfo MetaInfo { get; set; }
            public List<WWNWordInfo> WWN_WordInfo { get; set; }
        }
    }

    public class KoreanBotsResult
    {
        public int code { get; set; }
        public List<Bot> data { get; set; }
        public int currentPage { get; set; }
        public int totalPage { get; set; }

        public class Bot
        {
            public string id { get; set; }
            public string name { get; set; }
            public int servers { get; set; }
            public int votes { get; set; }
            public string intro { get; set; }
            public string avatar { get; set; }
            public string url { get; set; }
            public List<string> category { get; set; }
            public string tag { get; set; }
            public string status { get; set; }
            public string state { get; set; }
            public int verified { get; set; }
            public int trusted { get; set; }
            public int boosted { get; set; }
            public object vanity { get; set; }
            public object bg { get; set; }
            public object banner { get; set; }
        }
    }
}
