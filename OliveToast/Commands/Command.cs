﻿using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using OliveToast.Managements.CustomCommand;
using OliveToast.Managements.Data;
using OliveToast.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toast;
using static OliveToast.Utilities.RequireCategoryEnable;
using static OliveToast.Utilities.RequirePermission;

namespace OliveToast.Commands
{
    [Name("커맨드")]
    [RequireCategoryEnable(CategoryType.Command), RequireContext(ContextType.Guild)]
    public class Command : ModuleBase<SocketCommandContext>
    {
        public const int MaxListCommandCount = 15;

        [Command("커맨드 만들기"), Alias("커맨드 생성")]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("커스텀 커맨드를 생성하는 커맨드예요")]
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
        [Summary("고급 설정을 사용해 커맨드를 생성하는 커맨드예요")]
        public async Task CreatCommand()
        {
            if (CommandCreateSession.Sessions.ContainsKey(Context.User.Id))
            {
                await Context.ReplyEmbedAsync("이미 커맨드를 만드는 중이에요");

                return;
            }

            OliveGuild.CustomCommand command = new(null, false, new(), Context.User.Id, (Context.User as SocketGuildUser).GuildPermissions);

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("정규식으로 변경", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CreateCommand, (int)CommandCreateSession.ResponseType.ChangeRegex))
                .WithButton("취소", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CreateCommand, (int)CommandCreateSession.ResponseType.Cancel), ButtonStyle.Danger)
                .WithButton("커맨드 사용 방법", style: ButtonStyle.Link, url: "https://olivetoast.shinyou.ng/");

            RestUserMessage msg = await Context.ReplyEmbedAsync("커맨드를 입력해주세요", component: component.Build());

            CommandCreateSession.Sessions.Add(Context.User.Id, new()
            {
                SessionStatus = CommandCreateSession.Status.CommandInput,
                CustomCommand = command,
                Message = msg,
                UserMessageContext = Context,
                LastActiveTime = DateTime.Now,
                Type = CommandCreateSession.CommandType.CustomCommand,
            });
        }

        [Command("커맨드 삭제"), Alias("커맨드 제거"), Priority(1)]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("커스텀 커맨드를 제거하는 커맨드예요")]
        public async Task DeleteCommandByIndex([Name("번호")] int index)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (commands.Count <= index)
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            string target = commands.ElementAt(index).Key;

            await DeleteCommand(target);
        }

        [Command("커맨드 삭제"), Alias("커맨드 제거"), Priority(-1)]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("커스텀 커맨드를 제거하는 커맨드예요")]
        public async Task DeleteCommand([Name("커맨드"), Remainder] string command)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (!commands.ContainsKey(command))
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            EmbedBuilder emb = Context.CreateEmbed($"`{command}`커맨드를 삭제할까요?", "커맨드 삭제");

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("삭제", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.DeleteCommand, (int)CommandDeleteSession.ResponseType.DeleteCommand), ButtonStyle.Danger)
                .WithButton("취소", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.DeleteCommand, (int)CommandDeleteSession.ResponseType.Cancel));

            await Context.ReplyEmbedAsync(emb.Build(), component: component.Build());

            CommandDeleteSession.Sessions.Add(Context.User.Id, new(Context, command));
        }

        [Command("커맨드 삭제"), Alias("커맨드 제거")]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("커스텀 커맨드를 제거하는 커맨드예요")]
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
        [Summary("커스텀 커맨드를 제거하는 커맨드예요")]
        public async Task DeleteCommand([Name("커맨드")] string command, [Name("응답"), Remainder] string answer)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (!commands.ContainsKey(command) || !commands[command].Any(c => c.Answer == answer))
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            EmbedBuilder emb = Context.CreateEmbed($"`{command}`커맨드의 응답 `{answer}` {commands[command].Count(c => c.Answer == answer)}개를 삭제할까요?", "커맨드 삭제");

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("삭제", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.DeleteCommand, (int)CommandDeleteSession.ResponseType.DeleteAnswers), ButtonStyle.Danger)
                .WithButton("취소", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.DeleteCommand, (int)CommandDeleteSession.ResponseType.Cancel));

            await Context.ReplyEmbedAsync(emb.Build(), component: component.Build());

            CommandDeleteSession.Sessions.Add(Context.User.Id, new(Context, command, answer));
        }

        [Command("커맨드 삭제"), Alias("커맨드 제거")]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("커스텀 커맨드를 제거하는 커맨드예요")]
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
        [Summary("커스텀 커맨드를 제거하는 커맨드예요")]
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
        [Summary("커스텀 커맨드를 제거하는 커맨드예요")]
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

            if (CommandDeleteSession.Sessions.ContainsKey(Context.User.Id))
            {
                await Context.ReplyEmbedAsync("이미 다른 커맨드를 삭제중이에요");
                return;
            }

            EmbedBuilder emb = Context.CreateEmbed("이 커맨드를 삭제할까요?", "커맨드 삭제");

            var answer = commands[command][aIndex];

            emb.AddField("커맨드", command, true);
            if (answer.Answer is not null)
            {
                emb.AddField("응답", answer.Answer, true);
            }
            emb.AddField("정규식 사용 여부", answer.IsRegex.ToEmoji(), true);

            if (answer.ToastLines.Count > 0)
            {
                emb.AddField("토스트 커맨드", string.Concat(answer.ToastLines.Select(l => $"```\n{l.Slice(60)}\n```")));
            }

            SocketGuildUser user = Context.Guild.Users.ToList().Find(u => u.Id == answer.CreatedBy);
            emb.AddField("제작자", user is null ? answer.CreatedBy.ToString() : $"{user.Username}#{user.Discriminator}");

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("삭제", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.DeleteCommand, (int)CommandDeleteSession.ResponseType.DeleteSingleAnswer), ButtonStyle.Danger)
                .WithButton("취소", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.DeleteCommand, (int)CommandDeleteSession.ResponseType.Cancel));

            await Context.ReplyEmbedAsync(emb.Build(), component: component.Build());

            CommandDeleteSession.Sessions.Add(Context.User.Id, new(Context, cIndex, aIndex));
        }

        [Command("커맨드 목록")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 목록이에요")]
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
                component.WithButton("<", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CommandList, Context.Guild.Id, 0), disabled: true);
                component.WithButton($"1 / {Math.Ceiling(commands.Count / (float)MaxListCommandCount)}", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CommandList, Context.Guild.Id, -1), ButtonStyle.Secondary);
                component.WithButton(">", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CommandList, Context.Guild.Id, 2));
            }

            List<SelectMenuOptionBuilder> options = new();

            for (int i = 0; i < count; i++)
            {
                emb.AddField(i.ToString(), commands[i], true);

                options.Add(new(i.ToString(), i.ToString(), commands[i].Slice(100)));
            }

            if (commands.Count > 0)
            {
                component.WithSelectMenu(InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CommandListSelectMenu, Context.Guild.Id), options, "커맨드 선택하기", row: 1);
            }

            await Context.ReplyEmbedAsync(emb.Build(), component: component.Build());
        }

        public static async Task ChangeListPage(SocketGuild guild, ulong userId, SocketUserMessage msg, int page)
        {
            if (page == -1)
            {
                page = 1;
            }

            List<string> commands = OliveGuild.Get(guild.Id).Commands.Keys.ToList();

            EmbedBuilder emb = msg.Embeds.First().ToEmbedBuilder();
            emb.Fields = new List<EmbedFieldBuilder>();

            int startIndex = MaxListCommandCount * (page - 1);

            List<SelectMenuOptionBuilder> options = new();

            for (int i = 0; i < MaxListCommandCount; i++)
            {
                int index = startIndex + i;

                if (commands.Count <= index) break;

                emb.AddField(index.ToString(), commands[index], true);

                options.Add(new(index.ToString(), index.ToString(), commands[index].Slice(100)));
            }

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("<", InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.CommandList, guild.Id, page - 1), disabled: startIndex == 0)
                .WithButton($"{page} / {Math.Ceiling(commands.Count / (float)MaxListCommandCount)}", InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.CommandList, guild.Id, -1), ButtonStyle.Secondary)
                .WithButton(">", InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.CommandList, guild.Id, page + 1), disabled: startIndex + MaxListCommandCount >= commands.Count);

            component.WithSelectMenu(InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.CommandListSelectMenu, guild.Id), options, "커맨드 선택하기", row: 1);

            await msg.ModifyAsync(m =>
            {
                m.Embeds = new[] { emb.Build() };
                m.Components = component.Build();
            });
        }

        [Command("커맨드 목록"), Alias("응답 목록")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 응답 목록이에요")]
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
        [Summary("커스텀 커맨드의 응답 목록이에요")]
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
                component.WithButton("<", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CommandAnswerList, Context.Guild.Id, index, 0), disabled: true);
                component.WithButton($"1 / {Math.Ceiling(commands.Count / (float)MaxListCommandCount)}", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CommandAnswerList, Context.Guild.Id, index, -1), ButtonStyle.Secondary);
                component.WithButton(">", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CommandAnswerList, Context.Guild.Id, index, 2));
            }

            EmbedBuilder emb = Context.CreateEmbed("", $"`{command}` 커맨드의 응답 목록");

            List<SelectMenuOptionBuilder> options = new();

            for (int i = 0; i < count; i++)
            {
                SocketGuildUser user = Context.Guild.Users.ToList().Find(u => u.Id == answer[i].CreatedBy);
                string username = user is null ? answer[i].CreatedBy.ToString() : $"{user.Username}#{user.Discriminator}";
                string isRegex = answer[i].IsRegex ? "- 정규식" : "";
                string toastCommands = answer[i].ToastLines.Any() ? $"\n커맨드 {answer[i].ToastLines.Count}줄" : "";

                emb.AddField($"{i} {isRegex} {toastCommands}", answer[i].Answer ?? "응답이 없어요", true);

                options.Add(new(i.ToString(), i.ToString(), (answer[i].Answer ?? "응답이 없어요").Slice(100)));
            }

            component.WithSelectMenu(InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CommandAnswerListSelectMenu, Context.Guild.Id, index), options, "응답 선택하기", row: 1);

            await Context.ReplyEmbedAsync(emb.Build(), component: component.Build());
        }

        public static async Task ChangeAnswerListPage(SocketGuild guild, ulong userId, SocketUserMessage msg, int cIndex, int page)
        {
            if (page == -1)
            {
                page = 1;
            }

            var commands = OliveGuild.Get(guild.Id).Commands;

            string command = commands.Keys.ToList()[cIndex];
            var answer = commands[command];

            EmbedBuilder emb = msg.Embeds.First().ToEmbedBuilder();
            emb.Title = $"`{command}` 커맨드의 응답 목록";
            emb.Fields = new List<EmbedFieldBuilder>();

            int startIndex = MaxListCommandCount * (page - 1);

            List<SelectMenuOptionBuilder> options = new();

            for (int i = 0; i < MaxListCommandCount; i++)
            {
                int index = startIndex + i;

                if (answer.Count <= index) break;

                SocketGuildUser user = guild.Users.ToList().Find(u => u.Id == answer[index].CreatedBy);
                string username = user is null ? answer[index].CreatedBy.ToString() : $"{user.Username}#{user.Discriminator}";
                string isRegex = answer[index].IsRegex ? "- 정규식" : "";
                string toastCommands = answer[index].ToastLines.Any() ? $"\n커맨드 {answer[index].ToastLines.Count}줄" : "";

                emb.AddField($"{index} {isRegex} {toastCommands}", answer[index].Answer ?? "응답이 없어요", true);

                options.Add(new(index.ToString(), index.ToString(), (answer[i].Answer ?? "응답이 없어요").Slice(100)));
            }

            ComponentBuilder component = new();
            if (answer.Count > MaxListCommandCount) {
                component.WithButton("<", InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.CommandAnswerList, guild.Id, cIndex, page - 1), disabled: startIndex == 0);
                component.WithButton($"{page} / {Math.Ceiling(answer.Count / (float)MaxListCommandCount)}", InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.CommandAnswerList, guild.Id, cIndex, -1), ButtonStyle.Secondary);
                component.WithButton(">", InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.CommandAnswerList, guild.Id, cIndex, page + 1), disabled: startIndex + MaxListCommandCount >= answer.Count);
            }

            component.WithSelectMenu(InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.CommandAnswerListSelectMenu, guild.Id, cIndex), options, "응답 선택하기", row: 1);

            await msg.ModifyAsync(m =>
            {
                m.Embeds = new[] { emb.Build() };
                m.Components = component.Build();
            });
        }

        [Command("커맨드 정보"), Alias("응답 정보")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 응답의 정보를 확인할 수 있는 커맨드예요")]
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
        [Summary("커스텀 커맨드의 응답의 정보를 확인할 수 있는 커맨드예요")]
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
        [Summary("커스텀 커맨드의 응답의 정보를 확인할 수 있는 커맨드예요")]
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
        [Summary("커스텀 커맨드의 응답의 정보를 확인할 수 있는 커맨드예요")]
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

            EmbedBuilder emb = Context.CreateEmbed("", "커맨드 정보");

            emb.AddField("커맨드", command, true);
            if (answer.Answer is not null)
            {
                emb.AddField("응답", answer.Answer, true);
            }
            emb.AddField("정규식 사용 여부", answer.IsRegex.ToEmoji(), true);

            if (answer.ToastLines.Count > 0)
            {
                emb.AddField("토스트 커맨드", string.Concat(answer.ToastLines.Select(l => $"```\n{l.Slice(60)}\n```")));
            }

            SocketGuildUser user = Context.Guild.Users.ToList().Find(u => u.Id == answer.CreatedBy);
            emb.AddField("제작자", user is null ? answer.CreatedBy.ToString() : $"{user.Username}#{user.Discriminator}");

            await Context.ReplyEmbedAsync(emb.Build());
        }

        public static async Task UpdateCommandInfo(SocketGuild guild, SocketUserMessage msg, int cIndex, int aIndex)
        {
            var commands = OliveGuild.Get(guild.Id).Commands;

            string command = commands.Keys.ToList()[cIndex];
            var answer = commands[command][aIndex];

            EmbedBuilder emb = msg.Embeds.First().ToEmbedBuilder();
            emb.Title = "커맨드 정보";
            emb.Fields = new List<EmbedFieldBuilder>();

            emb.AddField("커맨드", command, true);
            if (answer.Answer is not null)
            {
                emb.AddField("응답", answer.Answer, true);
            }
            emb.AddField("정규식 사용 여부", answer.IsRegex.ToEmoji(), true);

            if (answer.ToastLines.Count > 0)
            {
                emb.AddField("토스트 커맨드", string.Concat(answer.ToastLines.Select(l => $"```\n{l.Slice(60)}\n```")));
            }

            SocketGuildUser user = guild.Users.ToList().Find(u => u.Id == answer.CreatedBy);
            emb.AddField("제작자", user is null ? answer.CreatedBy.ToString() : $"{user.Username}#{user.Discriminator}");

            await msg.ModifyAsync(m =>
            {
                m.Embeds = new[] { emb.Build() };
                m.Components = null;
            });
        }

        [Command("토스트 커맨드 다운로드"), Alias("커맨드 다운로드")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 토스트 커맨드를 다운로드 할 수 있는 커맨드예요")]
        public async Task DownloadToastCommand([Name("커맨드")] string command, [Name("응답"), Remainder] string answer)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (!commands.ContainsKey(command) || !commands[command].Any(c => c.Answer == answer))
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            await DownloadToastCommand(commands.Keys.ToList().IndexOf(command), commands[command].IndexOf(commands[command].Find(c => c.Answer == answer)));
        }

        [Command("토스트 커맨드 다운로드"), Alias("커맨드 다운로드")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 토스트 커맨드를 다운로드 할 수 있는 커맨드예요")]
        public async Task DownloadToastCommand([Name("커맨드 번호")] int cIndex, [Name("응답"), Remainder] string answer)
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

            await DownloadToastCommand(cIndex, commands[command].IndexOf(commands[command].Find(c => c.Answer == answer)));
        }

        [Command("토스트 커맨드 다운로드"), Alias("커맨드 다운로드")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 토스트 커맨드를 다운로드 할 수 있는 커맨드예요")]
        public async Task DownloadToastCommand([Name("커맨드")] string command, [Name("응답 번호")] int aIndex)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (!commands.ContainsKey(command) || commands[command].Count <= aIndex)
            {
                await Context.ReplyEmbedAsync("존재하지 않는 커맨드에요");
                return;
            }

            await DownloadToastCommand(commands.Keys.ToList().IndexOf(command), aIndex);
        }

        [Command("토스트 커맨드 다운로드"), Alias("커맨드 다운로드"), Priority(1)]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커스텀 커맨드의 토스트 커맨드를 다운로드 할 수 있는 커맨드예요")]
        public async Task DownloadToastCommand([Name("커맨드 번호")] int cIndex, [Name("응답 번호")] int aIndex)
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

            if (!answer.ToastLines.Any())
            {
                await Context.ReplyEmbedAsync("이 커맨드에는 토스트 커맨드가 없어요");

                return;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(answer.ToastLines, Formatting.Indented));
            MemoryStream stream = new(bytes);

            await Context.Channel.SendFileAsync(stream, "toastLines.json");

            stream.Dispose();
        }

        [Command("커맨드 검색")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("`메시지`를 보냈을 때 어떤 커맨드가 실행되는지 확인할 수 있는 커맨드예요")]
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
                component.WithButton("<", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CommandSearchList, Context.Guild.Id, 0, content), disabled: true);
                component.WithButton($"1 / {Math.Ceiling(commands.Count / (float)MaxListCommandCount)}", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CommandSearchList, Context.Guild.Id, -1, content), ButtonStyle.Secondary);
                component.WithButton(">", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CommandSearchList, Context.Guild.Id, 2, content));
            }

            var keys = OliveGuild.Get(Context.Guild.Id).Commands.Keys.ToList();

            List<SelectMenuOptionBuilder> options = new();
            for (int i = 0; i < count; i++)
            {
                int index = keys.IndexOf(commands[i]);

                emb.AddField(index.ToString(), commands[i], true);

                options.Add(new(index.ToString(), index.ToString(), commands[i]));
            }

            component.WithSelectMenu(InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.CommandListSelectMenu, Context.Guild.Id), options, "커맨드 선택하기", row: 1);

            await Context.ReplyEmbedAsync(emb.Build(), component: component.Build());
        }

        public static async Task ChangeSearchPage(SocketGuild guild, ulong userId, SocketUserMessage msg, string content, int page)
        {
            if (page == -1)
            {
                page = 1;
            }

            EmbedBuilder emb = msg.Embeds.First().ToEmbedBuilder();
            emb.Fields = new List<EmbedFieldBuilder>();

            List<string> commands = CustomCommandExecutor.FindCommands(OliveGuild.Get(guild.Id), content);

            var keys = OliveGuild.Get(guild.Id).Commands.Keys.ToList();

            int startIndex = MaxListCommandCount * (page - 1);

            List<SelectMenuOptionBuilder> options = new();
            for (int i = 0; i < MaxListCommandCount; i++)
            {
                int index = startIndex + i;

                if (commands.Count <= index) break;

                int keyIndex = keys.IndexOf(commands[index]);

                emb.AddField(keyIndex.ToString(), commands[index], true);

                options.Add(new(keyIndex.ToString(), keyIndex.ToString(), commands[index]));
            }

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("<", InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.CommandSearchList, guild.Id, page - 1, content), disabled: startIndex == 0)
                .WithButton($"{page} / {Math.Ceiling(commands.Count / (float)MaxListCommandCount)}", InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.CommandSearchList, guild.Id, -1, content), ButtonStyle.Secondary)
                .WithButton(">", InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.CommandSearchList, guild.Id, page + 1, content), disabled: startIndex + MaxListCommandCount >= commands.Count);

            component.WithSelectMenu(InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.CommandListSelectMenu, guild.Id), options, "커맨드 선택하기", row: 1);

            await msg.ModifyAsync(m =>
            {
                m.Embeds = new[] { emb.Build() };
                m.Components = component.Build();
            });
        }

        [Command("커맨드 사용 중지")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("자신의 메시지에 커스텀 커맨드가 실행되지 않게 만드는 커맨드예요")]
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
        [Summary("자신의 메시지에 커스텀 커맨드가 다시 실행되게 만드는 커맨드예요")]
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
        [Summary("토스트 커맨드를 실행하는 커맨드예요\n커맨드 라인은 줄바꿈 두 개로 구분돼요")]
        public async Task ExecuteToast([Name("입력"), Remainder] string lines)
        {
            if (CommandExecuteSession.Sessions.ContainsKey(Context.User.Id))
            {
                await Context.ReplyEmbedAsync("이미 다른 커맨드를 실행중이에요");

                return;
            }

            Toaster toaster = CustomCommandExecutor.GetToaster();
            var perm = (Context.User as SocketGuildUser).GuildPermissions;
            CustomCommandContext context = new(Context, Context.User.Id, new[] { Context.Message.Content }, perm.KickMembers, perm.BanMembers, perm.ManageRoles);

            object result = null;

            CommandExecuteSession.Sessions.Add(Context.User.Id, new(context));

            try
            {
                foreach (string line in lines.Split("\n\n"))
                {
                    result = toaster.Execute(line, context);

                    if (!CommandExecuteSession.Sessions.ContainsKey(Context.User.Id))
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                EmbedBuilder emb = Context.CreateEmbed(title: "오류 발생!", description: e.GetBaseException().Message);
                await Context.ReplyEmbedAsync(emb.Build());
            }

            if (CommandExecuteSession.Sessions.ContainsKey(Context.User.Id))
            {
                CommandExecuteSession.Sessions.Remove(Context.User.Id);
            }

            if (result is not null)
            {
                await Context.ReplyEmbedAsync(toaster.ExecuteConverter<string>(result, context));
            }
        }
    }
}
