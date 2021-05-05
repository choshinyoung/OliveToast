using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast.Managements
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    class HideInHelp : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    class RequirePermission : PreconditionAttribute
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
    class RequireCategoryEnable : PreconditionAttribute
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
    }

    public static class InfoExtension
    {
        public static bool HaveAttribute<T>(this CommandInfo info)
        {
            return info.Attributes.Where(a => a.GetType() == typeof(T)).Any();
        }

        public static bool HaveAttribute<T>(this ModuleInfo info)
        {
            return info.Attributes.Where(a => a.GetType() == typeof(T)).Any();
        }

        public static bool HavePrecondition<T>(this CommandInfo info)
        {
            return info.Preconditions.Where(p => p.GetType() == typeof(T)).Any();
        }

        public static bool HavePrecondition<T>(this ModuleInfo info)
        {
            return info.Attributes.Where(p => p.GetType() == typeof(T)).Any();
        }
    }
}
