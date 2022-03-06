using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace OliveToast.Managements.Data
{
    public class WordsManager
    {
        public static readonly List<string> Words;

        static WordsManager()
        {
            string content = File.ReadAllText("Configs/words.json");

            Words = JsonConvert.DeserializeObject<List<string>>(content);
        }
    }

    public class SentenceManager
    {
        public static readonly List<string> Sentences;
        public static readonly List<string> EnSentences;

        static SentenceManager()
        {
            string content = File.ReadAllText("Configs/sentences.json");
            Sentences = JsonConvert.DeserializeObject<List<string>>(content);

            content = File.ReadAllText("Configs/sentencesEn.json");
            EnSentences = JsonConvert.DeserializeObject<List<string>>(content);

            /*
                Sentences.Sort();
                File.WriteAllText("Configs/sentences.json", JsonConvert.SerializeObject(Sentences));

                EnSentences.Sort();
                File.WriteAllText("Configs/sentencesEn.json", JsonConvert.SerializeObject(EnSentences));
            */
        }
    }
}
