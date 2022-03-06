using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using OliveToast.Managements.Data;
using OliveToast.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OliveToast.Utilities.RequirePermission;

namespace OliveToast.Commands
{
    [Name("어드민")]
    public class Admin : ModuleBase<SocketCommandContext>
    {
        public static Dictionary<string, List<(ulong userId, ulong guildId)>> CommandStats = new();

        [Command("실행"), Alias("이발", "eval")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("C# 코드를 실행할 수 있는 커맨드예요")]
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
                    "OliveToast", "OliveToast.Managements", "OliveToast.Commands", "OliveToast.Utilities", 
                    "OliveToast.Managements.CustomCommand", "OliveToast.Managements.Data"
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
        [Summary("화이트리스트의 목록이에요")]
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
                    output += $"({id})\n";
                }
            }

            await Context.ReplyEmbedAsync(output == "" ? "화이트리스트 유저가 없어요" : output);
        }

        [Command("화이트리스트 추가")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("해당 유저를 봇 관리자로 만드는 커맨드예요")]
        public async Task AddWhiteList([Name("유저"), Remainder] SocketUser user)
        {
            if (SpecialListManager.WhiteList.Contains(user.Id))
            {
                await Context.ReplyEmbedAsync("해당 유저는 이미 화이트리스트에 있어요");

                return;
            }

            SpecialListManager.WhiteList.Add(user.Id);
            SpecialListManager.Update();

            await Context.ReplyEmbedAsync("해당 유저가 화이트리스트에 추가됐어요");
        }

        [Command("화이트리스트 제거")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("해당 유저를 봇 관리자 목록에서 제거하는 커맨드예요")]
        public async Task RemoveWhiteList([Name("유저"), Remainder] SocketUser user)
        {
            if (!SpecialListManager.WhiteList.Contains(user.Id))
            {
                await Context.ReplyEmbedAsync("해당 유저가 화이트리스트에 없어요");

                return;
            }

            SpecialListManager.WhiteList.Remove(user.Id);
            SpecialListManager.Update();

            await Context.ReplyEmbedAsync("해당 유저가 화이트리스트에서 제거됐어요");
        }

        [Command("블랙리스트")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("블랙리스트의 목록이에요")]
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
                    output += $"({id})\n";
                }
            }

            await Context.ReplyEmbedAsync(output == "" ? "블랙리스트 유저가 없어요" : output);
        }

        [Command("블랙리스트 추가")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("해당 유저가 봇을 사용할 수 없게 만드는 커맨드예요")]
        public async Task AddBlackList([Name("유저"), Remainder] SocketUser user)
        {
            if (SpecialListManager.BlackList.Contains(user.Id))
            {
                await Context.ReplyEmbedAsync("해당 유저는 이미 블랙리스트에 있어요");

                return;
            }

            SpecialListManager.BlackList.Add(user.Id);
            SpecialListManager.Update();

            await Context.ReplyEmbedAsync("해당 유저가 블랙리스트에 추가됐어요");
        }

        [Command("블랙리스트 제거")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("해당 유저를 블랙리스트에서 제거하는 커맨드예요")]
        public async Task RemoveBlackList([Name("유저"), Remainder] SocketUser user)
        {
            if (!SpecialListManager.BlackList.Contains(user.Id))
            {
                await Context.ReplyEmbedAsync("해당 유저가 블랙리스트에 없어요");

                return;
            }

            SpecialListManager.BlackList.Remove(user.Id);
            SpecialListManager.Update();

            await Context.ReplyEmbedAsync("해당 유저가 블랙리스트에서 제거됐어요");
        }

        [Command("메시지")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("특정 채널에 메시지를 보내는 커맨드예요")]
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
        [Summary("특정 채널에 메시지를 보내는 커맨드예요")]
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
        [Summary("특정 유저에게 메시지를 보내는 커맨드예요")]
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

        [Command("통계")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커맨드 사용량을 확인하는 커맨드예요")]
        public async Task CheckStat()
        {
            var sorted = CommandStats.ToList();
            sorted.Sort((p1, p2) => p2.Value.Count.CompareTo(p1.Value.Count));

            var str = sorted.Select(p => $"`{CommandEventHandler.prefix}{p.Key}`: {p.Value.Count}\n");

            var t = DateTime.Now - Program.Uptime;

            EmbedBuilder emb = Context.CreateEmbed(string.Concat(str), $"지난 {t.Days}일 {t.Hours}시간 동안 사용된 커맨드 통계");

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("투표확인"), Alias("투표 확인")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("해당 유저가 올리브토스트를 투표했는지 확인하는 커맨드예요")]
        public async Task CheckIsVoted([Remainder, Name("유저")]SocketUser user)
        {
            await CheckIsVoted(user.Id);
        }

        [Command("투표확인"), Alias("투표 확인")]
        [RequirePermission(PermissionType.BotAdmin)]
        [Summary("해당 유저가 올리브토스트를 투표했는지 확인하는 커맨드예요")]
        public async Task CheckIsVoted([Remainder, Name("유저")]ulong id)
        {
            var data = (await KoreanBots.GetVotedData(id)).data;

            DateTime time = Utility.TimestampToDateTime(data.lastVote.Value);

            await Context.ReplyEmbedAsync($"{data.voted}\n{time.ToLongDateString()} {time.ToLongTimeString()}");
        }
    }
}
