using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using OliveToast.Commands;
using OliveToast.Managements.CustomCommand;
using OliveToast.Managements.data;
using OliveToast.Managements.Data;
using OliveToast.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toast;

namespace OliveToast
{
    class CommandEventHandler
    {
        public enum InteractionType
        {
            None, CreateCommand, CancelTypingGame, CancelWordGame, CommandList, CommandAnswerList, CommandSearch, DeleteCommand
        }

        public static readonly string prefix = ConfigManager.Get("PREFIX");

        public static void RegisterEvents(DiscordSocketClient client, CommandService command)
        {
            client.Log += OnLog;
            command.Log += OnCommandLog;

            client.MessageReceived += OnMessageReceived;
            client.ReactionAdded += OnReactionAdded;

            command.CommandExecuted += OnCommandExecuted;

            client.InteractionCreated += OnInteractionCreated;

            client.JoinedGuild += OnJoinGuild;
            client.LeftGuild += OnLeftGuild;

            client.UserJoined += OnUserJoined;
            client.UserLeft += OnUserLeft;

            client.Ready += OnReady;
        }

        public static async Task OnLog(LogMessage msg)
        {
            Console.WriteLine(msg);

            await Task.CompletedTask;
        }

        public static async Task OnCommandLog(LogMessage msg)
        {
            Console.WriteLine(msg);

            await Task.CompletedTask;
        }

        public static async Task OnJoinGuild(SocketGuild guild)
        {
            await KoreanBots.UpdateServerCountAsync(Program.Client.Guilds.Count);
        }

        public static async Task OnLeftGuild(SocketGuild arg)
        {
            await KoreanBots.UpdateServerCountAsync(Program.Client.Guilds.Count);
        }

        public static async Task OnReady()
        {
            await KoreanBots.UpdateServerCountAsync(Program.Client.Guilds.Count);

            await Task.Factory.StartNew(SessionManager.CollectExpiredSessions);
        }

        public static async Task OnUserJoined(SocketGuildUser arg)
        {
            new Task(async () =>
            {
                SocketGuild guild = arg.Guild;
                if (guild.SystemChannel is null)
                {
                    return;
                }

                OliveGuild.GuildSetting setting = OliveGuild.Get(guild.Id).Setting;

                string joinMessage = setting.JoinMessage;
                List<string> toastLines = setting.JoinMessageToastLines;

                Toaster toaster = CustomCommandExecutor.GetToaster();
                CustomCommandContext context = new(arg.Guild, guild.SystemChannel, null, arg, arg.Guild.OwnerId, Array.Empty<string>(), true, true, true);
                string output = (string)toaster.Execute($"\"{joinMessage}\"", context);

                if (output is not null and not "")
                {
                    if (guild.GetUser(Program.Client.CurrentUser.Id).GetPermissions(guild.SystemChannel).SendMessages)
                    {
                        await guild.SystemChannel.SendMessageAsync(output);
                    }
                }

                foreach (string line in toastLines)
                {
                    toaster.Execute(line, context);
                }
            }).Start();

            await Task.CompletedTask;
        }

        public static async Task OnUserLeft(SocketGuildUser arg)
        {
            new Task(async () =>
            {
                SocketGuild guild = arg.Guild;
                if (guild.SystemChannel is null)
                {
                    return;
                }

                OliveGuild.GuildSetting setting = OliveGuild.Get(guild.Id).Setting;

                string leaveMessage = setting.LeaveMessage;
                List<string> toastLines = setting.LeaveMessageToastLines;

                Toaster toaster = CustomCommandExecutor.GetToaster();
                CustomCommandContext context = new(arg.Guild, guild.SystemChannel, null, arg, arg.Guild.OwnerId, Array.Empty<string>(), true, true, true);
                string output = (string)toaster.Execute($"\"{leaveMessage}\"", context);

                if (output is not null and not "")
                {
                    if (guild.GetUser(Program.Client.CurrentUser.Id).GetPermissions(guild.SystemChannel).SendMessages)
                    {
                        await guild.SystemChannel.SendMessageAsync(output);
                    }
                }

                foreach (string line in toastLines)
                {
                    toaster.Execute(line, context);
                }
            }).Start();

            await Task.CompletedTask;
        }

        public static async Task OnMessageReceived(SocketMessage msg)
        {
            new Task(async () =>
            {
                if (msg is not SocketUserMessage userMsg || userMsg.Content == null ||
                    userMsg.Author.Id == Program.Client.CurrentUser.Id || userMsg.Author.IsBot || SpecialListManager.IsBlackList(msg.Author.Id)) return;

                SocketCommandContext context = new(Program.Client, userMsg);

                if (await Games.WordRelay(context) || await Games.TypingGame(context) || await CommandCreateSession.MessageResponse(context.User.Id, context.Channel.Id, context.Message.Content) || await CustomCommandExecutor.OnMessageReceived(context))
                {
                    return;
                }

                int argPos = 0;
                if (userMsg.HasStringPrefix(prefix, ref argPos) || userMsg.HasMentionPrefix(Program.Client.CurrentUser, ref argPos))
                {
                    if (Program.Command.Search(context, argPos).IsSuccess)
                    {
                        if (CommandRateLimit.AddCount(context.User.Id))
                        {
                            await Program.Command.ExecuteAsync(context, argPos, Program.Service);
                        }
                        else
                        {
                            await context.Message.AddReactionAsync(new Emoji("🚫"));
                        }
                    }
                }

                if (context.IsPrivate)
                {
                    return;
                }

                OliveGuild guild = OliveGuild.Get(context.Guild.Id);

                new Task(async () => await CustomCommandExecutor.Execute(context, guild)).Start();

                #region level
                if (!guild.Setting.EnabledCategories.Contains(RequireCategoryEnable.CategoryType.Level))
                {
                    return;
                }
                if (guild.Setting.NonXpChannels.Contains(context.Channel.Id))
                {
                    return;
                }

                string UserId = context.User.Id.ToString();
                if (guild.Levels.ContainsKey(UserId))
                {
                    guild.Levels[UserId].Xp++;
                    if (guild.Levels[UserId].Xp >= Utility.GetLevelXp(guild.Levels[UserId].Level))
                    {
                        guild.Levels[UserId].Level++;
                        guild.Levels[UserId].Xp = 0;

                        string lv = guild.Levels[UserId].Level.ToString();

                        if (guild.Setting.LevelUpChannelId.HasValue && context.Guild.Channels.Any(c => c.Id == guild.Setting.LevelUpChannelId.Value))
                        {
                            SocketTextChannel c = context.Guild.GetTextChannel(guild.Setting.LevelUpChannelId.Value);

                            await c.SendMessageAsync($"{context.User.Mention}님, {lv}레벨이 되신걸 축하해요! :tada:");
                        }
                        else
                        {
                            await context.ReplyEmbedAsync($"{context.User.Mention}님, {lv}레벨이 되신걸 축하해요! :tada:", disalbeMention: false);
                        }

                        if (guild.Setting.LevelRoles.ContainsKey(lv) && context.Guild.Roles.Any(r => r.Id == guild.Setting.LevelRoles[lv]))
                        {
                            await (context.User as SocketGuildUser).AddRoleAsync(context.Guild.GetRole(guild.Setting.LevelRoles[lv]));
                        }
                    }
                }
                else
                {
                    guild.Levels.Add(UserId, new OliveGuild.UserLevel());
                    guild.Levels[UserId].Xp++;
                }

                OliveGuild.Set(context.Guild.Id, g => g.Levels, guild.Levels);
                #endregion
            }).Start();

            await Task.CompletedTask;
        }

        public static async Task OnInteractionCreated(SocketInteraction arg)
        {
            new Task(async () =>
            {
                SocketMessageComponent component = arg as SocketMessageComponent;
                string[] args = component.Data.CustomId.Split('.');

                ulong userId = ulong.Parse(args[0]);
                InteractionType type = (InteractionType)int.Parse(args[1]);

                if (component.User.Id != userId) return;

                switch (type)
                {
                    case InteractionType.CreateCommand:
                        CommandCreateSession.ResponseType response = (CommandCreateSession.ResponseType)int.Parse(args[2]);
                        await CommandCreateSession.ButtonResponse(userId, response);

                        break;
                    case InteractionType.CancelTypingGame:
                        if (!TypingSession.Sessions.ContainsKey(userId))
                        {
                            break;
                        }

                        SocketCommandContext context = TypingSession.Sessions[userId].Context;

                        TypingSession.Sessions.Remove(userId);
                        await context.ReplyEmbedAsync("게임이 취소됐어요");

                        break;
                    case InteractionType.CancelWordGame:
                        if (!WordSession.Sessions.ContainsKey(userId))
                        {
                            break;
                        }

                        context = WordSession.Sessions[userId].Context;

                        WordSession.Sessions.Remove(userId);
                        await context.ReplyEmbedAsync("게임이 취소됐어요");

                        break;
                    case InteractionType.CommandList:
                        SocketGuild guild = Program.Client.GetGuild(ulong.Parse(args[2]));
                        int page = int.Parse(args[3]);

                        await Command.ChangeListPage(guild, userId, component.Message, page);

                        break;
                    case InteractionType.CommandAnswerList:
                        guild = Program.Client.GetGuild(ulong.Parse(args[2]));
                        string command = args[3];
                        page = int.Parse(args[4]);

                        await Command.ChangeAnswerListPage(guild, userId, component.Message, command, page);

                        break;
                    case InteractionType.DeleteCommand:
                        if (!CommandDeleteSession.Sessions.ContainsKey(userId))
                        {
                            break;
                        }

                        context = CommandDeleteSession.Sessions[userId].Context;

                        CommandDeleteSession.ResponseType deleteResponse = (CommandDeleteSession.ResponseType)int.Parse(args[2]);

                        var commands = OliveGuild.Get(context.Guild.Id).Commands;

                        if (deleteResponse == CommandDeleteSession.ResponseType.Cancel)
                        {
                            CommandDeleteSession.Sessions.Remove(userId);

                            await context.ReplyEmbedAsync("커맨드 삭제를 취소했어요");

                            break;
                        }
                        else if (deleteResponse == CommandDeleteSession.ResponseType.DeleteSingleAnswer)
                        {
                            command = commands.Keys.ToList()[CommandDeleteSession.Sessions[userId].CommandIndex];
                            string answer = commands[command][CommandDeleteSession.Sessions[userId].AnswerIndex].Answer;

                            commands[command].RemoveAt(CommandDeleteSession.Sessions[userId].AnswerIndex);
                            if (!commands[command].Any())
                            {
                                commands.Remove(command);
                            }
                            OliveGuild.Set(context.Guild.Id, g => g.Commands, commands);

                            await context.ReplyEmbedAsync($"`{command}` 커맨드의 응답 `{answer.을를("`")} 삭제했어요");
                        }
                        else if (deleteResponse == CommandDeleteSession.ResponseType.DeleteAnswers)
                        {
                            commands[CommandDeleteSession.Sessions[userId].Command].RemoveAll(c => c.Answer == CommandDeleteSession.Sessions[userId].Answer);
                            if (!commands[CommandDeleteSession.Sessions[userId].Command].Any())
                            {
                                commands.Remove(CommandDeleteSession.Sessions[userId].Command);
                            }

                            OliveGuild.Set(context.Guild.Id, g => g.Commands, commands);

                            await context.ReplyEmbedAsync($"`{CommandDeleteSession.Sessions[userId].Command}` 커맨드의 응답 `{CommandDeleteSession.Sessions[userId].Answer.을를("`")} 삭제했어요");
                        }
                        else if (deleteResponse == CommandDeleteSession.ResponseType.DeleteCommand)
                        {
                            commands.Remove(CommandDeleteSession.Sessions[userId].Command);
                            OliveGuild.Set(context.Guild.Id, g => g.Commands, commands);

                            await context.ReplyEmbedAsync($"`{CommandDeleteSession.Sessions[userId].Command}` 커맨드를 삭제했어요");
                        }

                        CommandDeleteSession.Sessions.Remove(userId);

                        break;
                }
            }).Start();

            await Task.CompletedTask;
        }

        public static async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            new Task(async () =>
            {
                await CustomCommandExecutor.OnReactionAdded(reaction);
            }).Start();

            await Task.CompletedTask;
        }

        public static async Task OnCommandExecuted(Discord.Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                var ctx = context as SocketCommandContext;

                if (Program.IsDebugMode)
                {
                    EmbedBuilder emb = ctx.CreateEmbed(title: "오류 발생!", description: $"{result.Error}: {result.ErrorReason}");
                    await ctx.ReplyEmbedAsync(emb.Build());
                }
                else
                {
                    await context.Message.AddReactionAsync(new Emoji("⚠️"));
                }
            }
            else
            {
                if (new Random().Next(0, 15) == 0 && await KoreanBots.IsNotVotedAsync(context.User.Id))
                {
                    await (context as SocketCommandContext).ReplyEmbedAsync("[KOREANBOTS](https://koreanbots.dev/bots/495209098929766400)에서 올리브토스트에게 하트를 추가해주세요!\n(하트는 12시간마다 한번씩 추가할 수 있어요)");
                }
            }
        }
    }
}