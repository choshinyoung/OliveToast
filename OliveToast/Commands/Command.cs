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
        [Command("버튼"), Alias("버튼 테스트")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("와샌즈")]
        public async Task ButtonTest()
        {
            ComponentBuilder builder = new ComponentBuilder().WithButton("Click me!", "sanspapyrus", style: ButtonStyle.Danger);
            await Context.MsgReplyAsync("와 버튼 테스트", component: builder.Build());
        }

        [Command("드랍다운"), Alias("드랍다운드랍다운 테스트")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("와샌즈")]
        public async Task DropDownTest()
        {
            ComponentBuilder builder = new ComponentBuilder()
                .WithSelectMenu(new SelectMenuBuilder()
                .WithCustomId("sanssans")
                .WithPlaceholder("선택하세요")
                .WithOptions(new()
                {
                    new SelectMenuOptionBuilder()
                    .WithLabel("올리브토스트")
                    .WithEmote(Emote.Parse("<:OliveToast:870674018699735090>"))
                    .WithDescription("놀라운 봇")
                    .WithValue("olivetoast"),
                    new SelectMenuOptionBuilder()
                    .WithLabel("딸기도넛")
                    .WithEmote(Emote.Parse("<:StrawberryDonut:870674018540355674>"))
                    .WithDescription("망해가는 봇")
                    .WithValue("strawberrydonut"),
                    new SelectMenuOptionBuilder()
                    .WithLabel("미니봇")
                    .WithEmote(Emote.Parse("<:Minibot:870674019173670953>"))
                    .WithDescription("좀 이상한 봇")
                    .WithValue("minibot"),
                    new SelectMenuOptionBuilder()
                    .WithLabel("아삭크림")
                    .WithEmote(Emote.Parse("<:thinking_assac:856056208104357909>"))
                    .WithDescription("망봇")
                    .WithValue("assaccream"),
                }));

            await Context.MsgReplyAsync("와 드랍다운 테스트", component: builder.Build());
        }
    }
}
