using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast.Managements
{
    class DbManager
    {
        public static MongoClient Client = new MongoClient("mongodb://localhost");
        public static MongoDatabaseBase Db = (MongoDatabaseBase)Client.GetDatabase("oliveDb");

        public static IMongoCollection<OliveGuild> Guilds = Db.GetCollection<OliveGuild>("Guilds");
    }

    public class OliveGuild
    {
        public ObjectId Id;

        public ulong GuildId;

        public GuildSetting Setting;

        public OliveGuild(ulong id)
        {
            GuildId = id;
            Setting = new GuildSetting();
        }

        public static OliveGuild Get(ulong id)
        {
            return DbManager.Guilds.Find(g => g.GuildId == id).Single();
        }

        public static void Set(ulong id, Expression<Func<OliveGuild, object>> field, object value)
        {
            DbManager.Guilds.UpdateOne(g => g.GuildId == id, Builders<OliveGuild>.Update.Set(field, value));
        }

        public class GuildSetting
        {
            public ulong? LogChannelId;

            public enum LogTypes { 메시지수정, 메시지삭제, 메시지대량삭제, 채널생성, 채널삭제, 채널수정, 서버수정, 초대링크생성, 초대링크제거, 반응추가, 반응삭제, 모든반응삭제, 역할추가, 역할삭제, 역할수정, 차단, 차단해제, 입장, 퇴장, 유저수정, 음성상태수정, 음성서버수정 }
            public List<LogTypes> LogType;

            public GuildSetting()
            {
                LogChannelId = null;
                LogType = new List<LogTypes> { LogTypes.메시지수정, LogTypes.메시지삭제 };
            }
        }
    }

    class OliveUser
    {

    }
}
