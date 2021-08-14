using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Toast;
using Toast.Nodes;

namespace OliveToast.Managements
{
    class CustomCommandExecutor
    {
        public static Toaster GetToaster()
        {
            Toaster toaster = new();

            toaster.AddCommand(BasicCommands.Literals);
            toaster.AddCommand(BasicCommands.Operators.Except(new List<ToastCommand>() { BasicCommands.Equal }).ToArray());
            toaster.AddCommand(BasicCommands.Strings);
            toaster.AddCommand(BasicCommands.Lists);
            toaster.AddCommand(BasicCommands.If, BasicCommands.Else, BasicCommands.Foreach, BasicCommands.Assign, BasicCommands.Random, BasicCommands.RandomChoice);
            toaster.AddCommand(OliveToastCommands.Commands.ToArray());

            toaster.AddConverter(BasicConverters.All);
            toaster.AddConverter(OliveToastCommands.Converters.ToArray());

            return toaster;
        }

        public static async Task Execute(SocketCommandContext context, OliveGuild guild)
        {
            List<(string[] groups, OliveGuild.CustomCommand command)> answers = FindAnswers(guild, context.Message.Content);
            if (!answers.Any())
            {
                return;
            }

            if (!CommandExecuteSession.Sessions.ContainsKey(context.User.Id) && CommandRateLimit.AddCount(context.User.Id))
            {
                var command = answers[new Random().Next(answers.Count)];

                SocketUserMessage botMessage = null;
                if (command.command.Answer is not null)
                {
                    botMessage = await context.Channel.GetMessageAsync((await context.MsgReplyAsync(command.command.Answer)).Id) as SocketUserMessage;
                }

                Toaster toaster = GetToaster();
                CustomCommandContext toastContext = new(context, command.groups, command.command.CanKickUser, command.command.CanBanUser, command.command.CanManageRole, botMessage);

                CommandExecuteSession.Sessions.Add(context.User.Id, new(toastContext));

                foreach (string line in command.command.ToastLines)
                {
                    try
                    {
                        toaster.Execute(line, toastContext);
                    }
                    catch (Exception e)
                    {
                        EmbedBuilder emb = context.CreateEmbed(title: "오류 발생!", description: e.GetBaseException().Message);
                        await context.MsgReplyEmbedAsync(emb.Build());

                        if (CommandExecuteSession.Sessions.ContainsKey(context.User.Id))
                        {
                            CommandExecuteSession.Sessions.Remove(context.User.Id);
                        }

                        return;
                    }

                    if (!CommandExecuteSession.Sessions.ContainsKey(context.User.Id))
                    {
                        return;
                    }
                }

                if (CommandExecuteSession.Sessions.ContainsKey(context.User.Id))
                {
                    CommandExecuteSession.Sessions.Remove(context.User.Id);
                }
            }
            else
            {
                await context.Message.AddReactionAsync(new Emoji("🚫"));
            }
        }

        public static List<(string[] groups, OliveGuild.CustomCommand command)> FindAnswers(OliveGuild guild, string command)
        {
            var commands = guild.Commands;

            List<(string[] groups, OliveGuild.CustomCommand command)> answers = new();

            if (commands.ContainsKey(command) && commands[command].Any(c => !c.IsRegex))
            {
                answers.AddRange(commands[command].Where(c => !c.IsRegex).Select(c => (new[] { command }, c)));
            }
            foreach (var cmd in commands)
            {
                if (!cmd.Value.Any(a => a.IsRegex))
                {
                    continue;
                }

                Match match = new Regex(cmd.Key).Match(command);

                if (!match.Success || match.Value.Length != command.Length)
                {
                    continue;
                }

                answers.AddRange(cmd.Value.Where(a => a.IsRegex).Select(c => (match.Groups.Values.Select(v => v.Value).ToArray(), c)));
            }

            return answers;
        }

        public static List<string> FindCommands(OliveGuild guild, string command)
        {
            var commands = guild.Commands;

            List<string> answers = new();

            if (commands.ContainsKey(command) && commands[command].Any(c => !c.IsRegex))
            {
                answers.Add(command);
            }
            foreach (var cmd in commands)
            {
                if (!cmd.Value.Any(a => a.IsRegex))
                {
                    continue;
                }

                Match match = new Regex(cmd.Key).Match(command);

                if (!match.Success || match.Value.Length != command.Length)
                {
                    continue;
                }

                if (!answers.Contains(cmd.Key))
                {
                    answers.Add(cmd.Key);
                }
            }

            return answers;
        }

        public static async Task<bool> OnMessageReceived(SocketCommandContext context)
        {
            if (!CommandExecuteSession.Sessions.ContainsKey(context.User.Id))
            {
                return false;
            }

            var session = CommandExecuteSession.Sessions[context.User.Id];
            if (session.Context.DiscordContext.Channel.Id != context.Channel.Id)
            {
                return false;
            }

            session.Context.UserLastMessage = context.Message;
            session.Context.ContextMessages.Add(context.Message.Id);
            if (session.Context.OnMessageReceived is null)
            {
                return false;
            }

            try
            {
                session.Context.Toaster.ExecuteFunction(session.Context.OnMessageReceived, new object[] { context.Message.Content }, session.Context);
            }
            catch (Exception e)
            {
                EmbedBuilder emb = context.CreateEmbed(title: "오류 발생!", description: e.GetBaseException().Message);
                await context.MsgReplyEmbedAsync(emb.Build());

                if (CommandExecuteSession.Sessions.ContainsKey(context.User.Id))
                {
                    CommandExecuteSession.Sessions.Remove(context.User.Id);
                }
            }

            return true;
        }

        public static async Task OnReactionAdded(SocketReaction reaction)
        {
            var session = CommandExecuteSession.Sessions.Values.ToList().Find(s => s.Context.ContextMessages.Contains(reaction.MessageId));

            if (session is null || session.Context.OnReactionAdded is null)
            {
                return;
            }

            try {
                session.Context.Toaster.ExecuteFunction(session.Context.OnReactionAdded,
                    new object[] {
                        reaction.Message,
                        (reaction.Channel as SocketTextChannel).Guild.GetUser(reaction.UserId),
                        reaction.Emote.Name
                    }, session.Context);
            }
            catch (Exception e)
            {
                var context = session.Context.DiscordContext;

                EmbedBuilder emb = context.CreateEmbed(title: "오류 발생!", description: e.GetBaseException().Message);
                await context.MsgReplyEmbedAsync(emb.Build());

                if (CommandExecuteSession.Sessions.ContainsKey(context.User.Id))
                {
                    CommandExecuteSession.Sessions.Remove(context.User.Id);
                }
            }
        }
    }
    
    class CommandExecuteSession
    {
        public static readonly Dictionary<ulong, CommandExecuteSession> Sessions = new();

        public readonly CustomCommandContext Context;
        public readonly DateTime StartTime;

        public CommandExecuteSession(CustomCommandContext context)
        {
            Context = context;
            StartTime = DateTime.Now;
        }
    }

    class CustomCommandContext : ToastContext
    {
        public readonly SocketCommandContext DiscordContext;
        public readonly string[] Groups;

        public readonly SocketUserMessage BotMessage;
        public SocketUserMessage UserLastMessage;
        public SocketUserMessage BotLastMessage;

        public int SendCount;

        public FunctionNode OnMessageReceived;
        public FunctionNode OnReactionAdded;

        public bool CanKickUser;
        public bool CanBanUser;
        public bool CanManageRole;

        public readonly List<ulong> ContextMessages;

        public CustomCommandContext(SocketCommandContext context, string[] groups, bool canKickUser, bool canBanUser, bool canManageeRole, SocketUserMessage botMessage = null)
        {
            DiscordContext = context;
            Groups = groups;

            CanKickUser = canKickUser;
            CanBanUser = canBanUser;
            CanManageRole = canManageeRole;

            UserLastMessage = context.Message;
            BotMessage = botMessage;
            BotLastMessage = botMessage;

            SendCount = 0;

            ContextMessages = new() { context.Message.Id };
            if (BotMessage is not null)
            {
                ContextMessages.Add(BotMessage.Id);
            }
        }
    }
}
