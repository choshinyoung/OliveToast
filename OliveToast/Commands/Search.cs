using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using OliveToast.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static OliveToast.Utilities.KoreanBotsResult;
using static OliveToast.Utilities.RequireCategoryEnable;
using static OliveToast.Utilities.RequirePermission;

namespace OliveToast.Commands
{
    [Name("검색")]
    [RequireCategoryEnable(CategoryType.Search)]
    public class Search : ModuleBase<SocketCommandContext>
    {
        [Command("디코봇"), Alias("코리안봇", "디스코드봇")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("[한국 디스코드 봇 리스트](https://koreanbots.dev/)에서 봇을 검색할 수 있는 커맨드예요")]
        public async Task DiscordBot([Remainder, Name("봇")] string name)
        {
            using WebClient wc = new();

            EmbedBuilder emb;

            string str = "";
            try
            {
                str = wc.DownloadString($"https://koreanbots.dev/api/v2/search/bots?query={name}");
            }
            catch
            {
                emb = Context.CreateEmbed(title: "검색 실패", description: "해당 봇을 찾을 수 없어요");
                await Context.ReplyEmbedAsync(emb.Build());
                return;
            }

            KoreanBotsResult response = JsonConvert.DeserializeObject<KoreanBotsResult>(str);

            InnerData b = response.data.data.First();

            emb = Context.CreateEmbed(title: b.name, url: $"https://koreanbots.dev/bots/{b.id}", thumbnailUrl: $"https://koreanbots.dev/api/image/discord/avatars/{b.id}.gif?size=512");

            emb.AddField("상태", b.status switch 
            {
                "online" => "<:online:708147696879272027>",
                "idle" => "<:idle:708147696807968842>",
                "dnd" => "<:dnd:708147696976003092>",
                "offline" => "<:offline:708147696523018255>",
                _ => ":question:"
            }, true);
            emb.AddField("카테고리", string.Join(' ', b.category.Select(c => $"`{c}`")), true);
            if (b.flags != Flag.None)
            {
                string badges = "";
                if ((b.flags & Flag.Official) == Flag.Official)
                {
                    badges += "`공식 봇` ";
                }
                if ((b.flags & Flag.KoreanbotVerified) == Flag.KoreanbotVerified)
                {
                    badges += "`한디리 인증된 봇` ";
                }
                if ((b.flags & Flag.Partner) == Flag.Partner)
                {
                    badges += "`파트너` ";
                }
                if ((b.flags & Flag.DiscordVerified) == Flag.DiscordVerified)
                {
                    badges += "`인증된 봇` ";
                }
                if ((b.flags & Flag.Premium) == Flag.Premium)
                {
                    badges += "`프리미엄` ";
                }
                if ((b.flags & Flag.HackatonWinner) == Flag.HackatonWinner)
                {
                    badges += "`제 1회 한디리 해커톤 우승자 봇` ";
                }

                emb.AddField("배지", badges, true);
            }

            emb.AddField("설명", b.intro);

            emb.AddField("제작자", string.Join(", ", b.owners.Select(o => $"`{o.username}#{o.tag}`")), true);
            emb.AddField("라이브러리", $"`{b.lib}`", true);
            emb.AddField("접두사", $"`{b.prefix}`", true);

            emb.AddField("서버 수", $"{b.servers} 서버", true);
            emb.AddField("하트 수", $":heart: {b.votes}", true);
            emb.AddField("초대하기", $"[초대 링크]({b.url})", true);

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("스크래치 유저")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("스크래치에서 해당 유저를 검색할 수 있는 커맨드예요")]
        public async Task ScratchUser([Name("유저네임")] string name)
        {
            using WebClient wc = new();

            EmbedBuilder emb;

            string apiResponse;
            try
            {
                apiResponse = wc.DownloadString($"https://api.scratch.mit.edu/users/{name}");
            }
            catch (WebException)
            {
                emb = Context.CreateEmbed(title: "해당 유저를 찾을 수 없어요", description: "404. 오 이런! 올리브토스트가 머리를 스크래칭하고 있군요");
                await Context.ReplyEmbedAsync(emb.Build());
                return;
            }

            ScratchApiUserResult apiResult = JsonConvert.DeserializeObject<ScratchApiUserResult>(wc.DownloadString($"https://api.scratch.mit.edu/users/{name}"));

            emb = Context.CreateEmbed(title: apiResult.username, url: $"https://scratch.mit.edu/users/{name}", thumbnailUrl: $"https://cdn2.scratch.mit.edu/get_image/user/{apiResult.id}_90x90.png");

            // emb.AddField("스크래쳐", (dbResult.status == "Scratcher").ToEmoji(), true);
            emb.AddField("가입일", ((DateTimeOffset)apiResult.history.joined).ToShortKSTString(), true);
            emb.AddField("국가", apiResult.profile.country, true);

            // emb.AddField("팔로잉", $"{dbResult.statistics?.following.ToString() ?? "?"}명", true);
            // emb.AddField("팔로워", $"{dbResult.statistics?.followers.ToString() ?? "?"}명", true);
            // emb.AddField("팔로워 순위", $"{dbResult.statistics?.ranks.country.followers.ToString() ?? "?"}위", true);

            // emb.AddField("전체 조회수", $"{dbResult.statistics?.views.ToString() ?? "?"}번", true);
            // emb.AddField("전체 :heart:", $"{dbResult.statistics?.loves.ToString() ?? "?"}개", true);
            // emb.AddField("전체 :star:", $"{dbResult.statistics?.favorites.ToString() ?? "?"}개", true);

            emb.AddField("내 소개", $"```\n{apiResult.profile.bio}```");
            emb.AddField("내가 하고 있는 일", $"```\n{apiResult.profile.status}```");

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("스크래치 프로젝트")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("스크래치에서 해당 프로젝트를 검색할 수 있는 커맨드예요")]
        public async Task ScratchProject([Remainder, Name("검색어")] string keyword)
        {
            using WebClient wc = new();
            List<ScratchProjectResult> projects = JsonConvert.DeserializeObject<List<ScratchProjectResult>>(wc.DownloadString($"https://api.scratch.mit.edu/search/projects?q={keyword}"));

            EmbedBuilder emb;

            if (projects.Count == 0)
            {
                emb = Context.CreateEmbed(title: "검색 실패", description: "404. 오 이런! 올리브토스트가 머리를 스크래칭하고 있군요");
                await Context.ReplyEmbedAsync(emb.Build());
                return;
            }

            ScratchProjectResult p = JsonConvert.DeserializeObject<ScratchProjectResult>(wc.DownloadString($"https://api.scratch.mit.edu/projects/{projects.First().id}"));

            emb = Context.CreateEmbed(title: p.title, url: $"https://scratch.mit.edu/projects/{p.id}", thumbnailUrl: p.image);

            emb.AddField("제작자", $"[{p.author.username}](https://scratch.mit.edu/users/{p.author.username})", true);
            emb.AddField("공유일", ((DateTimeOffset)p.history.shared).ToShortKSTString(), true);
            emb.AddField("조회수", $"{p.stats.views}번", true);

            emb.AddField(":heart:", $"{p.stats.loves}", true);
            emb.AddField(":star:", $"{p.stats.favorites}", true);
            emb.AddField(":cyclone:", $"{p.stats.remixes}번", true);

            emb.AddField("사용 방법", $"```\n{p.instructions}```");
            emb.AddField("참고사항 및 참여자", $"```\n{p.description}```");

            await Context.ReplyEmbedAsync(emb.Build());
        }

        [Command("마크 유저"), Alias("마인크래프트 유저", "마인크래프트", "마크", "마크 정보")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("마인크래프트 유저를 검색할 수 있는 커맨드예요")]
        public async Task MinecraftUser([Name("유저")] string name)
        {
            using WebClient wc = new();

            MinecraftUuidResult uuid;
            List<MinecraftNameResult> names;
            MinecraftProfileResult profile;
            MinecraftSkinResult skin;

            EmbedBuilder emb;

            try
            {
                uuid = JsonConvert.DeserializeObject<MinecraftUuidResult>(wc.DownloadString($"https://api.mojang.com/users/profiles/minecraft/{name}"));
                names = JsonConvert.DeserializeObject<List<MinecraftNameResult>>(wc.DownloadString($"https://api.mojang.com/user/profiles/{uuid.id}/names"));
                profile = JsonConvert.DeserializeObject<MinecraftProfileResult>(wc.DownloadString($"https://sessionserver.mojang.com/session/minecraft/profile/{uuid.id}"));
                skin = JsonConvert.DeserializeObject<MinecraftSkinResult>(Encoding.UTF8.GetString(Convert.FromBase64String(profile.properties.First().value)));
            }
            catch
            {
                emb = Context.CreateEmbed(title: "검색 실패", description: "해당 유저를 찾을 수 없어요");
                await Context.ReplyEmbedAsync(emb.Build());
                return;
            }

            emb = Context.CreateEmbed(title: uuid.name, thumbnailUrl: skin.textures.SKIN.url, imgUrl: $"https://visage.surgeplay.com/full/{uuid.id}");

            emb.AddField("UUID", $"{uuid.id}\n{uuid.id[..8]}-{uuid.id[8..12]}-{uuid.id[12..16]}-{uuid.id[16..20]}-{uuid.id[20..]}");

            emb.AddField("UUIDMost", Convert.ToInt64(uuid.id[..16], 16), true);
            emb.AddField("UUIDLeast", Convert.ToInt64(uuid.id[16..], 16), true);

            emb.AddField("닉네임 변경 역사", string.Join('\n', names.Select(n => $"`{n.name}`: {(n.changedToAt != null ? DateTimeOffset.FromUnixTimeMilliseconds(n.changedToAt.Value).ToShortKSTString() : "?")}")));

            await Context.ReplyEmbedAsync(emb.Build());
        }
    }
}
