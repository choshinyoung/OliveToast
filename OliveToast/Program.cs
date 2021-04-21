using Microsoft.Extensions.Configuration;
using System;

namespace OliveToast
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            
            var config = builder.Build();
            Console.WriteLine(config.GetSection("TOKEN").Value);
        }
    }
}
