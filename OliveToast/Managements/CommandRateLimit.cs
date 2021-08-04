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

        public static bool Check(ulong user)
        {
            if (RateLimits.ContainsKey(user))
            {
                if (RateLimits[user] < LimitCount)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public static bool AddCount(ulong user)
        {
            if (!Check(user))
            {
                return false;
            }

            if (RateLimits.ContainsKey(user))
            {
                RateLimits[user]++;
            }
            else
            {
                RateLimits.Add(user, 1);
            }
            StartTimer(user);

            return true;
        }

        private static void StartTimer(ulong user)
        {
            Task.Run(async () =>
            {
                await Task.Delay(LimitingTime * 1000);

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
