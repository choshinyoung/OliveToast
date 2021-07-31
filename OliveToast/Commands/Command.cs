using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OliveToast.Managements.RequireCategoryEnable;
using static OliveToast.Managements.RequirePermission;

namespace OliveToast.Commands
{
    [Name("커맨드")]
    [RequireCategoryEnable(CategoryType.Command), RequireContext(ContextType.Guild)]
    public class Command : ModuleBase<SocketCommandContext>
    {
        [Command("커맨드 만들기"), Alias("커맨드 생성")]
        [RequirePermission(PermissionType.ManageCommand)]
        [Summary("커스텀 커맨드를 생성합니다")]
        public async Task CreateCommand([Name("커맨드")] string command, [Name("응답"), Remainder] string response)
        {
            var commands = OliveGuild.Get(Context.Guild.Id).Commands;

            if (commands.ContainsKey(command))
            {
                commands[command].Add(new(response, true, Array.Empty<string>(), Context.User.Id));
            }
            else {
                commands.Add(command, new() { new(response, true, Array.Empty<string>(), Context.User.Id) });
            }
            OliveGuild.Set(Context.Guild.Id, g => g.Commands, commands);

            EmbedBuilder emb = Context.CreateEmbed("커맨드를 만들었어요");
            emb.AddField("커맨드", command, true);
            emb.AddField("응답", response, true);

            await Context.MsgReplyEmbedAsync(emb.Build());
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
        public async Task DeleteCommand([Name("커맨드")] string command)
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

            for (int i = 0; i < commands.Count; i++)
            {
                emb.Description += $"{i}. {commands[i]}\n";
            }

            await Context.MsgReplyEmbedAsync(emb.Build());
        }
    }
}
