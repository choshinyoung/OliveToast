using Discord;
using Discord.Commands;
using Discord.WebSocket;
using OliveToast.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OliveToast.Utilities.RequirePermission;

namespace OliveToast.Commands
{
    public class Helps : ModuleBase<SocketCommandContext>
    {
        [Command("도움"), Alias("도움말")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("커맨드의 목록이에요")]
        public async Task Help()
        {
            int page = 1;

            List<ModuleInfo> modules = GetModules();

            ModuleInfo module = modules[page - 1];

            EmbedBuilder emb = Context.CreateEmbed($"\"{CommandEventHandler.prefix}도움 `커맨드`\"로 자세한 사용법 보기", $"{module.Name} 커맨드 도움말");

            List<CommandInfo> cmds = GetDeduplicatedCommandsFromModule(module);

            foreach (CommandInfo info in cmds)
            {
                emb.AddField($"{CommandEventHandler.prefix}{info.Name} {string.Join(' ', info.Parameters.Where(p => p.Name != "").Select(p => $"`{p.Name}`"))}", info.Summary.Split('\n')[0]);
            }

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("<", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.HelpList, 0), disabled: true)
                .WithButton($"1 / {modules.Count}", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.HelpList, -1), ButtonStyle.Secondary)
                .WithButton(">", InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.HelpList, 2));

            List<SelectMenuOptionBuilder> caterogyOptions = modules.Select((m, index) => new SelectMenuOptionBuilder(m.Name, (index + 1).ToString())).ToList();
            component.WithSelectMenu(InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.HelpCategoryListSelectMenu), caterogyOptions, "카테고리 선택하기", row: 1);

            List<SelectMenuOptionBuilder> commandOptions = cmds.Select(c => new SelectMenuOptionBuilder(c.Name, c.Name)).ToList();
            component.WithSelectMenu(InteractionHandler.GenerateCustomId(Context.User.Id, InteractionHandler.InteractionType.HelpCommandListSelectMenu), commandOptions, "커맨드 선택하기", row: 2);

            await Context.ReplyEmbedAsync(emb.Build(), component: component.Build());
        }

        public static async Task ChangeCategoryListPage(ulong userId, SocketUserMessage msg, int page)
        {
            if (page == -1)
            {
                page = 1;
            }

            List<ModuleInfo> modules = GetModules();

            ModuleInfo module = modules[page - 1];

            EmbedBuilder emb = msg.Embeds.First().ToEmbedBuilder();
            emb.Title = $"{module.Name} 커맨드 도움말";
            emb.Fields = new List<EmbedFieldBuilder>();

            List<CommandInfo> cmds = GetDeduplicatedCommandsFromModule(module);

            foreach (CommandInfo info in cmds)
            {
                emb.AddField($"{CommandEventHandler.prefix}{info.Name} {string.Join(' ', info.Parameters.Where(p => p.Name != "").Select(p => $"`{p.Name}`"))}", info.Summary.Split('\n')[0]);
            }

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("<", InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.HelpList, page - 1), disabled: page == 1)
                .WithButton($"{page} / {modules.Count}", InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.HelpList, -1), ButtonStyle.Secondary)
                .WithButton(">", InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.HelpList, page + 1), disabled: page == modules.Count);

            List<SelectMenuOptionBuilder> categoryOptions = modules.Select((m, index) => new SelectMenuOptionBuilder(m.Name, (index + 1).ToString())).ToList();
            component.WithSelectMenu(InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.HelpCategoryListSelectMenu, page), categoryOptions, "카테고리 선택하기", row: 1);

            List<SelectMenuOptionBuilder> commandOptions = cmds.Select(c => new SelectMenuOptionBuilder(c.Name, c.Name)).ToList();
            component.WithSelectMenu(InteractionHandler.GenerateCustomId(userId, InteractionHandler.InteractionType.HelpCommandListSelectMenu), commandOptions, "커맨드 선택하기", row: 2);

            await msg.ModifyAsync(m =>
            {
                m.Embeds = new[] { emb.Build() };
                m.Components = component.Build();
            });
        }

        public static async Task UpdateCommandInfo(SocketUserMessage msg, string name)
        {
            List<CommandInfo> commandInfos = Program.Command.Commands.Where(c => c.Aliases.Contains(name) && c.Summary != null).ToList();

            EmbedBuilder emb = msg.Embeds.First().ToEmbedBuilder();
            emb.Description = null;
            emb.Title = null;
            emb.Fields = new List<EmbedFieldBuilder>();

            foreach (CommandInfo info in commandInfos)
            {
                string aliases = info.Aliases.Count > 1 ? string.Join(" ", info.Aliases.Where(a => a != info.Name).Select(a => $"`{CommandEventHandler.prefix}{a}`")) + "\n" : "";

                string param = "";
                foreach (ParameterInfo paramInfo in info.Parameters)
                {
                    if (paramInfo.Name == "")
                    {
                        continue;
                    }

                    param += $"`{paramInfo.Name}";

                    Type t = paramInfo.Type;
                    if (t == typeof(bool))
                        param += "<bool>";
                    else if (t == typeof(char))
                        param += "<문자>";
                    else if (t == typeof(byte) || t == typeof(sbyte) || t == typeof(short) || t == typeof(int) || t == typeof(long) || t == typeof(ushort) || t == typeof(uint) || t == typeof(ulong) || t == typeof(float) || t == typeof(double) || t == typeof(decimal))
                        param += "<숫자>";
                    else if (t == typeof(string))
                        param += "<텍스트>";
                    else if (t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(TimeSpan))
                        param += "<시간>";
                    else if (typeof(Enum).IsAssignableFrom(t))
                        param += "<enum>";
                    else if (typeof(IUser).IsAssignableFrom(t))
                        param += "<유저>";
                    else if (typeof(IChannel).IsAssignableFrom(t))
                        param += "<채널>";
                    else if (typeof(IRole).IsAssignableFrom(t))
                        param += "<역할>";
                    else if (typeof(IMessage).IsAssignableFrom(t))
                        param += "<메시지>";

                    param += "` ";
                }

                string permission = null;
                if (info.HavePrecondition<RequirePermission>())
                {
                    PermissionType pms = ((RequirePermission)info.Preconditions.Where(p => p.GetType() == typeof(RequirePermission)).FirstOrDefault()).Permission;

                    if (pms != PermissionType.UseBot)
                    {
                        permission = PermissionToString(pms);
                    }
                }
                permission = permission != null ? $"\n - 이 커맨드를 실행하려면 `<{permission}>` 권한이 필요해요" : "";

                emb.AddField($"{CommandEventHandler.prefix}{info.Name} {param}", $"{aliases}\n{info.Summary}{permission}\n​");
            }

            await msg.ModifyAsync(m =>
            {
                m.Embeds = new[] { emb.Build() };
                m.Components = null;
            });
        }

        [Command("도움"), Alias("도움말")]
        [RequirePermission(PermissionType.UseBot)]
        [Summary("특정 카테고리 또는 커맨드의 자세한 정보를 확인할 수 있는 커맨드예요")]
        public async Task Help([Remainder, Name("카테고리/커맨드")] string name)
        {
            List<ModuleInfo> moduleInfos = Program.Command.Modules.Where(m => m.Name == name).ToList();
            if (moduleInfos.Any())
            {
                List<CommandInfo> cmds = GetDeduplicatedCommandsFromModule(moduleInfos.FirstOrDefault());

                EmbedBuilder emb = Context.CreateEmbed(title: moduleInfos.FirstOrDefault().Name);

                foreach (CommandInfo info in cmds)
                {
                    emb.AddField($"{CommandEventHandler.prefix}{info.Name} {string.Join(' ', info.Parameters.Where(p => p.Name != "").Select(p => $"`{p.Name}`"))}", info.Summary.Split('\n')[0]);
                }

                await Context.ReplyEmbedAsync(emb.Build());
            }
            else
            {
                List<CommandInfo> commandInfos = Program.Command.Commands.Where(c => c.Aliases.Contains(name) && c.Summary != null).ToList();
                if (commandInfos.Any())
                {
                    EmbedBuilder emb = Context.CreateEmbed();

                    foreach (CommandInfo info in commandInfos)
                    {
                        string aliases = info.Aliases.Count > 1 ? string.Join(" ", info.Aliases.Where(a => a != info.Name).Select(a => $"`{CommandEventHandler.prefix}{a}`")) + "\n" : "";

                        string param = "";
                        foreach (ParameterInfo paramInfo in info.Parameters)
                        {
                            if (paramInfo.Name == "")
                            {
                                continue;
                            }

                            param += $"`{paramInfo.Name}";

                            Type t = paramInfo.Type;
                            if (t == typeof(bool))
                                param += "<bool>";
                            else if (t == typeof(char))
                                param += "<문자>";
                            else if (t == typeof(byte) || t == typeof(sbyte) || t == typeof(short) || t == typeof(int) || t == typeof(long) || t == typeof(ushort) || t == typeof(uint) || t == typeof(ulong) || t == typeof(float) || t == typeof(double) || t == typeof(decimal))
                                param += "<숫자>";
                            else if (t == typeof(string))
                                param += "<텍스트>";
                            else if (t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(TimeSpan))
                                param += "<시간>";
                            else if (typeof(Enum).IsAssignableFrom(t))
                                param += "<enum>";
                            else if (typeof(IUser).IsAssignableFrom(t))
                                param += "<유저>";
                            else if (typeof(IChannel).IsAssignableFrom(t))
                                param += "<채널>";
                            else if (typeof(IRole).IsAssignableFrom(t))
                                param += "<역할>";
                            else if (typeof(IMessage).IsAssignableFrom(t))
                                param += "<메시지>";

                            param += "` ";
                        }

                        string permission = null;
                        if (info.HavePrecondition<RequirePermission>())
                        {
                            PermissionType pms = ((RequirePermission)info.Preconditions.Where(p => p.GetType() == typeof(RequirePermission)).FirstOrDefault()).Permission;

                            if (pms != PermissionType.UseBot)
                            {
                                permission = PermissionToString(pms);
                            }
                        }
                        permission = permission != null ? $"\n - 이 커맨드를 실행하려면 `<{permission}>` 권한이 필요해요" : "";

                        emb.AddField($"{CommandEventHandler.prefix}{info.Name} {param}", $"{aliases}\n{info.Summary}{permission}\n​");
                    }

                    await Context.ReplyEmbedAsync(emb.Build());
                }
                else
                {
                    await Context.ReplyEmbedAsync("해당 커맨드 또는 카테고리가 없어요");
                }
            }
        }

        private static List<ModuleInfo> GetModules()
        {
            List<ModuleInfo> modules = Program.Command.Modules.Where(m => m.HavePrecondition<RequireCategoryEnable>()).ToList();
            modules.Sort((m1, m2) => RequireCategoryEnable.GetCategory(m1).CompareTo(RequireCategoryEnable.GetCategory(m2)));

            return modules;
        }

        private static List<CommandInfo> GetDeduplicatedCommandsFromModule(ModuleInfo module)
        {
            List<CommandInfo> commands = new();
            foreach (CommandInfo cmd in module.Commands)
            {
                if (cmd.Summary != null && !commands.Any(c => c.Name == cmd.Name))
                    commands.Add(cmd);
            }

            return commands;
        }
    }
}
