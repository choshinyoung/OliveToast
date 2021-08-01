using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast.Managements
{
    class WordsManager
    {
        public static readonly List<string> Words;

        static WordsManager()
        {
            string content = File.ReadAllText("Configs/words.json");

            Words = JsonConvert.DeserializeObject<List<string>>(content);
        }
    }

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

    class SentenceManager
    {
        public static readonly List<string> Sentences;
        public static readonly List<string> EnSentences;

        static SentenceManager()
        {
            string content = File.ReadAllText("Configs/sentences.json");
            Sentences = JsonConvert.DeserializeObject<List<string>>(content);

            content = File.ReadAllText("Configs/sentencesEn.json");
            EnSentences = JsonConvert.DeserializeObject<List<string>>(content);
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
