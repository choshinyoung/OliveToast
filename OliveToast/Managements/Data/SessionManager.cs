using Discord;
using OliveToast.Managements.CustomCommand;
using OliveToast.Utilities;
using System;
using System.Threading.Tasks;

namespace OliveToast.Managements.Data
{
    class SessionManager
    {
        public const int ExpireMinute = 5;
        public const int CommandExecuteSessionExpireMinute = 10;
        public const int ExceptionSessionExpireMinute = 3;

        public static async Task CollectExpiredSessions()
        {
            while (true)
            {
                await Task.Delay(1000 * 15);

                foreach (var session in WordSession.Sessions)
                {
                    if ((DateTime.Now - session.Value.LastActiveTime).TotalMinutes >= ExpireMinute)
                    {
                        WordSession.Sessions.Remove(session.Key);

                        await session.Value.Context.ReplyEmbedAsync("게임이 자동으로 중단됐어요");
                    }
                }

                foreach (var session in TypingSession.Sessions)
                {
                    if ((DateTime.Now - session.Value.LastActiveTime).TotalMinutes >= ExpireMinute)
                    {
                        TypingSession.Sessions.Remove(session.Key);

                        await session.Value.Context.ReplyEmbedAsync("게임이 자동으로 종료됐어요");
                    }
                }

                foreach (var session in CommandCreateSession.Sessions)
                {
                    if ((DateTime.Now - session.Value.LastActiveTime).TotalMinutes >= ExpireMinute)
                    {
                        CommandCreateSession.Sessions.Remove(session.Key);

                        await session.Value.UserMessageContext.ReplyEmbedAsync("커맨드 생성이 자동으로 종료됐어요");
                    }
                }

                foreach (var session in CommandExecuteSession.Sessions)
                {
                    if ((DateTime.Now - session.Value.StartTime).TotalMinutes >= CommandExecuteSessionExpireMinute)
                    {
                        CommandExecuteSession.Sessions.Remove(session.Key);

                        await session.Value.Context.Message.AddReactionAsync(new Emoji("⚠️"));
                    }
                }

                foreach (var session in CommandEventHandler.ExceptionSessions)
                {
                    if ((DateTime.Now - session.Value.occurredTime).TotalMinutes >= ExceptionSessionExpireMinute)
                    {
                        CommandEventHandler.ExceptionSessions.Remove(session.Key);
                    }
                }
            }
        }
    }
}
