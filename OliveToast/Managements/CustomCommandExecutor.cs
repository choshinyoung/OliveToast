using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
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
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, ulong>("id", (ctx, user) => user.Id));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, int>("tag", (ctx, user) => user.DiscriminatorValue));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, string>("nickname", (ctx, user) => user.GetName(false)));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, bool>("isBot", (ctx, user) => user.IsBot));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, string>("mention", (ctx, user) => user.Mention));

            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, int, string>("group", (ctx, x) => ctx.Groups[x]));
            toaster.AddCommand(ToastCommand.CreateAction<CustomCommandContext, int>("wait", (ctx, x) =>
            {
                if (ctx.HowLongWaited + x is var a && (a > 10 * 60 * 1000 || a < 0))
                {
                    throw new Exception("대기 시간이 너무 길어요!");
                }

                ctx.HowLongWaited += x;
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

            return toaster;
        }

        public static async Task Execute(SocketCommandContext context, OliveGuild guild)
        {
            var commands = guild.Commands;

            List<(string[] groups, OliveGuild.CustomCommand command)> answers = new();

            if (commands.ContainsKey(context.Message.Content) && commands[context.Message.Content].Any(c => !c.IsRegex))
            {
                answers.AddRange(commands[context.Message.Content].Where(c => !c.IsRegex).Select(c => (new[] { context.Message.Content }, c)));
            }
            foreach (var command in commands)
            {
                if (!command.Value.Any(a => a.IsRegex))
                {
                    continue;
                }

                Match match = new Regex(command.Key).Match(context.Message.Content);

                if (!match.Success || match.Value.Length != context.Message.Content.Length)
                {
                    continue;
                }

                answers.AddRange(command.Value.Where(a => a.IsRegex).Select(c => (match.Groups.Values.Select(v => v.Value).ToArray(), c)));
            }

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
