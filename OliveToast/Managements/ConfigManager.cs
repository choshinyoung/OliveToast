﻿using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace OliveToast.Managements
{
    class ConfigManager
    {
        private static readonly IConfigurationRoot config;

        static ConfigManager()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            config = builder.Build();
        }

        public static string Get(string key)
        {
            return config.GetSection(key).Value;
        }
    }
}
