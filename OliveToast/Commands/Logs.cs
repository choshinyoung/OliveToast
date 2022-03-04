using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OliveToast.Managements.Data;
using OliveToast.Utilities;
using System.Linq;
using System.Threading.Tasks;
using static OliveToast.Utilities.RequireCategoryEnable;
using static OliveToast.Utilities.RequirePermission;

namespace OliveToast.Commands
{
    [Name("로그")]
    [RequireCategoryEnable(CategoryType.Log), RequireContext(ContextType.Guild)]
    public class Logs : ModuleBase<SocketCommandContext>
    {
        [Command("로그 채널 설정"), Alias("로그 채널")]
        [RequirePermission(PermissionType.ManageBotSetting)]
        [Summary("로그 채널을 설정하는 커맨드예요")]
        public async Task SetLogChannel([Remainder, Name("채널")] SocketTextChannel channel)
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;

            setting.LogChannelId = channel.Id;

            OliveGuild.Set(Context.Guild.Id, g => g.Setting, setting);

            await Context.ReplyEmbedAsync($"로그 채널을 {channel.Name.으로()} 설정했어요");
        }
        
        [Command("로그 채널 확인"), Alias("로그 설정 보기", "로그 채널 보기", "로그 채널")]
        [RequirePermission(PermissionType.ManageBotSetting)]
        [Summary("설정된 로그 채널을 확인할 수 있는 커맨드예요")]
        public async Task SeeLogChannel()
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(Context.Guild.Id).Setting;

            EmbedBuilder emb = Context.CreateEmbed(title: "로그 채널");

            ulong? id = setting.LogChannelId;
            if (!id.HasValue || !Context.Guild.Channels.Any(c => c.Id == id))
            {
                emb.Description = "로그 채널이 설정되지 않았어요";
            }
            else
            {
                emb.Description = $"현재 로그 채널은 {Context.Guild.GetTextChannel(id.Value).Name.이()}에요";
            }

            await Context.ReplyEmbedAsync(emb.Build());
        }
    }
}
