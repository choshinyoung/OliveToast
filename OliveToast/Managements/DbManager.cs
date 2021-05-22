using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static OliveToast.Managements.RequireCategoryEnable;
using static OliveToast.Managements.RequirePermission;

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
            public List<CategoryType> EnabledCategories;
            public Dictionary<string, ulong> PermissionRoles;

            public GuildSetting()
            {
                LogChannelId = null;
                EnabledCategories = new List<CategoryType>() { CategoryType.Default, CategoryType.Info, CategoryType.Search, CategoryType.Game, CategoryType.Text, CategoryType.Image, CategoryType.Setting };
                PermissionRoles = new Dictionary<string, ulong>();
            }
        }
    }

    class OliveUser
    {

    }
}
