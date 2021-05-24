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
        public static readonly string prefix = ConfigManager.Get("PREFIX");

        public static void RegisterEvents(DiscordSocketClient client, CommandService command)
        {
            client.Log += OnLog;
            command.Log += OnCommandLog;

            client.MessageReceived += OnMessageReceived;
            command.CommandExecuted += OnCommandExecuted;

            client.GuildAvailable += OnGuildAvailable;
            client.JoinedGuild += OnJoinGuild;
            client.LeftGuild += OnLeftGuild;

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

        private static async Task OnGuildAvailable(SocketGuild guild)
        {
            if (DbManager.Guilds.Find(g => g.GuildId == guild.Id).Any())
            {
                return;
            }

            DbManager.Guilds.InsertOne(new OliveGuild(guild.Id));

            await Task.CompletedTask;
        }

        private static async Task OnJoinGuild(SocketGuild guild)
        {
            await OnGuildAvailable(guild);

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

            SocketCommandContext context = new SocketCommandContext(Program.Client, userMsg);

            if (await Games.WordRelay(context))
            {
                return;
            }
            if (await Games.TypingGame(context))
            {
                return;
            }

            int argPos = 0;
            if (userMsg.HasStringPrefix(prefix, ref argPos) || userMsg.HasMentionPrefix(Program.Client.CurrentUser, ref argPos))
            {
                await Program.Command.ExecuteAsync(context, argPos, Program.Service);
            }

            if (context.IsPrivate) 
            {
                return;
            }

            OliveGuild guild = OliveGuild.Get(context.Guild.Id);

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

                    if (guild.Setting.LevelUpChannelId.HasValue && context.Guild.Channels.Any(c => c.Id == guild.Setting.LevelUpChannelId.Value))
                    {
                        SocketTextChannel c = context.Guild.GetTextChannel(guild.Setting.LevelUpChannelId.Value);

                        string lv = guild.Levels[UserId].Level.ToString();

                        await c.SendMessageAsync($"{context.User.Mention}님, {lv}레벨이 되신걸 축하해요! :tada:");

                        if (guild.Setting.LevelRoles.ContainsKey(lv) && context.Guild.Roles.Any(r => r.Id == guild.Setting.LevelRoles[lv]))
                        {
                            await (context.User as SocketGuildUser).AddRoleAsync(context.Guild.GetRole(guild.Setting.LevelRoles[lv]));
                        }
                    }
                    else
                    {
                        await context.MsgReplyEmbedAsync($"{context.User.Mention}님, {guild.Levels[UserId].Level}레벨이 되신걸 축하해요! :tada:", disalbeMention: false);
                    }
                }

            }
            else
            {
                guild.Levels.Add(UserId, new OliveGuild.UserLevel());
                guild.Levels[UserId].Xp++;
            }

            OliveGuild.Set(context.Guild.Id, g => g.Levels, guild.Levels);
        }

        public static async Task OnCommandExecuted(Discord.Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                if (result.Error == CommandError.UnknownCommand)
                {
                    return;
                }

                var ctx = context as SocketCommandContext;

                EmbedBuilder emb = ctx.CreateEmbed(title: "오류 발생!", description: $"{result.Error}: {result.ErrorReason}");

                await ctx.MsgReplyEmbedAsync(emb.Build());
            }
            else
            {
                if (new Random().Next(0, 5) == 0 && !await KoreanBots.IsVotedAsync(context.User.Id))
                {
                    await (context as SocketCommandContext).MsgReplyEmbedAsync("아직 디코봇 웹사이트에서 올리브토스트에게 하트를 주지 않았습니다.\n[이곳](https://koreanbots.dev/bots/495209098929766400)에서 올리브토스트에게 투표해주세요!");
                }
            }
        }
    }
}
