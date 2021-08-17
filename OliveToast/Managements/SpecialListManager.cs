using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast.Managements
{
    class SpecialListManager
    {
        const string WhiteListFilePath = @"Configs\whitelist.json";
        const string BlackListFilePath = @"Configs\blacklist.json";

        public static readonly List<ulong> WhiteList;
        public static readonly List<ulong> BlackList;

        static SpecialListManager()
        {
            WhiteList = JsonConvert.DeserializeObject<List<ulong>>(File.ReadAllText(WhiteListFilePath));
            BlackList = JsonConvert.DeserializeObject<List<ulong>>(File.ReadAllText(BlackListFilePath));
        }

        public static void Update()
        {
            File.WriteAllText(WhiteListFilePath, JsonConvert.SerializeObject(WhiteList));
            File.WriteAllText(BlackListFilePath, JsonConvert.SerializeObject(BlackList));
        }

        public static bool IsWhiteList(ulong id)
            => id == 396163884005851137 || WhiteList.Contains(id);

        public static bool IsBlackList(ulong id)
            => id != 396163884005851137 && BlackList.Contains(id);
    }
}
