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
        public static Dictionary<ulong, (ulong channel, List<string> words)> Sessions = new();
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
        public static Dictionary<ulong, (ulong channel, string sentence, DateTime StartTime)> Sessions = new();
    }
}
