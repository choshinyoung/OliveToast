using Discord.Commands;
using System;
using System.Collections.Generic;

namespace OliveToast.Utilities
{
    class WordSession
    {
        public SocketCommandContext Context;
        public List<string> Words;
        public DateTime LastActiveTime;

        public static Dictionary<ulong, WordSession> Sessions = new();

        public WordSession(SocketCommandContext context, List<string> words, DateTime lastActiveTime) 
        {
            Context = context;
            Words = words;
            LastActiveTime = lastActiveTime;
        }
    }

    class TypingSession
    {
        public SocketCommandContext Context;
        public string Sentence;
        public DateTime LastActiveTime;

        public static Dictionary<ulong, TypingSession> Sessions = new();

        public TypingSession(SocketCommandContext context, string sentence, DateTime lastActiveTime)
        {
            Context = context;
            Sentence = sentence;
            LastActiveTime = lastActiveTime;
        }
    }
}
