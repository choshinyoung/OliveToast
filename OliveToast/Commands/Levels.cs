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

        [Command("순위")]
        [RequirePermission(PermissionType.UseBot), RequireContext(ContextType.Guild)]
        [Summary("서버 유저들의 레벨 순위를 확인합니다")]
        public async Task Rank()
        {
            OliveGuild guild = OliveGuild.Get(Context.Guild.Id);

            List<string> userIds = Context.Guild.Users.Select(u => u.Id.ToString()).ToList();
            var users = guild.Levels.Where(u => userIds.Contains(u.Key)).ToList();
            users.Sort((u1, u2) => u1.Value == u2.Value ? u1.Value.Xp.CompareTo(u2.Value.Xp) : u1.Value.Level.CompareTo(u2.Value.Level));
            users.Reverse();

            var rankUser = new List<KeyValuePair<string, OliveGuild.UserLevel>>(users);
            if (rankUser.Count > 5)
            {
                rankUser = rankUser.GetRange(0, 5);
            }

            EmbedBuilder emb = Context.CreateEmbed(title: "순위");

            for (int i = 0; i < rankUser.Count; i++)
            {
                emb.AddField($"{i + 1}위: {rankUser[i].Value.Level}/{rankUser[i].Value.Xp}", $"{Context.Guild.GetUser(ulong.Parse(rankUser[i].Key)).Mention}");
            }

            if (users.Any(u => u.Key == Context.User.Id.ToString()) && !rankUser.Any(u => u.Key == Context.User.Id.ToString())) 
            {
                var u = users.Where(u => u.Key == Context.User.Id.ToString()).First();

                emb.AddField($"{users.IndexOf(u)}위: {u.Value.Level}/{u.Value.Xp}", $"{Context.Guild.GetUser(ulong.Parse(u.Key)).Mention}");
            }

            await Context.MsgReplyEmbedAsync(emb.Build());
        }
    }
}
