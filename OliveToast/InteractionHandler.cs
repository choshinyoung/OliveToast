using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OliveToast.Commands;
using OliveToast.Managements.CustomCommand;
using OliveToast.Managements.data;
using OliveToast.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast
{
    class InteractionHandler
    {
        public enum InteractionType
        {
            None,
            CancelTypingGame, CancelWordGame,
            CreateCommand, DeleteCommand,
            CommandList, CommandAnswerList, CommandSearchList,
            CommandListSelectMenu, CommandAnswerListSelectMenu,
        }

        public struct InteractionData
        {
            public ulong UserId;
            public InteractionType Type;
            public string[] Args;

            public InteractionData(string customId)
            {
                string[] args = customId.Split('.');

                UserId = ulong.Parse(args[0]);
                Type = (InteractionType)int.Parse(args[1]);

                Args = args[2..];
            }
        }

        public static string GenerateCustomId(ulong userId, InteractionType type, params object[] args)
        {
            return $"{userId}.{(int)type}.{string.Join('.', args)}";
        }

        public static Dictionary<InteractionType, Func<InteractionData, SocketMessageComponent, Task>> Functions = new()
        {
            { InteractionType.None, None },
            { InteractionType.CancelTypingGame, CancelTypingGame },
            { InteractionType.CancelWordGame, CancelWordGame },
            { InteractionType.CreateCommand, CreateCommand },
            { InteractionType.DeleteCommand, DeleteCommand },
            { InteractionType.CommandList, CommandList },
            { InteractionType.CommandAnswerList, CommandAnswerList },
            { InteractionType.CommandSearchList, CommandSearchList },
            { InteractionType.CommandListSelectMenu, CommandListSelctMenu },
            { InteractionType.CommandAnswerListSelectMenu, CommandAnswerListSelectMenu },
        };

        public static async Task OnInteractionCreated(SocketMessageComponent component)
        {
            InteractionData data = new(component.Data.CustomId);

            if (component.User.Id != data.UserId) return;

            await Functions[data.Type](data, component);
        }

        public static async Task None(InteractionData data, SocketMessageComponent component)
        {
            await Task.CompletedTask;
        }

        public static async Task CreateCommand(InteractionData data, SocketMessageComponent component)
        {
            CommandCreateSession.ResponseType response = (CommandCreateSession.ResponseType)int.Parse(data.Args[0]);
            await CommandCreateSession.ButtonResponse(data.UserId, response);

            await component.DeferAsync();
        }

        public static async Task CancelTypingGame(InteractionData data, SocketMessageComponent component)
        {
            if (!TypingSession.Sessions.ContainsKey(data.UserId))
            {
                return;
            }

            SocketCommandContext context = TypingSession.Sessions[data.UserId].Context;

            TypingSession.Sessions.Remove(data.UserId);
            await context.ReplyEmbedAsync("게임이 취소됐어요");

            await component.DeferAsync();
        }

        public static async Task CancelWordGame(InteractionData data, SocketMessageComponent component)
        {
            if (!WordSession.Sessions.ContainsKey(data.UserId))
            {
                return;
            }

            SocketCommandContext context = WordSession.Sessions[data.UserId].Context;

            WordSession.Sessions.Remove(data.UserId);
            await context.ReplyEmbedAsync("게임이 취소됐어요");

            await component.DeferAsync();
        }

        public static async Task CommandList(InteractionData data, SocketMessageComponent component)
        {
            SocketGuild guild = Program.Client.GetGuild(ulong.Parse(data.Args[0]));
            int page = int.Parse(data.Args[1]);

            await Command.ChangeListPage(guild, data.UserId, component.Message, page);

            await component.DeferAsync();
        }

        public static async Task CommandAnswerList(InteractionData data, SocketMessageComponent component)
        {
            SocketGuild guild = Program.Client.GetGuild(ulong.Parse(data.Args[0]));
            int command = int.Parse(data.Args[1]);
            int page = int.Parse(data.Args[2]);

            await Command.ChangeAnswerListPage(guild, data.UserId, component.Message, command, page);

            await component.DeferAsync();
        }

        public static async Task CommandSearchList(InteractionData data, SocketMessageComponent component)
        {
            SocketGuild guild = Program.Client.GetGuild(ulong.Parse(data.Args[0]));
            int page = int.Parse(data.Args[1]);
            string command = string.Join('.', data.Args[2..]);

            await Command.ChangeSearchPage(guild, data.UserId, component.Message, command, page);

            await component.DeferAsync();
        }

        public static async Task CommandListSelctMenu(InteractionData data, SocketMessageComponent component)
        {
            SocketGuild guild = Program.Client.GetGuild(ulong.Parse(data.Args[0]));

            await Command.ChangeAnswerListPage(guild, data.UserId, component.Message, int.Parse(component.Data.Values.ToArray()[0]), 1);

            await component.DeferAsync();
        }

        public static async Task CommandAnswerListSelectMenu(InteractionData data, SocketMessageComponent component)
        {
            SocketGuild guild = Program.Client.GetGuild(ulong.Parse(data.Args[0]));

            await Command.UpdateCommandInfo(guild, component.Message, int.Parse(data.Args[1]), int.Parse(component.Data.Values.ToArray()[0]));

            await component.DeferAsync();
        }

        public static async Task DeleteCommand(InteractionData data, SocketMessageComponent component)
        {
            if (!CommandDeleteSession.Sessions.ContainsKey(data.UserId))
            {
                return;
            }

            SocketCommandContext context = CommandDeleteSession.Sessions[data.UserId].Context;

            CommandDeleteSession.ResponseType deleteResponse = (CommandDeleteSession.ResponseType)int.Parse(data.Args[0]);

            var commands = OliveGuild.Get(context.Guild.Id).Commands;

            if (deleteResponse == CommandDeleteSession.ResponseType.Cancel)
            {
                CommandDeleteSession.Sessions.Remove(data.UserId);

                await context.ReplyEmbedAsync("커맨드 삭제를 취소했어요");

                await component.DeferAsync();

                return;
            }
            else if (deleteResponse == CommandDeleteSession.ResponseType.DeleteSingleAnswer)
            {
                string command = commands.Keys.ToList()[CommandDeleteSession.Sessions[data.UserId].CommandIndex];
                string answer = commands[command][CommandDeleteSession.Sessions[data.UserId].AnswerIndex].Answer;

                commands[command].RemoveAt(CommandDeleteSession.Sessions[data.UserId].AnswerIndex);
                if (!commands[command].Any())
                {
                    commands.Remove(command);
                }
                OliveGuild.Set(context.Guild.Id, g => g.Commands, commands);

                await context.ReplyEmbedAsync($"`{command}` 커맨드의 응답 `{answer.을를("`")} 삭제했어요");
            }
            else if (deleteResponse == CommandDeleteSession.ResponseType.DeleteAnswers)
            {
                commands[CommandDeleteSession.Sessions[data.UserId].Command].RemoveAll(c => c.Answer == CommandDeleteSession.Sessions[data.UserId].Answer);
                if (!commands[CommandDeleteSession.Sessions[data.UserId].Command].Any())
                {
                    commands.Remove(CommandDeleteSession.Sessions[data.UserId].Command);
                }

                OliveGuild.Set(context.Guild.Id, g => g.Commands, commands);

                await context.ReplyEmbedAsync($"`{CommandDeleteSession.Sessions[data.UserId].Command}` 커맨드의 응답 `{CommandDeleteSession.Sessions[data.UserId].Answer.을를("`")} 삭제했어요");
            }
            else if (deleteResponse == CommandDeleteSession.ResponseType.DeleteCommand)
            {
                commands.Remove(CommandDeleteSession.Sessions[data.UserId].Command);
                OliveGuild.Set(context.Guild.Id, g => g.Commands, commands);

                await context.ReplyEmbedAsync($"`{CommandDeleteSession.Sessions[data.UserId].Command}` 커맨드를 삭제했어요");
            }

            CommandDeleteSession.Sessions.Remove(data.UserId);

            await component.DeferAsync();
        }
    }
}