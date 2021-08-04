using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OliveToast.Managements
{
    class CustomCommandExecutor
    {
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
            }
            else
            {
                await context.Message.AddReactionAsync(new Emoji("🚫"));
            }
        }
    }
}
