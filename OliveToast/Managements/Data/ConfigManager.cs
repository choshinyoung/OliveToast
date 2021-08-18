using Microsoft.Extensions.Configuration;

namespace OliveToast.Managements.Data
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
