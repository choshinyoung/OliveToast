using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast.Managements
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequirePermission : PreconditionAttribute
    {
        public enum PermissionType
        {
            UseBot, ManageCommand, ManageBotSetting, CreateVote, SpeakByBot, ServerAdmin, BotAdmin
        }

        public static readonly string[] PermissionNames = { "봇 사용", "커맨드 관리", "설정 관리", "투표 생성", "봇으로 말하기", "서버 어드민", "봇 어드민" };

        public readonly PermissionType Permission;

        public RequirePermission(PermissionType permission)
        {
            Permission = permission;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            // whitelist
            if (context.User.Id == 396163884005851137)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            if (Permission != PermissionType.BotAdmin)
            {
                OliveGuild.GuildSetting setting = OliveGuild.Get(context.Guild.Id).Setting;
                SocketGuildUser user = context.User as SocketGuildUser;

                if (context.Guild.OwnerId == user.Id)
                {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                string perm = Permission.ToString();

                if (setting.PermissionRoles.ContainsKey(perm) && context.Guild.Roles.Any(r => r.Id == setting.PermissionRoles[perm]))
                {
                    if (user.Roles.Any(r => r.Position >= context.Guild.GetRole(setting.PermissionRoles[perm]).Position))
                    {
                        return Task.FromResult(PreconditionResult.FromSuccess());
                    }
                }
                else
                {
                    if (Permission is PermissionType.UseBot or PermissionType.SpeakByBot or PermissionType.ManageCommand or PermissionType.CreateVote)
                    {
                        return Task.FromResult(PreconditionResult.FromSuccess());
                    }
                }
            }

            return Task.FromResult(PreconditionResult.FromError($"이 커맨드를 실행하려면 {PermissionToString(Permission)} 권한이 필요해요"));
        }

        public static string PermissionToString(PermissionType type)
        {
            return PermissionNames[(int)type];
        }

        public static PermissionType StringToPermission(string type)
        {
            return (PermissionType)Array.IndexOf(PermissionNames, type);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RequireCategoryEnable : PreconditionAttribute
    {
        public enum CategoryType
        {
            Default, Info, Search, Game, Text, Image, Vote, Command, Level, Log, Setting
        }

        public static readonly string[] CategoryNames = { "일반", "정보", "검색", "게임", "텍스트", "이미지", "투표", "커맨드", "레벨", "로그", "설정" };

        public readonly CategoryType Category;

        public RequireCategoryEnable(CategoryType category)
        {
            Category = category;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            OliveGuild.GuildSetting setting = OliveGuild.Get(context.Guild.Id).Setting;
            if (setting.EnabledCategories.Contains(Category))
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError($"이 커맨드를 실행하려면 {CategoryToString(Category)} 타입의 활성화가 필요해요"));
            }
        }

        public static CategoryType GetCategory(ModuleInfo info)
        {
            return ((RequireCategoryEnable)info.Preconditions.Where(p => p.GetType() == typeof(RequireCategoryEnable)).First()).Category;
        }

        public static string CategoryToString(CategoryType type)
        {
            return CategoryNames[(int)type];
        }

        public static CategoryType StringToCategory(string type)
        {
            return (CategoryType)Array.IndexOf(CategoryNames, type);
        }
    }

    public static class InfoExtension
    {
        public static bool HavePrecondition<T>(this CommandInfo info)
        {
            return info.Preconditions.Any(p => p.GetType() == typeof(T));
        }

        public static bool HavePrecondition<T>(this ModuleInfo info)
        {
            return info.Preconditions.Any(p => p.GetType() == typeof(T));
        }
    }
}
