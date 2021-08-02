using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
                commands[command].Add(new(answer, true, new(), new(), Context.User.Id));
            }
            else 
            {
                commands.Add(command, new() { new(answer, true, new(), new(), Context.User.Id) });
            }
            OliveGuild.Set(Context.Guild.Id, g => g.Commands, commands);

            EmbedBuilder emb = Context.CreateEmbed("커맨드를 만들었어요");
            emb.AddField("커맨드", command, true);
            emb.AddField("응답", answer, true);

            await Context.MsgReplyEmbedAsync(emb.Build());
        }

        [Command("커맨드 만들기"), Alias("커맨드 생성")]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("고급 설정을 사용해 커맨드를 생성합니다")]
        public async Task CreatCommand()
        {
            OliveGuild.CustomCommand command = new(null, false, new(), new(), Context.User.Id);

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("정규식으로 변경", $"{Context.User.Id}.{(int)CommandEventHandler.InteractionType.CreateCommand}.{(int)CommandCreateSession.ResponseType.ChangeRegex}")
                .WithButton("취소", $"{Context.User.Id}.{(int)CommandEventHandler.InteractionType.CreateCommand}.{(int)CommandCreateSession.ResponseType.Cancel}", ButtonStyle.Danger);

            RestUserMessage msg = await Context.MsgReplyEmbedAsync("커맨드를 입력해주세요", component: component.Build());

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
                await Context.MsgReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            string target = commands.ElementAt(index).Key;
            commands.Remove(target);
            OliveGuild.Set(Context.Guild.Id, g => g.Commands, commands);
            
            await Context.MsgReplyEmbedAsync($"`{target}` 커맨드를 삭제했어요");
        }

        [Command("커맨드 삭제"), Alias("커맨드 제거")]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("커스텀 커맨드를 제거합니다")]
        public async Task DeleteCommand([Name("커맨드"), Remainder] string command)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (!commands.ContainsKey(command))
            {
                await Context.MsgReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            int count = commands[command].Count;
            commands.Remove(command);
            OliveGuild.Set(Context.Guild.Id, g => g.Commands, commands);
            
            await Context.MsgReplyEmbedAsync($"해당 커맨드 {count}개를 삭제했어요");
        }

        [Command("커맨드 목록")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 목록을 확인합니다")]
        public async Task CommandList()
        {
            List<string> commands = OliveGuild.Get(Context.Guild.Id).Commands.Keys.ToList();

            EmbedBuilder emb = Context.CreateEmbed("", "커맨드 목록");

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

            await Context.MsgReplyEmbedAsync(emb.Build(), component: component.Build());
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
                .WithButton($"1 / {Math.Ceiling(commands.Count / (float)MaxListCommandCount)}", $"{userId}.{(int)CommandEventHandler.InteractionType.None}", ButtonStyle.Secondary)
                .WithButton(">", $"{userId}.{(int)CommandEventHandler.InteractionType.CommandList}.{guild.Id}.{page + 1}", disabled: startIndex + MaxListCommandCount >= commands.Count);

            await msg.ModifyAsync(m =>
            {
                m.Embed = emb.Build();
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
                await Context.MsgReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

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

                emb.AddField($"{i} - {(user is null ? answer[i].CreatedBy : $"{user.Username}#{user.Discriminator}")}", answer[i].Answer ?? "응답이 없어요", true);
            }

            await Context.MsgReplyEmbedAsync(emb.Build(), component: component.Build());
        }

        [Command("커맨드 목록"), Alias("응답 목록"), Priority(1)]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 응답 목록을 확인합니다")]
        public async Task AnswerList([Name("번호")] int index)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (commands.Count <= index)
            {
                await Context.MsgReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            string command = commands.Keys.ToList()[index];
            await AnswerList(command);
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

                SocketGuildUser user = guild.Users.ToList().Find(u => u.Id == answer[i].CreatedBy);

                emb.AddField($"{index} - {(user is null ? answer[index].CreatedBy : $"{user.Username}#{user.Discriminator}")}", answer[index].Answer ?? "응답이 없어요", true);
            }

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("<", $"{userId}.{(int)CommandEventHandler.InteractionType.CommandAnswerList}.{guild.Id}.{page - 1}", disabled: startIndex == 0)
                .WithButton($"1 / {Math.Ceiling(answer.Count / (float)MaxListCommandCount)}", $"{userId}.{(int)CommandEventHandler.InteractionType.None}", ButtonStyle.Secondary)
                .WithButton(">", $"{userId}.{(int)CommandEventHandler.InteractionType.CommandAnswerList}.{guild.Id}.{page + 1}", disabled: startIndex + MaxListCommandCount >= answer.Count);

            await msg.ModifyAsync(m =>
            {
                m.Embed = emb.Build();
                m.Components = component.Build();
            });
        }

        [Command("토스트")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("토스트 커맨드를 실행합니다")]
        public async Task ExecuteToast([Name("입력"), Remainder] string line)
        {
            Toaster toaster = new();
            toaster.AddCommand(BasicCommands.All);
            toaster.AddConverter(BasicConverters.All);

            object result = toaster.Execute(line);

            if (result is not null)
            {
                await Context.MsgReplyEmbedAsync(result);
            }
        }
    }
}
