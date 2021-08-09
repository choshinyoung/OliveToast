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

namespace OliveToast.Managements
{
    class CustomCommandExecutor
    {
        public static Toaster GetToaster()
        {
            Toaster toaster = new();

            toaster.AddCommand(BasicCommands.Literals);
            toaster.AddCommand(BasicCommands.Operators);
            toaster.AddCommand(BasicCommands.Strings);
            toaster.AddCommand(BasicCommands.Lists);
            toaster.AddCommand(BasicCommands.If, BasicCommands.Else, BasicCommands.Foreach, BasicCommands.Assign, BasicCommands.Random, BasicCommands.RandomChoice);

            toaster.AddConverter(BasicConverters.All);

            toaster.AddCommand(ToastCommand.CreateAction<CustomCommandContext, string>("send", (ctx, x) =>
            {
                if (ctx.SendCount >= 5)
                {
                    throw new Exception("메시지를 너무 많이 보내고있어요!");
                }

                ctx.DiscordContext.Channel.SendMessageAsync(x, allowedMentions: AllowedMentions.None).Wait();
                ctx.SendCount++;
            }, -1));
            toaster.AddCommand(ToastCommand.CreateAction<CustomCommandContext, string>("reply", (ctx, x) =>
            {
                if (ctx.SendCount >= 5)
                {
                    throw new Exception("메시지를 너무 많이 보내고있어요!");
                }

                ctx.DiscordContext.MsgReplyAsync(x).Wait();
                ctx.SendCount++;
            }, -1));
            toaster.AddCommand(ToastCommand.CreateAction<CustomCommandContext>("delete", (ctx) => ctx.DiscordContext.Message.DeleteAsync().Wait()));
            toaster.AddCommand(ToastCommand.CreateAction<CustomCommandContext, string>("react", (ctx, x) 
                => ctx.DiscordContext.Message.AddReactionAsync(Emote.TryParse(x, out var result) ? result : new Emoji(x)), -1));

            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, object[]>("users", (ctx) => ctx.DiscordContext.Guild.Users.Select(u => (object)u).ToArray()));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser>("user", (ctx) => ctx.DiscordContext.User as SocketGuildUser));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, string>("username", (ctx, user) => user.Username));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, ulong>("userId", (ctx, user) => user.Id));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, int>("tag", (ctx, user) => user.DiscriminatorValue));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, string>("nickname", (ctx, user) => user.GetName(false)));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, bool>("isBot", (ctx, user) => user.IsBot));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, string>("userMention", (ctx, user) => user.Mention));
            toaster.AddCommand(ToastCommand.CreateAction<CustomCommandContext, SocketGuildUser>("kick", (ctx, user) => user.KickAsync().Wait()));
            toaster.AddCommand(ToastCommand.CreateAction<CustomCommandContext, SocketGuildUser>("ban", (ctx, user) => user.BanAsync().Wait()));

            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, object[]>("channels", (ctx) => ctx.DiscordContext.Guild.TextChannels.Select(u => (object)u).ToArray()));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel>("channel", (ctx) => ctx.DiscordContext.Channel as SocketTextChannel));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel, string>("channelName", (ctx, channel) => channel.Name));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel, ulong>("channelId", (ctx, channel) => channel.Id));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel, string>("category", (ctx, channel) => channel.Category is not null ? channel.Category.Name : "-"));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel, bool>("isNsfw", (ctx, channel) => channel.IsNsfw));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel, string>("channelMention", (ctx, channel) => channel.Mention));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel, int>("slowMode", (ctx, channel) => channel.SlowModeInterval));

            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, object[]>("roles", (ctx) => ctx.DiscordContext.Guild.TextChannels.Select(u => (object)u).ToArray()));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketRole, string>("roleName", (ctx, role) => role.Name));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketRole, ulong>("roleId", (ctx, role) => role.Id));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketRole, bool>("isHoisted", (ctx, role) => role.IsHoisted));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketRole, bool>("isMentionable", (ctx, role) => role.IsMentionable));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketRole, string>("roleMention", (ctx, role) => role.Mention));

            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, string>("serverName", (ctx) => ctx.DiscordContext.Guild.Name));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, ulong>("serverId", (ctx) => ctx.DiscordContext.Guild.Id));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel>("systemChannel", (ctx) => ctx.DiscordContext.Guild.SystemChannel));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel>("ruleChannel", (ctx) => ctx.DiscordContext.Guild.RulesChannel));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel>("publicUpdateChannel", (ctx) => ctx.DiscordContext.Guild.PublicUpdatesChannel));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser>("owner", (ctx) => ctx.DiscordContext.Guild.Owner));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, int>("boostCount", (ctx) => ctx.DiscordContext.Guild.PremiumSubscriptionCount));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, int>("boostLevel", (ctx) => (int)ctx.DiscordContext.Guild.PremiumTier));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, bool>("isMfaEnabled", (ctx) => ctx.DiscordContext.Guild.MfaLevel == MfaLevel.Enabled));

            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, int, string>("group", (ctx, x) => ctx.Groups[x]));
            toaster.AddCommand(ToastCommand.CreateAction<CustomCommandContext, int>("wait", (ctx, x) =>
            {
                if ((ctx.HowLongWaited + x) is var a && (a > 600 || a < 0))
                {
                    throw new Exception("대기 시간이 너무 길어요!");
                }

                ctx.HowLongWaited = a;
                Console.WriteLine(ctx.HowLongWaited);
                Task.Delay(x * 1000).Wait();
            }));

            toaster.AddConverter(ToastConverter.Create<string, SocketGuildUser>((_ctx, x) =>
            {
                CustomCommandContext ctx = (CustomCommandContext)_ctx;

                var user = ctx.DiscordContext.Guild.Users.ToList().Find(u => u.Username == x || (u.Nickname is not null && u.Nickname == x));
                if (user is not null)
                {
                    return user;
                }

                Match match = new Regex("<@!([0-9]+)>").Match(x);
                if (match.Success)
                {
                    return ctx.DiscordContext.Guild.GetUser(ulong.Parse(match.Groups[1].Value));
                }

                if (x.All(c => char.IsDigit(c)))
                {
                    return ctx.DiscordContext.Guild.GetUser(ulong.Parse(x));
                }

                return null;
            }));
            toaster.AddConverter(ToastConverter.Create<ulong, SocketGuildUser>((_ctx, x) =>
            {
                CustomCommandContext ctx = (CustomCommandContext)_ctx;

                return ctx.DiscordContext.Guild.GetUser(x);
            }));

            toaster.AddConverter(ToastConverter.Create<string, SocketTextChannel>((_ctx, x) =>
            {
                CustomCommandContext ctx = (CustomCommandContext)_ctx;

                var channel = ctx.DiscordContext.Guild.TextChannels.ToList().Find(u => u.Name == x);
                if (channel is not null)
                {
                    return channel;
                }

                Match match = new Regex("<#([0-9]+)>").Match(x);
                if (match.Success)
                {
                    return ctx.DiscordContext.Guild.GetTextChannel(ulong.Parse(match.Groups[1].Value));
                }

                if (x.All(c => char.IsDigit(c)))
                {
                    return ctx.DiscordContext.Guild.GetTextChannel(ulong.Parse(x));
                }

                return null;
            }));
            toaster.AddConverter(ToastConverter.Create<ulong, SocketTextChannel>((_ctx, x) =>
            {
                CustomCommandContext ctx = (CustomCommandContext)_ctx;

                return ctx.DiscordContext.Guild.GetTextChannel(x);
            }));

            toaster.AddConverter(ToastConverter.Create<string, SocketRole>((_ctx, x) =>
            {
                CustomCommandContext ctx = (CustomCommandContext)_ctx;

                var channel = ctx.DiscordContext.Guild.Roles.ToList().Find(u => u.Name.ToLower() == x.ToLower());
                if (channel is not null)
                {
                    return channel;
                }

                Match match = new Regex("<#([0-9]+)>").Match(x);
                if (match.Success)
                {
                    return ctx.DiscordContext.Guild.GetRole(ulong.Parse(match.Groups[1].Value));
                }

                if (x.All(c => char.IsDigit(c)))
                {
                    return ctx.DiscordContext.Guild.GetRole(ulong.Parse(x));
                }

                return null;
            }));
            toaster.AddConverter(ToastConverter.Create<ulong, SocketRole>((_ctx, x) =>
            {
                CustomCommandContext ctx = (CustomCommandContext)_ctx;

                return ctx.DiscordContext.Guild.GetRole(x);
            }));

            return toaster;
        }

        public static async Task Execute(SocketCommandContext context, OliveGuild guild)
        {
            List<(string[] groups, OliveGuild.CustomCommand command)> answers = FindAnswers(guild, context.Message.Content);
            if (!answers.Any())
            {
                return;
            }

            if (CommandRateLimit.AddCount(context.User.Id))
            {
                var command = answers[new Random().Next(answers.Count)];

                if (command.command.Answer is not null)
                {
                    await context.MsgReplyAsync(command.command.Answer);
                }

                Toaster toaster = GetToaster();
                CustomCommandContext toastContext = new(context, command.groups);

                foreach (string line in command.command.ToastLines)
                {
                    try
                    {
                        toaster.Execute(line, toastContext);
                    }
                    catch (Exception e)
                    {
                        EmbedBuilder emb = context.CreateEmbed(title: "오류 발생!", description: e.Message);
                        await context.MsgReplyEmbedAsync(emb.Build());

                        return;
                    }
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

                answers.Add(cmd.Key);
            }

            return answers;
        }
    }

    class CustomCommandContext : ToastContext
    {
        public readonly SocketCommandContext DiscordContext;
        public readonly string[] Groups;

        public int SendCount;
        public int HowLongWaited;

        public CustomCommandContext(SocketCommandContext context, string[] groups)
        {
            DiscordContext = context;
            Groups = groups;

            SendCount = 0;
            HowLongWaited = 0;
        }
    }
}
