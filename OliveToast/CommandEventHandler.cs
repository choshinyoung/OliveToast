﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using OliveToast.Commands;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OliveToast
{
    class CommandEventHandler
    {
        public enum InteractionType
        {
            CreateCommand
        }

        public static readonly string prefix = ConfigManager.Get("PREFIX");

        public static void RegisterEvents(DiscordSocketClient client, CommandService command)
        {
            client.Log += OnLog;
            command.Log += OnCommandLog;

            client.MessageReceived += OnMessageReceived;
            command.CommandExecuted += OnCommandExecuted;

            client.InteractionCreated += OnInteractionCreated;

            client.JoinedGuild += OnJoinGuild;
            client.LeftGuild += OnLeftGuild;

            client.Ready += OnReady;
        }

        private static async Task OnInteractionCreated(SocketInteraction arg)
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
            }
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

        private static async Task OnJoinGuild(SocketGuild guild)
        {
            await KoreanBots.UpdateServerCountAsync(Program.Client.Guilds.Count);
        }

        private static async Task OnLeftGuild(SocketGuild arg)
        {
            await KoreanBots.UpdateServerCountAsync(Program.Client.Guilds.Count);
        }

        private static async Task OnReady()
        {
            await KoreanBots.UpdateServerCountAsync(Program.Client.Guilds.Count);
        }

        public static async Task OnMessageReceived(SocketMessage msg)
        {
            if (msg is not SocketUserMessage userMsg || userMsg.Content == null ||
                userMsg.Author.Id == Program.Client.CurrentUser.Id || userMsg.Author.IsBot) return;

            SocketCommandContext context = new(Program.Client, userMsg);

            if (await Games.WordRelay(context) || await Games.TypingGame(context) || await CommandCreateSession.MessageResponse(context.User.Id, context.Message.Content))
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

            #region custom command
            var commands = guild.Commands;
            if (commands.ContainsKey(context.Message.Content))
            {
                if (CommandRateLimit.AddCount(context.User.Id))
                {
                    OliveGuild.CustomCommand command = commands[context.Message.Content][new Random().Next(commands[context.Message.Content].Count)];

                    await context.MsgReplyAsync(command.Answer);
                }
                else
                {
                    await context.Message.AddReactionAsync(new Emoji("🚫"));
                }
            }
            #endregion

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
                        await context.MsgReplyEmbedAsync($"{context.User.Mention}님, {lv}레벨이 되신걸 축하해요! :tada:", disalbeMention: false);
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
        }

        public static async Task OnCommandExecuted(Discord.Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                var ctx = context as SocketCommandContext;

                if (Program.IsDebugMode)
                {
                    EmbedBuilder emb = ctx.CreateEmbed(title: "오류 발생!", description: $"{result.Error}: {result.ErrorReason}");
                    await ctx.MsgReplyEmbedAsync(emb.Build());
                }
                else
                {
                    await context.Message.AddReactionAsync(new Emoji("⚠️"));
                }
            }
            else
            {
                if (new Random().Next(0, 10) == 0 && await KoreanBots.IsVotedAsync(context.User.Id))
                {
                    await (context as SocketCommandContext).MsgReplyEmbedAsync("[KOREANBOTS](https://koreanbots.dev/bots/495209098929766400)에서 올리브토스트에게 하트를 추가해주세요!\n(하트는 12시간마다 한번씩 추가할 수 있어요)");
                }
            }
        }
    }
}