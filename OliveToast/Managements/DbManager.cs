﻿using Discord;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Toast.Nodes;
using static OliveToast.Managements.RequireCategoryEnable;
using static OliveToast.Managements.RequirePermission;

namespace OliveToast.Managements
{
    class DbManager
    {
        public static MongoClient Client = new("mongodb://localhost");
        public static MongoDatabaseBase Db = (MongoDatabaseBase)Client.GetDatabase("oliveDb");

        public static IMongoCollection<OliveGuild> Guilds = Db.GetCollection<OliveGuild>("Guilds");
    }

    public class OliveGuild
    {
        public ObjectId Id;

        public ulong GuildId;

        public GuildSetting Setting;

        public Dictionary<string, UserLevel> Levels;

        public Dictionary<string, List<CustomCommand>> Commands;

        public OliveGuild(ulong id)
        {
            GuildId = id;
            Setting = new();
            Levels = new();
            Commands = new();
        }

        public static OliveGuild Get(ulong id)
        {
            var searchResult = DbManager.Guilds.Find(g => g.GuildId == id);

            if (searchResult.Any())
            {
                return searchResult.Single();
            }
            else
            {
                OliveGuild guild = new(id);
                DbManager.Guilds.InsertOne(guild);

                return guild;
            }
        }

        public static void Set(ulong id, Expression<Func<OliveGuild, object>> field, object value)
        {
            DbManager.Guilds.UpdateOne(g => g.GuildId == id, Builders<OliveGuild>.Update.Set(field, value));
        }

        public class UserLevel
        {
            public int Level;
            public int Xp;

            public UserLevel()
            {
                Level = 0;
                Xp = 0;
            }
        }

        public class GuildSetting
        {
            public ulong? LogChannelId;
            public List<CategoryType> EnabledCategories;
            public Dictionary<string, ulong> PermissionRoles;

            public ulong? LevelUpChannelId;
            public List<ulong> NonXpChannels;
            public Dictionary<string, ulong> LevelRoles;

            public string JoinMessage;
            public string LeaveMessage;

            public GuildSetting()
            {
                LogChannelId = null;
                EnabledCategories = new List<CategoryType>() { CategoryType.Default, CategoryType.Info, CategoryType.Search, CategoryType.Game, CategoryType.Text, CategoryType.Image, CategoryType.Setting };
                PermissionRoles = new Dictionary<string, ulong>();

                LevelUpChannelId = null;
                NonXpChannels = new List<ulong>();
                LevelRoles = new Dictionary<string, ulong>();

                JoinMessage = "{userMention user}님, {serverName}에 오신 것을 환영합니다! :tada:";
                LeaveMessage = "{username user}님이 서버를 나가셨습니다 ㅠ.ㅠ";
            }
        }

        public class CustomCommand
        {
            public string Answer;

            public bool IsRegex;
            public List<string> ToastLines;

            public ulong CreatedBy;

            public bool CanKickUser;
            public bool CanBanUser;
            public bool CanManageRole;

            public CustomCommand(string answer, bool isRegex, List<string> lines, ulong createdBy, GuildPermissions acceptedPermissions)
            {
                Answer = answer;
                IsRegex = isRegex;
                ToastLines = lines;
                CreatedBy = createdBy;

                CanKickUser = acceptedPermissions.KickMembers;
                CanBanUser = acceptedPermissions.BanMembers;
                CanManageRole = acceptedPermissions.ManageRoles;
            }
        }
    }

    class OliveUser
    {

    }
}
