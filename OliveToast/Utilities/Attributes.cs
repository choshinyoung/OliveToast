using Discord.Commands;
using Discord.WebSocket;
using OliveToast.Managements.data;
using OliveToast.Managements.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OliveToast.Utilities
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequirePermission : PreconditionAttribute
    {
        public enum PermissionType
        {
            UseBot, ManageCommand, ManageBotSetting, SpeakByBot, ServerAdmin, BotAdmin
        }

        public static readonly string[] PermissionNames = { "봇 사용", "커맨드 관리", "설정 관리", "봇으로 말하기", "서버 어드민", "봇 어드민" };

        public readonly PermissionType Permission;

        public RequirePermission(PermissionType permission)
        {
            Permission = permission;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (SpecialListManager.IsWhiteList(context.User.Id))
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            if (Permission != PermissionType.BotAdmin)
            {
                if ((context as SocketCommandContext).IsPrivate)
                {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                OliveGuild.GuildSetting setting = OliveGuild.Get(context.Guild.Id).Setting;
                SocketGuildUser user = context.User as SocketGuildUser;

                if (user.GuildPermissions.Administrator)
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
                    if (Permission is PermissionType.UseBot or PermissionType.SpeakByBot)
                    {
                        return Task.FromResult(PreconditionResult.FromSuccess());
                    }
                }
            }

            return Task.FromResult(PreconditionResult.FromError(new RequirePermissionException(Permission)));
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
            Default, Info, Search, Game, Text, Image, Command, Level, Log, Setting
        }

        public static readonly string[] CategoryNames = { "일반", "정보", "검색", "게임", "텍스트", "이미지", "커맨드", "레벨", "로그", "설정" };

        public readonly CategoryType Category;

        public RequireCategoryEnable(CategoryType category)
        {
            Category = category;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if ((context as SocketCommandContext).IsPrivate)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            OliveGuild.GuildSetting setting = OliveGuild.Get(context.Guild.Id).Setting;
            if (setting.EnabledCategories.Contains(Category))
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError(new CategoryNotEnabledException(Category)));
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

    public class RequirePermissionException : Exception
    {
        public const string Emoji = "<:PermissionDenied:907628900266434620>";

        public RequirePermissionException(RequirePermission.PermissionType perm) : base($"{Emoji} 이 커맨드를 실행하려면 {RequirePermission.PermissionToString(perm)} 권한이 필요해요")
        {

        }
    }

    public class CategoryNotEnabledException : Exception
    {
        public const string Emoji = "<:CategoryNotEnabled:907628900564209705>";

        public CategoryNotEnabledException(RequireCategoryEnable.CategoryType cat) : base($"{Emoji} 이 커맨드를 실행하려면 {RequireCategoryEnable.CategoryToString(cat)} 타입의 활성화가 필요해요")
        {

        }
    }
}
