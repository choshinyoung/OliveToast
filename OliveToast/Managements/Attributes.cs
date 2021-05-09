using Discord.Commands;
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
            UseBot, ManageCommand, ChangeAnnouncementChannel, ManageBotSetting, CreateVote, SpeakByBot, ServerAdmin, BotAdmin
        }

        public readonly PermissionType Permission;

        public RequirePermission(PermissionType permission)
        {
            Permission = permission;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RequireCategoryEnable : PreconditionAttribute
    {
        public enum CategoryType
        {
            Default, Info, Search, Game, String, Emoji, Image, Vote, Command, Level, Log, Management, Setting
        }

        public readonly CategoryType Category;

        public RequireCategoryEnable(CategoryType category)
        {
            Category = category;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        public static CategoryType GetCategory(ModuleInfo info)
        {
            return ((RequireCategoryEnable)info.Preconditions.Where(p => p.GetType() == typeof(RequireCategoryEnable)).First()).Category;
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
