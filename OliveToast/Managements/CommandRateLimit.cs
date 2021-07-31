using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OliveToast.Managements
{
    class CommandRateLimit
    {
        public const int LimitCount = 5;
        public const int LimitingTime = 10;

        private static readonly Dictionary<ulong, int> RateLimits = new();

        public static bool AddCount(ulong user)
        {
            if (RateLimits.ContainsKey(user))
            {
                if (RateLimits[user] < 5)
                {
                    RateLimits[user]++;
                    StartTimer(user);

                    return true;
                }
            }
            else
            {
                RateLimits.Add(user, 1);
                StartTimer(user);

                return true;
            }

            return false;
        }

        private static void StartTimer(ulong user)
        {
            Task.Run(async () =>
            {
                Console.WriteLine("s");
                await Task.Delay(LimitingTime * 1000);
                Console.WriteLine("e");

                if (RateLimits.ContainsKey(user)) 
                {
                    if ((RateLimits[user]--) <= 0)
                    {
                        RateLimits.Remove(user);
                    }
                }
            });
        }
    }
}
