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
    [Name("레벨")]
    [RequireCategoryEnable(CategoryType.Level)]
    public class Levels : ModuleBase<SocketCommandContext>
    {
        [Command("레벨 확인"), Alias("레벨 보기")]
        [RequirePermission(PermissionType.UseBot), RequireContext(ContextType.Guild)]
        [Summary("레벨을 확인합니다\n`유저`는 생략할 수 있습니다")]
        public async Task SeeLevel([Remainder, Name("유저")] SocketGuildUser user = null)
        {
            if (user == null)
            {
                user = Context.User as SocketGuildUser;
            }

            OliveGuild guild = OliveGuild.Get(Context.Guild.Id); 
            string UserId = Context.User.Id.ToString();

            if (!guild.Levels.ContainsKey(UserId))
            {
                guild.Levels.Add(UserId, new OliveGuild.UserLevel());

                OliveGuild.Set(Context.Guild.Id, g => g.Levels, guild.Levels);
            }

            await Context.MsgReplyEmbedAsync($"{user.Mention}님의 레벨은 {guild.Levels[UserId].Level}, XP는 {guild.Levels[UserId].Xp}에요");
        }
    }
}
