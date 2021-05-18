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

            public GuildSetting()
            {
                LogChannelId = null;
            }
        }
    }
}
