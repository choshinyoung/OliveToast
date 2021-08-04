using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            toaster.AddCommand(BasicCommands.Operators);
            toaster.AddCommand(BasicCommands.Strings);
            toaster.AddCommand(BasicCommands.Lists);
            toaster.AddCommand(BasicCommands.If, BasicCommands.Else, BasicCommands.Assign);

            toaster.AddCommand(ToastCommand.CreateAction<CustomCommandContext, string>("send", (ctx, x) =>
            {
                ctx.DiscordContext.Channel.SendMessageAsync(x).Wait();
            }));
            toaster.AddCommand(ToastCommand.CreateFunc<CustomCommandContext, int, string>("group", (ctx, x) => ctx.Groups[x]));

            toaster.AddConverter(BasicConverters.All);

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

                if (!match.Success)
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

                foreach (string line in command.command.RawToastLines)
                {
                    ExecuteToastCommand(line, context, command.groups);
                }
            }
            else
            {
                await context.Message.AddReactionAsync(new Emoji("🚫"));
            }
        }

        public static object ExecuteToastCommand(string line, SocketCommandContext context, string[] groups)
        {
            return GetToaster().Execute(line, new CustomCommandContext(context, groups));
        }
    }

    class CustomCommandContext : ToastContext
    {
        public readonly SocketCommandContext DiscordContext;
        public readonly string[] Groups;

        public CustomCommandContext(SocketCommandContext context, string[] groups)
        {
            DiscordContext = context;
            Groups = groups;
        }
    }
}
