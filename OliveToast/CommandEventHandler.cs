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
            await Task.Factory.StartNew(async () =>
            {
                SocketGuild guild = arg.Guild;

                OliveGuild.GuildSetting setting = OliveGuild.Get(guild.Id).Setting;

                string joinMessage = setting.JoinMessage;
                List<string> toastLines = setting.JoinMessageToastLines;

                Toaster toaster = CustomCommandExecutor.GetToaster();
                CustomCommandContext context = new(arg.Guild, guild.SystemChannel, null, arg, arg.Guild.OwnerId, Array.Empty<string>(), true, true, true);
                string output = (string)toaster.Execute($"\"{joinMessage}\"", context);

                if (output is not null and not "")
                {
                    if (guild.SystemChannel is not null)
                    {
                        if (guild.GetUser(Program.Client.CurrentUser.Id).GetPermissions(guild.SystemChannel).SendMessages)
                        {
                            await guild.SystemChannel.SendMessageAsync(output);
                        }
                    }
                }

                foreach (string line in toastLines)
                {
                    toaster.Execute(line, context);
                }
            });
        }

        public static async Task OnUserLeft(SocketGuildUser arg)
        {
            await Task.Factory.StartNew(async () =>
            {
                SocketGuild guild = arg.Guild;

                OliveGuild.GuildSetting setting = OliveGuild.Get(guild.Id).Setting;

                string leaveMessage = setting.LeaveMessage;
                List<string> toastLines = setting.LeaveMessageToastLines;

                Toaster toaster = CustomCommandExecutor.GetToaster();
                CustomCommandContext context = new(arg.Guild, guild.SystemChannel, null, arg, arg.Guild.OwnerId, Array.Empty<string>(), true, true, true);
                string output = (string)toaster.Execute($"\"{leaveMessage}\"", context);

                if (output is not null and not "")
                {
                    if (guild.SystemChannel is not null)
                    {
                        if (guild.GetUser(Program.Client.CurrentUser.Id).GetPermissions(guild.SystemChannel).SendMessages)
                        {
                            await guild.SystemChannel.SendMessageAsync(output);
                        }
                    }
                }

                foreach (string line in toastLines)
                {
                    toaster.Execute(line, context);
                }
            });
        }

        public static async Task OnMessageReceived(SocketMessage msg)
        {
            await Task.Factory.StartNew(async () =>
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

                await Task.Factory.StartNew(async () => await CustomCommandExecutor.Execute(context, guild));

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
            });
        }

        public static async Task OnInteractionCreated(SocketInteraction arg)
        {
            await Task.Factory.StartNew(async () =>
            {
                await InteractionHandler.OnInteractionCreated(arg);
            });
        }

        public static async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            await Task.Factory.StartNew(async () =>
            {
                if ((reaction.Channel as SocketTextChannel).Guild.GetUser(reaction.UserId).IsBot) return;

                await CustomCommandExecutor.OnReactionAdded(reaction);
            });
        }

        public static async Task OnCommandExecuted(Discord.Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            await Task.Factory.StartNew(async () =>
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
                        IEmote emote = new Emoji("⚠️");

                        if (result.ErrorReason.StartsWith(RequirePermissionException.Emoji))
                        {
                            emote = Emote.Parse(RequirePermissionException.Emoji);
                        }
                        else if (result.ErrorReason.StartsWith(CategoryNotEnabledException.Emoji))
                        {
                            emote = Emote.Parse(CategoryNotEnabledException.Emoji);
                        }

                        await context.Message.AddReactionAsync(emote);
                    }
                }
                else
                {
                    if (Admin.CommandStats.ContainsKey(command.Value.Name))
                    {
                        Admin.CommandStats[command.Value.Name].Add((context.User.Id, context.Guild.Id));
                    }
                    else
                    {
                        Admin.CommandStats.Add(command.Value.Name, new() { (context.User.Id, context.Guild.Id) });
                    }

                    if (new Random().Next(0, 15) == 0 && await KoreanBots.IsNotVotedAsync(context.User.Id))
                    {
                        await (context as SocketCommandContext).ReplyEmbedAsync("[KOREANBOTS](https://koreanbots.dev/bots/495209098929766400)에서 올리브토스트에게 하트를 추가해주세요!\n(하트는 12시간마다 한번씩 추가할 수 있어요)");
                    }
                }
            });
        }
    }
}