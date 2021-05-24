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
    [RequireCategoryEnable(CategoryType.Level), RequireContext(ContextType.Guild)]
    public class Levels : ModuleBase<SocketCommandContext>
    {
        [Command("레벨 확인"), Alias("레벨 보기", "레벨")]
        [RequirePermission(PermissionType.UseBot)]
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

            EmbedBuilder emb = Context.CreateEmbed();

            emb.AddField("level", guild.Levels[UserId].Level, true);
            emb.AddField("xp", guild.Levels[UserId].Xp, true);

            int totalXp = Utility.GetLevelXp(guild.Levels[UserId].Level);
            emb.AddField($"레벨업까지 남은 xp: {totalXp - guild.Levels[UserId].Xp}", $"{guild.Levels[UserId].Xp} [{new string(Enumerable.Repeat('■', (int)Math.Round(guild.Levels[UserId].Xp / (float)totalXp * 10)).ToArray())}{new string(Enumerable.Repeat('□', (int)Math.Round((totalXp - guild.Levels[UserId].Xp) / (float)totalXp * 10)).ToArray())}] {totalXp}");

            await Context.MsgReplyEmbedAsync(emb.Build());
        }

        [Command("순위")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("서버 유저들의 레벨 순위를 확인합니다")]
        public async Task Rank()
        {
            OliveGuild guild = OliveGuild.Get(Context.Guild.Id);

            List<string> userIds = Context.Guild.Users.Select(u => u.Id.ToString()).ToList();
            var users = guild.Levels.Where(u => userIds.Contains(u.Key)).ToList();
            users.Sort((u1, u2) => u1.Value.Level == u2.Value.Level ? u2.Value.Xp.CompareTo(u1.Value.Xp) : u2.Value.Level.CompareTo(u1.Value.Level));

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

                emb.AddField($"{users.IndexOf(u) + 1}위: {u.Value.Level}/{u.Value.Xp}", $"{Context.Guild.GetUser(ulong.Parse(u.Key)).Mention}");
            }

            await Context.MsgReplyEmbedAsync(emb.Build());
        }

        [Command("레벨 역할 추가"), Alias("레벨 역할 설정", "레벨 역할")]
        [RequirePermission(PermissionType.ManageBotSetting)]
        [Summary("특정 레벨에 도달하면 얻을 수 있는 역할을 설정합니다")]
        public async Task AddLevelRole([Name("레벨")] int level, [Remainder, Name("역할")] SocketRole role)
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;

            if (setting.LevelRoles.ContainsKey(level.ToString()))
            {
                await Context.MsgReplyEmbedAsync("해당 레벨의 역할이 이미 설정돼있어요");
                return;
            }

            setting.LevelRoles.Add(level.ToString(), role.Id);
            OliveGuild.Set(Context.Guild.Id, g => g.Setting, setting);

            await Context.MsgReplyEmbedAsync($"이제 {level} 레벨이 되면 {role.Mention} 역할을 얻을 수 있어요");
        }

        [Command("레벨 역할 제거"), Alias("레벨 역할 삭제")]
        [RequirePermission(PermissionType.ManageBotSetting)]
        [Summary("특정 레벨에 도달하면 얻을 수 있는 역할을 설정합니다")]
        public async Task RemoveLevelRole([Name("레벨")] int level)
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;

            if (!setting.LevelRoles.ContainsKey(level.ToString()))
            {
                await Context.MsgReplyEmbedAsync("해당 레벨에 설정된 역할이 없어요");
                return;
            }

            setting.LevelRoles.Remove(level.ToString());
            OliveGuild.Set(Context.Guild.Id, g => g.Setting, setting);

            await Context.MsgReplyEmbedAsync($"{level} 레벨의 역할을 제거했어요");
        }

        [Command("레벨 역할 확인"), Alias("레벨 역할 보기", "레벨 역할")]
        [RequirePermission(PermissionType.ManageBotSetting)]
        [Summary("특정 레벨에 도달하면 얻을 수 있는 역할을 설정합니다")]
        public async Task SeeLevelRole()
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;

            EmbedBuilder emb = Context.CreateEmbed(title: "레벨 역할");

            var roles = setting.LevelRoles.Where(lr => Context.Guild.Roles.Any(r => r.Id == lr.Value)).ToList();

            if (roles.Count == 0)
            {
                emb.Description = "설정된 레벨 역할이 없어요";
                await Context.MsgReplyEmbedAsync(emb.Build());
                return;
            }

            foreach (var value in roles)
            {
                emb.AddField($"{value.Key} 레벨", Context.Guild.GetRole(value.Value).Mention, true);
            }

            await Context.MsgReplyEmbedAsync(emb.Build());
        }
    }
}
