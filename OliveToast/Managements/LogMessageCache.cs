using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OliveToast.Managements
{
    public record LoggedMessage(IUser Author, string Content, IReadOnlyList<string> AttachmentUrls);

    public static class LogMessageCache
    {
        public static readonly TimeSpan Retention = TimeSpan.FromDays(7);
        public const int MaxMessagesPerGuild = 10000;

        private static readonly Dictionary<ulong, SortedDictionary<ulong, LoggedMessage>> guilds = new();

        public static LoggedMessage Store(ulong guildId, IMessage message)
        {
            lock (guilds)
            {
                if (!guilds.TryGetValue(guildId, out var messages))
                {
                    messages = new();
                    guilds.Add(guildId, messages);
                }

                messages.TryGetValue(message.Id, out LoggedMessage previous);
                messages[message.Id] = new(message.Author, message.Content, message.Attachments.Select(a => a.ProxyUrl).ToList());

                while (messages.Count > MaxMessagesPerGuild)
                {
                    messages.Remove(messages.Keys.First());
                }

                return previous;
            }
        }

        public static LoggedMessage Remove(ulong guildId, ulong messageId)
        {
            lock (guilds)
            {
                if (!guilds.TryGetValue(guildId, out var messages))
                {
                    return null;
                }

                messages.Remove(messageId, out LoggedMessage removed);

                return removed;
            }
        }

        public static void Clear(ulong guildId)
        {
            lock (guilds)
            {
                guilds.Remove(guildId);
            }
        }

        public static void RemoveExpired()
        {
            ulong oldestId = SnowflakeUtils.ToSnowflake(DateTimeOffset.UtcNow - Retention);

            lock (guilds)
            {
                foreach (var (guildId, messages) in guilds.ToList())
                {
                    while (messages.Count > 0 && messages.Keys.First() < oldestId)
                    {
                        messages.Remove(messages.Keys.First());
                    }

                    if (messages.Count == 0)
                    {
                        guilds.Remove(guildId);
                    }
                }
            }
        }
    }
}
