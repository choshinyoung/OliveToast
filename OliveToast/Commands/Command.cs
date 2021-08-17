using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toast;
using static OliveToast.Managements.RequireCategoryEnable;
using static OliveToast.Managements.RequirePermission;

namespace OliveToast.Commands
{
    [Name("커맨드")]
    [RequireCategoryEnable(CategoryType.Command), RequireContext(ContextType.Guild)]
    public class Command : ModuleBase<SocketCommandContext>
    {
        public const int MaxListCommandCount = 15;

        [Command("커맨드 만들기"), Alias("커맨드 생성")]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("커스텀 커맨드를 생성합니다")]
        public async Task CreateCommand([Name("커맨드")] string command, [Name("응답"), Remainder] string answer)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (commands.ContainsKey(command))
            {
                commands[command].Add(new(answer, false, new(), Context.User.Id, (Context.User as SocketGuildUser).GuildPermissions));
            }
            else
            {
                commands.Add(command, new() { new(answer, false, new(), Context.User.Id, (Context.User as SocketGuildUser).GuildPermissions) });
            }
            OliveGuild.Set(Context.Guild.Id, g => g.Commands, commands);

            EmbedBuilder emb = Context.CreateEmbed(title: "커맨드 생성 완료");
            emb.AddField("커맨드", command, true);
            emb.AddField("응답", answer, true);
            emb.AddField("정규식 사용 여부", false.ToEmoji(), true);

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("커맨드 만들기"), Alias("커맨드 생성")]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("고급 설정을 사용해 커맨드를 생성합니다")]
        public async Task CreatCommand()
        {
            if (CommandCreateSession.Sessions.ContainsKey(Context.User.Id))
            {
                await Context.ReplyEmbedAsync("이미 커맨드를 만드는 중이에요");

                return;
            }

            OliveGuild.CustomCommand command = new(null, false, new(), Context.User.Id, (Context.User as SocketGuildUser).GuildPermissions);

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("정규식으로 변경", $"{Context.User.Id}.{(int)CommandEventHandler.InteractionType.CreateCommand}.{(int)CommandCreateSession.ResponseType.ChangeRegex}")
                .WithButton("취소", $"{Context.User.Id}.{(int)CommandEventHandler.InteractionType.CreateCommand}.{(int)CommandCreateSession.ResponseType.Cancel}", ButtonStyle.Danger);

            RestUserMessage msg = await Context.ReplyEmbedAsync("커맨드를 입력해주세요", component: component.Build());

            CommandCreateSession.Sessions.Add(Context.User.Id, new()
            {
                SessionStatus = CommandCreateSession.Status.CommandInput,
                CustomCommand = command,
                Message = msg,
                UserMessageContext = Context,
                LastActiveTime = DateTime.Now
            });
        }

        [Command("커맨드 삭제"), Alias("커맨드 제거"), Priority(1)]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("커스텀 커맨드를 제거합니다")]
        public async Task DeleteCommandByIndex([Name("번호")] int index)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (commands.Count <= index)
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            string target = commands.ElementAt(index).Key;
            commands.Remove(target);
            OliveGuild.Set(Context.Guild.Id, g => g.Commands, commands);
            
            await Context.ReplyEmbedAsync($"`{target}` 커맨드를 삭제했어요");
        }

        [Command("커맨드 삭제"), Alias("커맨드 제거"), Priority(-1)]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("커스텀 커맨드를 제거합니다")]
        public async Task DeleteCommand([Name("커맨드"), Remainder] string command)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (!commands.ContainsKey(command))
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            commands.Remove(command);
            OliveGuild.Set(Context.Guild.Id, g => g.Commands, commands);
            
            await Context.ReplyEmbedAsync($"`{command}` 커맨드를 삭제했어요");
        }

        [Command("커맨드 삭제"), Alias("커맨드 제거")]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("커스텀 커맨드를 제거합니다")]
        public async Task DeleteCommand([Name("번호"), Remainder] int index)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (commands.Count <= index)
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            string command = commands.Keys.ToList()[index];

            await DeleteCommand(command);
        }

        [Command("커맨드 삭제"), Alias("커맨드 제거")]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("커스텀 커맨드를 제거합니다")]
        public async Task DeleteCommand([Name("커맨드")] string command, [Name("응답"), Remainder] string answer)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (!commands.ContainsKey(command) || !commands[command].Any(c => c.Answer == answer))
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            commands[command].RemoveAll(c => c.Answer == answer);
            if (!commands[command].Any())
            {
                commands.Remove(command);
            }

            OliveGuild.Set(Context.Guild.Id, g => g.Commands, commands);

            await Context.ReplyEmbedAsync($"`{command}` 커맨드의 응답 `{answer.을를("`")} 삭제했어요");
        }

        [Command("커맨드 삭제"), Alias("커맨드 제거")]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("커스텀 커맨드를 제거합니다")]
        public async Task DeleteCommand([Name("커맨드 번호")] int cIndex, [Name("응답"), Remainder] string answer)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (commands.Count <= cIndex)
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            string command = commands.Keys.ToList()[cIndex];

            if (!commands[command].Any(c => c.Answer == answer))
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            await DeleteCommand(command, answer);
        }

        [Command("커맨드 삭제"), Alias("커맨드 제거")]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("커스텀 커맨드를 제거합니다")]
        public async Task DeleteCommand([Name("커맨드")] string command, [Name("응답 번호"), Remainder] int aIndex)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (!commands.ContainsKey(command) || commands[command].Count <= aIndex)
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            await DeleteCommand(commands.Keys.ToList().IndexOf(command), aIndex);
        }

        [Command("커맨드 삭제"), Alias("커맨드 제거")]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("커스텀 커맨드를 제거합니다")]
        public async Task DeleteCommand([Name("커맨드 번호")] int cIndex, [Name("응답 번호"), Remainder] int aIndex)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (commands.Count <= cIndex)
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            string command = commands.Keys.ToList()[cIndex];

            if (commands[command].Count <= aIndex)
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            string answer = commands[command][aIndex].Answer;

            commands[command].RemoveAt(aIndex);
            if (!commands[command].Any())
            {
                commands.Remove(command);
            }
            OliveGuild.Set(Context.Guild.Id, g => g.Commands, commands);

            await Context.ReplyEmbedAsync($"`{command}` 커맨드의 응답 `{answer.을를("`")} 삭제했어요");
        }

        [Command("커맨드 목록")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 목록을 확인합니다")]
        public async Task CommandList()
        {
            List<string> commands = OliveGuild.Get(Context.Guild.Id).Commands.Keys.ToList();

            EmbedBuilder emb = Context.CreateEmbed(title: "커맨드 목록");

            if (commands.Count == 0)
            {
                emb.Description = "커맨드가 없어요";
            }

            int count = commands.Count;

            ComponentBuilder component = new();
            if (count > MaxListCommandCount)
            {
                count = MaxListCommandCount;
                component.WithButton("<", $"{Context.User.Id}.{(int)CommandEventHandler.InteractionType.CommandList}.{Context.Guild.Id}.{0}", disabled: true);
                component.WithButton($"1 / {Math.Ceiling(commands.Count / (float)MaxListCommandCount)}", $"{Context.User.Id}.{(int)CommandEventHandler.InteractionType.None}", ButtonStyle.Secondary);
                component.WithButton(">", $"{Context.User.Id}.{(int)CommandEventHandler.InteractionType.CommandList}.{Context.Guild.Id}.{2}");
            }

            for (int i = 0; i < count; i++)
            {
                emb.AddField(i.ToString(), commands[i], true);
            }

            await Context.ReplyEmbedAsync(emb.Build(), component: component.Build());
        }

        public static async Task ChangeListPage(SocketGuild guild, ulong userId, SocketUserMessage msg, int page)
        {
            List<string> commands = OliveGuild.Get(guild.Id).Commands.Keys.ToList();

            EmbedBuilder emb = msg.Embeds.First().ToEmbedBuilder();
            emb.Fields = new List<EmbedFieldBuilder>();

            int startIndex = MaxListCommandCount * (page - 1);

            for (int i = 0; i < MaxListCommandCount; i++)
            {
                int index = startIndex + i;

                if (commands.Count <= index) break;

                emb.AddField(index.ToString(), commands[index], true);
            }

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("<", $"{userId}.{(int)CommandEventHandler.InteractionType.CommandList}.{guild.Id}.{page - 1}", disabled: startIndex == 0)
                .WithButton($"{page} / {Math.Ceiling(commands.Count / (float)MaxListCommandCount)}", $"{userId}.{(int)CommandEventHandler.InteractionType.None}", ButtonStyle.Secondary)
                .WithButton(">", $"{userId}.{(int)CommandEventHandler.InteractionType.CommandList}.{guild.Id}.{page + 1}", disabled: startIndex + MaxListCommandCount >= commands.Count);

            await msg.ModifyAsync(m =>
            {
                m.Embeds = new[] { emb.Build() };
                m.Components = component.Build();
            });
        }

        [Command("커맨드 목록"), Alias("응답 목록")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 응답 목록을 확인합니다")]
        public async Task AnswerList([Name("커맨드"), Remainder] string command)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (!commands.ContainsKey(command))
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            await AnswerList(commands.Keys.ToList().IndexOf(command));
        }

        [Command("커맨드 목록"), Alias("응답 목록"), Priority(1)]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 응답 목록을 확인합니다")]
        public async Task AnswerList([Name("번호")] int index)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (commands.Count <= index)
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            string command = commands.Keys.ToList()[index];
            var answer = commands[command];

            int count = answer.Count;

            ComponentBuilder component = new();
            if (count > MaxListCommandCount)
            {
                count = MaxListCommandCount;
                component.WithButton("<", $"{Context.User.Id}.{(int)CommandEventHandler.InteractionType.CommandAnswerList}.{Context.Guild.Id}.{command}.{0}", disabled: true);
                component.WithButton($"1 / {Math.Ceiling(commands.Count / (float)MaxListCommandCount)}", $"{Context.User.Id}.{(int)CommandEventHandler.InteractionType.None}", ButtonStyle.Secondary);
                component.WithButton(">", $"{Context.User.Id}.{(int)CommandEventHandler.InteractionType.CommandAnswerList}.{Context.Guild.Id}.{command}.{2}");
            }

            EmbedBuilder emb = Context.CreateEmbed("", $"`{command}` 커맨드의 응답 목록");

            for (int i = 0; i < count; i++)
            {
                SocketGuildUser user = Context.Guild.Users.ToList().Find(u => u.Id == answer[i].CreatedBy);
                string username = user is null ? answer[i].CreatedBy.ToString() : $"{user.Username}#{user.Discriminator}";
                string isRegex = answer[i].IsRegex ? "- 정규식" : "";
                string toastCommands = answer[i].ToastLines.Any() ? $"\n커맨드 {answer[i].ToastLines.Count}줄" : "";

                emb.AddField($"{i} {isRegex} {toastCommands}", answer[i].Answer ?? "응답이 없어요", true);
            }

            await Context.ReplyEmbedAsync(emb.Build(), component: component.Build());
        }

        public static async Task ChangeAnswerListPage(SocketGuild guild, ulong userId, SocketUserMessage msg, string command, int page)
        {
            var answer = OliveGuild.Get(guild.Id).Commands[command];

            EmbedBuilder emb = msg.Embeds.First().ToEmbedBuilder();
            emb.Fields = new List<EmbedFieldBuilder>();

            int startIndex = MaxListCommandCount * (page - 1);

            for (int i = 0; i < MaxListCommandCount; i++)
            {
                int index = startIndex + i;

                if (answer.Count <= index) break;

                SocketGuildUser user = guild.Users.ToList().Find(u => u.Id == answer[index].CreatedBy);
                string username = user is null ? answer[index].CreatedBy.ToString() : $"{user.Username}#{user.Discriminator}";
                string isRegex = answer[index].IsRegex ? "- 정규식" : "";
                string toastCommands = answer[index].ToastLines.Any() ? $"\n커맨드 {answer[index].ToastLines.Count}줄" : "";

                emb.AddField($"{index} {isRegex} {toastCommands}", answer[index].Answer ?? "응답이 없어요", true);
            }

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("<", $"{userId}.{(int)CommandEventHandler.InteractionType.CommandAnswerList}.{guild.Id}.{page - 1}", disabled: startIndex == 0)
                .WithButton($"{page} / {Math.Ceiling(answer.Count / (float)MaxListCommandCount)}", $"{userId}.{(int)CommandEventHandler.InteractionType.None}", ButtonStyle.Secondary)
                .WithButton(">", $"{userId}.{(int)CommandEventHandler.InteractionType.CommandAnswerList}.{guild.Id}.{page + 1}", disabled: startIndex + MaxListCommandCount >= answer.Count);

            await msg.ModifyAsync(m =>
            {
                m.Embeds = new[] { emb.Build() };
                m.Components = component.Build();
            });
        }

        [Command("커맨드 정보"), Alias("응답 정보")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 응답의 정보를 확인합니다")]
        public async Task CommandInfo([Name("커맨드")] string command, [Name("응답"), Remainder] string answer)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (!commands.ContainsKey(command) || !commands[command].Any(c => c.Answer == answer))
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            await CommandInfo(commands.Keys.ToList().IndexOf(command), commands[command].IndexOf(commands[command].Find(c => c.Answer == answer)));
        }

        [Command("커맨드 정보"), Alias("응답 정보")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 응답의 정보를 확인합니다")]
        public async Task CommandInfo([Name("커맨드 번호")] int cIndex, [Name("응답"), Remainder] string answer)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (commands.Count <= cIndex)
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            string command = commands.Keys.ToList()[cIndex];

            if (!commands[command].Any(c => c.Answer == answer))
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            await CommandInfo(cIndex, commands[command].IndexOf(commands[command].Find(c => c.Answer == answer)));
        }

        [Command("커맨드 정보"), Alias("응답 정보")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 응답의 정보를 확인합니다")]
        public async Task CommandInfo([Name("커맨드")] string command, [Name("응답 번호")] int aIndex)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (!commands.ContainsKey(command) || commands[command].Count <= aIndex)
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            await CommandInfo(commands.Keys.ToList().IndexOf(command), aIndex);
        }

        [Command("커맨드 정보"), Alias("응답 목록"), Priority(1)]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 응답의 정보를 확인합니다")]
        public async Task CommandInfo([Name("커맨드 번호")] int cIndex, [Name("응답 번호")] int aIndex)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (commands.Count <= cIndex)
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            string command = commands.Keys.ToList()[cIndex];

            if (commands[command].Count <= aIndex)
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            var answer = commands[command][aIndex];

            EmbedBuilder emb = Context.CreateEmbed("", $"커맨드 정보");

            emb.AddField("커맨드", command, true);
            if (answer.Answer is not null)
            {
                emb.AddField("응답", answer.Answer, true);
            }
            emb.AddField("정규식 사용 여부", answer.IsRegex.ToEmoji(), true);

            if (answer.ToastLines.Count > 0)
            {
                emb.AddField("토스트 커맨드", string.Concat(answer.ToastLines.Select(l => $"```\n{l}\n```")));
            }

            SocketGuildUser user = Context.Guild.Users.ToList().Find(u => u.Id == answer.CreatedBy);
            emb.AddField("제작자", user is null ? answer.CreatedBy.ToString() : $"{user.Username}#{user.Discriminator}");

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("커맨드 검색")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("`메시지`를 보냈을 때 어떤 커맨드가 실행될지 확인할 수 있습니다")]
        public async Task SearchCommand([Name("메시지"), Remainder] string content)
        {
            EmbedBuilder emb = Context.CreateEmbed(title: "커맨드 검색");

            List<string> commands = CustomCommandExecutor.FindCommands(OliveGuild.Get(Context.Guild.Id), content);

            if (commands.Count == 0)
            {
                emb.Description = "커맨드가 없어요";
            }

            int count = commands.Count;

            ComponentBuilder component = new();
            if (count > MaxListCommandCount)
            {
                count = MaxListCommandCount;
                component.WithButton("<", $"{Context.User.Id}.{(int)CommandEventHandler.InteractionType.CommandSearch}.{Context.Guild.Id}.{0}", disabled: true);
                component.WithButton($"1 / {Math.Ceiling(commands.Count / (float)MaxListCommandCount)}", $"{Context.User.Id}.{(int)CommandEventHandler.InteractionType.None}", ButtonStyle.Secondary);
                component.WithButton(">", $"{Context.User.Id}.{(int)CommandEventHandler.InteractionType.CommandSearch}.{Context.Guild.Id}.{2}");
            }

            var keys = OliveGuild.Get(Context.Guild.Id).Commands.Keys.ToList();

            for (int i = 0; i < count; i++)
            {
                emb.AddField(keys.IndexOf(commands[i]).ToString(), commands[i], true);
            }

            await Context.ReplyEmbedAsync(emb.Build(), component: component.Build());
        }

        public static async Task ChangeSearchPage(SocketGuild guild, ulong userId, SocketUserMessage msg, string content, int page)
        {
            EmbedBuilder emb = msg.Embeds.First().ToEmbedBuilder();
            emb.Fields = new List<EmbedFieldBuilder>();

            List<string> commands = CustomCommandExecutor.FindCommands(OliveGuild.Get(guild.Id), content);

            var keys = OliveGuild.Get(guild.Id).Commands.Keys.ToList();

            int startIndex = MaxListCommandCount * (page - 1);

            for (int i = 0; i < MaxListCommandCount; i++)
            {
                int index = startIndex + i;

                if (commands.Count <= index) break;

                emb.AddField(keys.IndexOf(commands[index]).ToString(), commands[index], true);
            }

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("<", $"{userId}.{(int)CommandEventHandler.InteractionType.CommandSearch}.{guild.Id}.{page - 1}", disabled: startIndex == 0)
                .WithButton($"{page} / {Math.Ceiling(commands.Count / (float)MaxListCommandCount)}", $"{userId}.{(int)CommandEventHandler.InteractionType.None}", ButtonStyle.Secondary)
                .WithButton(">", $"{userId}.{(int)CommandEventHandler.InteractionType.CommandSearch}.{guild.Id}.{page + 1}", disabled: startIndex + MaxListCommandCount >= commands.Count);

            await msg.ModifyAsync(m =>
            {
                m.Embeds = new[] { emb.Build() };
                m.Components = component.Build();
            });
        }

        [Command("커맨드 사용 중지")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("자신의 메시지에 커스텀 커맨드가 실행되지 않게 만듭니다")]
        public async Task DoNotUseCommand()
        {
            OliveUser user = OliveUser.Get(Context.User.Id);

            if (!user.IsCommandEnabled)
            {
                await Context.ReplyEmbedAsync("이미 커맨드 사용이 비활성화돼있어요");

                return;
            }

            OliveUser.Set(Context.User.Id, u => u.IsCommandEnabled, false);

            await Context.ReplyEmbedAsync("커맨드 사용을 중지했어요");
        }

        [Command("커맨드 사용")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("자신의 메시지에 커스텀 커맨드가 다시 실행되게 만듭니다")]
        public async Task UseCommand()
        {
            OliveUser user = OliveUser.Get(Context.User.Id);

            if (user.IsCommandEnabled)
            {
                await Context.ReplyEmbedAsync("이미 커맨드 사용이 활성화돼있어요");

                return;
            }

            OliveUser.Set(Context.User.Id, u => u.IsCommandEnabled, true);

            await Context.ReplyEmbedAsync("커맨드 사용을 활성화했어요");
        }

        [Command("토스트")]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("토스트 커맨드를 실행합니다")]
        public async Task ExecuteToast([Name("입력"), Remainder] string lines)
        {
            if (CommandExecuteSession.Sessions.ContainsKey(Context.User.Id))
            {
                await Context.ReplyEmbedAsync("이미 다른 커맨드를 실행중이에요");

                return;
            }

            Toaster toaster = CustomCommandExecutor.GetToaster();
            var perm = (Context.User as SocketGuildUser).GuildPermissions;
            CustomCommandContext context = new(Context, new[] { Context.Message.Content }, perm.KickMembers, perm.BanMembers, perm.ManageRoles);

            object result = null;

            CommandExecuteSession.Sessions.Add(Context.User.Id, new(context));

            foreach (string line in lines.Split('\n'))
            {
                try
                {
                    result = toaster.Execute(line, context);
                }
                catch (Exception e)
                {
                    EmbedBuilder emb = Context.CreateEmbed(title: "오류 발생!", description: e.GetBaseException().Message);
                    await Context.ReplyEmbedAsync(emb.Build());
                }

                if (!CommandExecuteSession.Sessions.ContainsKey(Context.User.Id))
                {
                    break;
                }
            }

            if (CommandExecuteSession.Sessions.ContainsKey(Context.User.Id))
            {
                CommandExecuteSession.Sessions.Remove(Context.User.Id);
            }

            if (result is not null)
            {
                await Context.ReplyEmbedAsync(result);
            }
        }
    }
}
