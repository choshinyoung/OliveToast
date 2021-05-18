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

    class OliveGuild
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

            public enum LogTypes { 메시지수정, 메시지삭제, 메시지뭉탱이삭제, 채널생성, 채널삭제, 채널설정, 서버설정, 초대링크생성, 초대링크삭제, 반응추가, 반응제거, 모든반응제거, 역할생성, 역할삭제, 역할설정, 차단, 입장, 퇴장, 차단해제, 유저설정, 음성상태설정, 음성서버설정 }
            public LogTypes LogType;

            public GuildSetting()
            {
                LogChannelId = null;
                LogType = LogTypes.메시지수정 & LogTypes.메시지삭제;
            }
        }
    }

    class OliveUser
    {

    }
}
