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

    public class ScratchDbUserResult
    {
        public string username { get; set; }
        public int id { get; set; }
        public int sys_id { get; set; }
        public DateTime joined { get; set; }
        public string country { get; set; }
        public string bio { get; set; }
        public string work { get; set; }
        public string status { get; set; }
        public object school { get; set; }
        public Statistics statistics { get; set; }

        public class Country
        {
            public int loves { get; set; }
            public int favorites { get; set; }
            public int comments { get; set; }
            public int views { get; set; }
            public int followers { get; set; }
            public int following { get; set; }
        }

        public class Ranks
        {
            public Country country { get; set; }
            public int loves { get; set; }
            public int favorites { get; set; }
            public int comments { get; set; }
            public int views { get; set; }
            public int followers { get; set; }
            public int following { get; set; }
        }

        public class Statistics
        {
            public Ranks ranks { get; set; }
            public int loves { get; set; }
            public int favorites { get; set; }
            public int comments { get; set; }
            public int views { get; set; }
            public int followers { get; set; }
            public int following { get; set; }
        }
    }

    public class ScratchApiUserResult
    {
        public int id { get; set; }
        public string username { get; set; }
        public bool scratchteam { get; set; }
        public History history { get; set; }
        public Profile profile { get; set; }

        public class History
        {
            public DateTime joined { get; set; }
        }

        public class Images
        {
            public string _90x90 { get; set; }
            public string _60x60 { get; set; }
            public string _55x55 { get; set; }
            public string _50x50 { get; set; }
            public string _32x32 { get; set; }
        }

        public class Profile
        {
            public int id { get; set; }
            public Images images { get; set; }
            public string status { get; set; }
            public string bio { get; set; }
            public string country { get; set; }
        }
    }

    public class ScratchProjectResult
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string instructions { get; set; }
        public string visibility { get; set; }
        public bool @public { get; set; }
        public bool comments_allowed { get; set; }
        public bool is_published { get; set; }
        public Author author { get; set; }
        public string image { get; set; }
        public Images images { get; set; }
        public History history { get; set; }
        public Stats stats { get; set; }
        public Remix remix { get; set; }

        public class History
        {
            public DateTime joined { get; set; }
            public DateTime created { get; set; }
            public DateTime modified { get; set; }
            public DateTime shared { get; set; }
        }

        public class Images
        {
            public string _90x90 { get; set; }
            public string _60x60 { get; set; }
            public string _55x55 { get; set; }
            public string _50x50 { get; set; }
            public string _32x32 { get; set; }
            public string _282x218 { get; set; }
            public string _216x163 { get; set; }
            public string _200x200 { get; set; }
            public string _144x108 { get; set; }
            public string _135x102 { get; set; }
            public string _100x80 { get; set; }
        }

        public class Profile
        {
            public object id { get; set; }
            public Images images { get; set; }
        }

        public class Author
        {
            public int id { get; set; }
            public string username { get; set; }
            public bool scratchteam { get; set; }
            public History history { get; set; }
            public Profile profile { get; set; }
        }

        public class Stats
        {
            public int views { get; set; }
            public int loves { get; set; }
            public int favorites { get; set; }
            public int remixes { get; set; }
        }

        public class Remix
        {
            public object root { get; set; }
        }
    }

    public class MinecraftUuidResult
    {
        public string name { get; set; }
        public string id { get; set; }
    }

    public class MinecraftNameResult
    {
        public string name { get; set; }
        public long? changedToAt { get; set; }
    }

    public class MinecraftProfileResult
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<Property> properties { get; set; }

        public class Property
        {
            public string name { get; set; }
            public string value { get; set; }
        }
    }

    public class MinecraftSkinResult
    {
        public long timestamp { get; set; }
        public string profileId { get; set; }
        public string profileName { get; set; }
        public Textures textures { get; set; }

        public class SKIN
        {
            public string url { get; set; }
        }

        public class Textures
        {
            public SKIN SKIN { get; set; }
        }
    }

    public class Voted
    {
        public int code { get; set; }
        public Data data { get; set; }
        public int version { get; set; }

        public class Data
        {
            public bool voted { get; set; }
            public long lastVote { get; set; }
        }
    }
}
