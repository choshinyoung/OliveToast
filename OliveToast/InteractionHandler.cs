﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OliveToast.Commands;
using OliveToast.Managements;
using OliveToast.Managements.CustomCommand;
using OliveToast.Managements.Data;
using OliveToast.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OliveToast
{
    class InteractionHandler
    {
        public enum InteractionType
        {
            None,
            CancelTypingGame, CancelWordGame,
            HelpList, HelpCategoryListSelectMenu,
            CreateCommand, DeleteCommand,
            CommandList, CommandAnswerList, CommandSearchList,
            CommandListSelectMenu, CommandAnswerListSelectMenu,
            RoleMenu,
            HelpCommandListSelectMenu,
            JoinWordGame, StartWordGame,
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
            { InteractionType.HelpList, HelpList },
            { InteractionType.HelpCategoryListSelectMenu, HelpCategoryListSelectMenu },
            { InteractionType.HelpCommandListSelectMenu, HelpCommandListSelectMenu },
            { InteractionType.CreateCommand, CreateCommand },
            { InteractionType.DeleteCommand, DeleteCommand },
            { InteractionType.CommandList, CommandList },
            { InteractionType.CommandAnswerList, CommandAnswerList },
            { InteractionType.CommandSearchList, CommandSearchList },
            { InteractionType.CommandListSelectMenu, CommandListSelctMenu },
            { InteractionType.CommandAnswerListSelectMenu, CommandAnswerListSelectMenu },
            { InteractionType.RoleMenu, RoleMenu },
            { InteractionType.JoinWordGame, JoinWordGame },
            { InteractionType.StartWordGame, StartWordGame },
        };

        public static async Task OnInteractionCreated(SocketMessageComponent component)
        {
            InteractionData data = new(component.Data.CustomId);

            if (component.User.Id != data.UserId &&
                data.Type is not InteractionType.RoleMenu and not InteractionType.JoinWordGame and not InteractionType.StartWordGame and not InteractionType.CancelWordGame)
            {
                return;
            }

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
            if (!WordSession.Sessions.ContainsKey(component.Message.Id))
            {
                return;
            }

            var session = WordSession.Sessions[component.Message.Id];

            if (session.Context.User.Id != component.User.Id)
            {
                await component.RespondAsync("주최자만 게임을 취소할 수 있어요", ephemeral: true);

                return;
            }

            if (WordSession.Sessions.Any(s => s.Value.Context.User.Id == data.UserId))
            {
                await session.Context.ReplyEmbedAsync("게임이 취소됐어요");
                WordSession.Sessions.Remove(component.Message.Id);
            }

            await component.DeferAsync();
        }

        public static async Task JoinWordGame(InteractionData data, SocketMessageComponent component)
        {
            if (!WordSession.Sessions.ContainsKey(component.Message.Id))
            {
                return;
            }

            var session = WordSession.Sessions[component.Message.Id];

            if (session.IsStarted)
            {
                return;
            }

            if (session.Players.Contains(component.User.Id))
            {
                if (session.Context.User.Id == component.User.Id)
                {
                    await component.RespondAsync("주최자는 게임 참가를 취소할 수 없어요", ephemeral: true);

                    return;
                }

                session.Players.Remove(component.User.Id);

                await component.RespondAsync("게임 참가를 취소했어요", ephemeral: true);
            }
            else
            {
                session.Players.Add(component.User.Id);

                await component.RespondAsync("게임에 참가했어요", ephemeral: true);
            }

            EmbedBuilder emb = session.JoinMessage.Embeds.First().ToEmbedBuilder();
            emb.Description = $"현재 참가자: {string.Join(" ", session.Players.Select(p => session.Context.Guild.GetUser(p).Mention))}";

            await session.JoinMessage.ModifyAsync(m => m.Embed = emb.Build());

            await component.DeferAsync();
        }

        public static async Task StartWordGame(InteractionData data, SocketMessageComponent component)
        {
            if (!WordSession.Sessions.ContainsKey(component.Message.Id))
            {
                return;
            }

            var session = WordSession.Sessions[component.Message.Id];

            if (session.Context.User.Id != component.User.Id)
            {
                await component.RespondAsync("주최자만 게임을 시작할 수 있어요", ephemeral: true);

                return;
            }

            await Games.StartWordGame(session);

            await component.DeferAsync();
        }

        public static async Task HelpList(InteractionData data, SocketMessageComponent component)
        {
            int page = int.Parse(data.Args[0]);

            await Helps.ChangeCategoryListPage(data.UserId, component.Message, page);

            await component.DeferAsync();
        }

        public static async Task HelpCategoryListSelectMenu(InteractionData data, SocketMessageComponent component)
        {
            await Helps.ChangeCategoryListPage(data.UserId, component.Message, int.Parse(component.Data.Values.ToArray()[0]));

            await component.DeferAsync();
        }

        public static async Task HelpCommandListSelectMenu(InteractionData data, SocketMessageComponent component)
        {
            await Helps.UpdateCommandInfo(component.Message, component.Data.Values.ToArray()[0]);

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

        public static async Task RoleMenu(InteractionData data, SocketMessageComponent component)
        {
            SocketGuild guild = Program.Client.GetGuild(ulong.Parse(data.Args[0]));
            SocketGuildUser user = guild.GetUser(component.User.Id);
            SocketRole role = guild.GetRole(ulong.Parse(component.Data.Values.ToArray()[0]));
            bool isSingle = data.Args[1] == "1";

            if (user.Roles.Contains(role))
            {
                await user.RemoveRoleAsync(role);

                await component.RespondAsync($"{role.Mention} 역할을 제거했어요", ephemeral: true, allowedMentions: AllowedMentions.None);
            }
            else
            {
                List<SocketRole> removedRoles = new();

                if (isSingle)
                {
                    List<SocketRole> roles = (component.Message.Components.First().Components.First() as SelectMenuComponent).Options.Select(o => guild.GetRole(ulong.Parse(o.Value))).ToList();

                    foreach (SocketRole _role in roles)
                    {
                        if (user.Roles.Contains(_role))
                        {
                            removedRoles.Add(_role);
                        }
                    }

                    await user.RemoveRolesAsync(removedRoles);
                }

                await user.AddRoleAsync(ulong.Parse(component.Data.Values.ToArray()[0]));

                string removedRoleMessage = removedRoles.Any() ? $"{string.Join(", ", removedRoles.Select(r => r.Mention))} 역할을 제거하고 " : "";
                await component.RespondAsync($"{removedRoleMessage}{role.Mention} 역할을 추가했어요", ephemeral: true, allowedMentions: AllowedMentions.None);
            }
        }
    }
}