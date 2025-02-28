﻿using System;
using System.Collections.Generic;

namespace OliveToast.Utilities
{
#pragma warning disable IDE1006 // 명명 스타일
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

    public class KoreanBotsResult
    {
        public int code { get; set; }
        public Data data { get; set; }
        public int version { get; set; }

        public class Data
        {
            public string type { get; set; }
            public List<InnerData> data { get; set; }
            public int currentPage { get; set; }
            public int totalPage { get; set; }
        }

        public class InnerData
        {
            public string id { get; set; }
            public Flag flags { get; set; }
            public List<Owner> owners { get; set; }
            public string lib { get; set; }
            public string prefix { get; set; }
            public int votes { get; set; }
            public int servers { get; set; }
            public string intro { get; set; }
            public string desc { get; set; }
            public string web { get; set; }
            public string git { get; set; }
            public string url { get; set; }
            public List<string> category { get; set; }
            public string status { get; set; }
            public string discord { get; set; }
            public string state { get; set; }
            public string vanity { get; set; }
            public string bg { get; set; }
            public string banner { get; set; }
            public string tag { get; set; }
            public string avatar { get; set; }
            public string name { get; set; }
        }

        public class Owner
        {
            public string id { get; set; }
            public int flags { get; set; }
            public string github { get; set; }
            public string tag { get; set; }
            public string username { get; set; }
            public List<string> bots { get; set; }
        }

        public enum Status
        {
            online, idle, dnd, streaming, offline
        }

        public enum Flag
        {
            None = 0 << 0,
            Official = 1 << 0,
            KoreanbotVerified = 1 << 2,
            Partner = 1 << 3,
            DiscordVerified = 1 << 4,
            Premium = 1 << 5,
            HackatonWinner = 1 << 6,
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
            public long? lastVote { get; set; }
        }
    }

#pragma warning restore IDE1006 // 명명 스타일
}