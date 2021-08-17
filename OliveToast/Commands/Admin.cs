using Discord;
using Discord.Commands;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using OliveToast.Managements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OliveToast.Managements.RequirePermission;

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
                EmbedBuilder emb = Context.CreateEmbed(e.ToString(), "오류 발생!");
                await Context.ReplyEmbedAsync(emb.Build());
            }
        }
    }
}
