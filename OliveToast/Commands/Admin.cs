using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using OliveToast.Managements.Data;
using OliveToast.Utilities;
using System;
using System.Threading.Tasks;
using static OliveToast.Utilities.RequirePermission;

namespace OliveToast.Commands
{
    [Name("어드민")]
    public class Admin : ModuleBase<SocketCommandContext>
    {
        [Command("실행"), Alias("이발", "eval")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("C# 코드를 실행합니다")]
        public async Task Eval([Name("코드"), Remainder] string code)
        {
            code = code.Trim();
            if (code.StartsWith("```cs") && code.EndsWith("```"))
            {
                code = code[5..^3];
            }

            try
            {
                var result = await CSharpScript.EvaluateAsync(code, ScriptOptions.Default.WithReferences(typeof(Program).Assembly).WithImports(
                    "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading", "System.Threading.Tasks",
                    "System.Text", "System.Text.RegularExpressions", "System.IO", "System.Net", "System.Numerics",
                    "Discord", "Discord.Commands", "Discord.WebSocket", "Discord.Rest", "Discord.Net",
                    "Newtonsoft.Json", "HPark.Hangul", "Toast", "Toast.Nodes",
                    "OliveToast", "OliveToast.Managements", "OliveToast.Commands"
                    ), this);

                if (result is not null)
                {
                    await Context.ReplyEmbedAsync(result);
                }
            }
            catch (Exception e)
            {
                EmbedBuilder emb = Context.CreateEmbed(e.ToString().Slice(4000), "오류 발생!");
                await Context.ReplyEmbedAsync(emb.Build());
            }
        }

        [Command("화이트리스트")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("화이트리스트의 목록입니다")]
        public async Task WhiteList()
        {
            string output = "";
            foreach (ulong id in SpecialListManager.WhiteList)
            {
                try
                {
                    var user = Program.Client.GetUser(id);
                    output += $"{user.Username}#{user.DiscriminatorValue} ({user.Id})\n";
                }
                catch
                {
                    output += $"({id})";
                }
            }

            await Context.ReplyEmbedAsync(output == "" ? "화이트리스트 유저가 없어요" : output);
        }

        [Command("화이트리스트 추가")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("해당 유저를 봇 관리자로 만듭니다")]
        public async Task AddWhiteList([Name("유저"), Remainder] SocketUser user)
        {
            SpecialListManager.WhiteList.Add(user.Id);
            SpecialListManager.Update();

            await Context.ReplyEmbedAsync("해당 유저가 화이트리스트에 추가됐어요");
        }

        [Command("화이트리스트 제거")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("해당 유저를 봇 관리자 목록에서 제거합니다")]
        public async Task RemoveWhiteList([Name("유저"), Remainder] SocketUser user)
        {
            SpecialListManager.WhiteList.Remove(user.Id);
            SpecialListManager.Update();

            await Context.ReplyEmbedAsync("해당 유저가 화이트리스트에서 제거됐어요");
        }

        [Command("블랙리스트")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("블랙리스트의 목록입니다")]
        public async Task BlackList()
        {
            string output = "";
            foreach (ulong id in SpecialListManager.BlackList)
            {
                try
                {
                    var user = Program.Client.GetUser(id);
                    output += $"{user.Username}#{user.DiscriminatorValue} ({user.Id})\n";
                }
                catch
                {
                    output += $"({id})";
                }
            }

            await Context.ReplyEmbedAsync(output == "" ? "블랙리스트 유저가 없어요" : output);
        }

        [Command("블랙리스트 추가")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("해당 유저가 봇을 사용할 수 없게 만듭니다")]
        public async Task AddBlackList([Name("유저"), Remainder] SocketUser user)
        {
            SpecialListManager.BlackList.Add(user.Id);
            SpecialListManager.Update();

            await Context.ReplyEmbedAsync("해당 유저가 블랙리스트에 추가됐어요");
        }

        [Command("블랙리스트 제거")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("해당 유저를 블랙리스트에서 제거합니다")]
        public async Task RemoveBlackList([Name("유저"), Remainder] SocketUser user)
        {
            SpecialListManager.BlackList.Remove(user.Id);
            SpecialListManager.Update();

            await Context.ReplyEmbedAsync("해당 유저가 블랙리스트에서 제거됐어요");
        }

        [Command("메시지")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("해당 채널에 메시지를 보냅니다")]
        public async Task SendMessage([Name("채널")] SocketTextChannel channel, [Name("메시지"), Remainder] string msg)
        {
            using (channel.EnterTypingState())
            {
                await Task.Delay(5000);
                await channel.SendMessageAsync(msg);
            }

            await Context.ReplyEmbedAsync("메시지를 전송했어요");
        }

        [Command("메시지")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("해당 채널에 메시지를 보냅니다")]
        public async Task SendMessage([Name("채널")] ulong id, [Name("메시지"), Remainder] string msg)
        {
            var channel = Program.Client.GetChannel(id) as SocketTextChannel;

            using (channel.EnterTypingState()) 
            { 
                await Task.Delay(5000);
                await channel.SendMessageAsync(msg);
            }

            await Context.ReplyEmbedAsync("메시지를 전송했어요");
        }

        [Command("메시지")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("해당 유저에게 메시지를 보냅니다")]
        public async Task SendMessage([Name("유저")] SocketUser user, [Name("메시지"), Remainder] string msg)
        {
            var channel = await user.CreateDMChannelAsync();

            using (channel.EnterTypingState())
            {
                await Task.Delay(5000);
                await channel.SendMessageAsync(msg);
            }

            await Context.ReplyEmbedAsync("메시지를 전송했어요");
        }
    }
}
