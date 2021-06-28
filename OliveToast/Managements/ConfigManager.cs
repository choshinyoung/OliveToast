using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace OliveToast.Managements
{
    class ConfigManager
    {
        private static readonly IConfigurationRoot config;

        static ConfigManager()
        {
            config = new ConfigurationBuilder().AddJsonFile("Configs/appsettings.json").Build();
        }

        public static string Get(string key)
        {
            return config.GetSection(key).Value;
        }
    }
}
