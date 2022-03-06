using Discord.Commands;
using Discord.Rest;
using System;
using System.Collections.Generic;

namespace OliveToast.Managements
{
    public class WordSession
    {
        public SocketCommandContext Context;

        public bool IsKorean;
        public int letterLength = 2;

        public bool IsStarted = false;

        public List<string> Words = new();

        public List<ulong> Players = new();
        public ulong CurrentTurn;

        public RestUserMessage JoinMessage;
        public ulong LastBotMessage = 0;

        public DateTime LastActiveTime;

        public static Dictionary<ulong, WordSession> Sessions = new();

        public WordSession(SocketCommandContext context, bool isKorean, RestUserMessage joinMessage, DateTime lastActiveTime)
        {
            IsKorean = isKorean;
            Context = context;
            LastActiveTime = lastActiveTime;
            JoinMessage = joinMessage;
        }
    }

    public class TypingSession
    {
        public SocketCommandContext Context;

        public string Sentence;

        public DateTimeOffset LastActiveTime;

        public static Dictionary<ulong, TypingSession> Sessions = new();

        public TypingSession(SocketCommandContext context, string sentence, DateTimeOffset lastActiveTime)
        {
            Context = context;
            Sentence = sentence;
            LastActiveTime = lastActiveTime;
        }
    }
}
