using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Toast;
using Toast.Nodes;

namespace OliveToast.Managements
{
    public class OliveToastCommands
    {
        public static readonly List<ToastCommand> Commands = new()
        {
            ToastCommand.CreateFunc<object, ToastContext, object, bool>("is", (x, ctx, y) =>
            {
                if (x.GetType() == typeof(SocketTextChannel) && y.GetType() == typeof(SocketTextChannel))
                {
                    return ((SocketTextChannel)x).Id == ((SocketTextChannel)y).Id;
                }
                if (x.GetType() == typeof(SocketUserMessage) && y.GetType() == typeof(SocketUserMessage))
                {
                    return ((SocketUserMessage)x).Id == ((SocketUserMessage)y).Id;
                }
                if (x.GetType() == typeof(SocketGuildUser) && y.GetType() == typeof(SocketGuildUser))
                {
                    return ((SocketGuildUser)x).Id == ((SocketGuildUser)y).Id;
                }
                if (x.GetType() == typeof(SocketRole) && y.GetType() == typeof(SocketRole))
                {
                    return ((SocketRole)x).Id == ((SocketRole)y).Id;
                }

                return x.Equals(y);
            }, 9),

            ToastCommand.CreateAction<CustomCommandContext, string>("send", (ctx, x) =>
            {
                if (ctx.SendCount >= 5)
                {
                    throw new Exception("메시지를 너무 많이 보내고있어요!");
                }

                ulong msgId = ctx.Channel.SendMessageAsync(x, allowedMentions: AllowedMentions.None).GetAwaiter().GetResult().Id;
                ctx.BotLastMessage = ctx.Channel.GetMessageAsync(msgId).GetAwaiter().GetResult() as SocketUserMessage;
                ctx.ContextMessages.Add(msgId);

                ctx.SendCount++;
            }, -1),
            ToastCommand.CreateAction<CustomCommandContext, SocketUserMessage, string>("reply", (ctx, x, y) =>
            {
                if (ctx.SendCount >= 5)
                {
                    throw new Exception("메시지를 너무 많이 보내고있어요!");
                }

                ulong msgId = x.ReplyAsync(y, allowedMentions: AllowedMentions.None).GetAwaiter().GetResult().Id;
                ctx.BotLastMessage = ctx.Channel.GetMessageAsync(msgId).GetAwaiter().GetResult() as SocketUserMessage;
                ctx.ContextMessages.Add(msgId);
                ctx.SendCount++;
            }, -1),
            ToastCommand.CreateAction<CustomCommandContext, SocketUserMessage, string>("edit", (ctx, x, y) =>
            {
                if (x.Author.Id != Program.Client.CurrentUser.Id)
                {
                    throw new Exception("다른 유저가 보낸 메시지는 수정할 수 없어요");
                }

                x.ModifyAsync(msg => msg.Content = y).Wait();
            }),
            ToastCommand.CreateAction<CustomCommandContext, SocketUserMessage>("delete", (ctx, x) => x.DeleteAsync().Wait()),
            ToastCommand.CreateAction<CustomCommandContext, SocketUserMessage, string>("react", (ctx, x, y)
                => x.AddReactionAsync(Emote.TryParse(y, out var result) ? result : new Emoji(y)).Wait(), -1),

            ToastCommand.CreateFunc<CustomCommandContext, SocketUserMessage>("userMessage", (ctx) => ctx.Message),
            ToastCommand.CreateFunc<CustomCommandContext, SocketUserMessage>("botMessage", (ctx) => ctx.BotMessage),
            ToastCommand.CreateFunc<CustomCommandContext, SocketUserMessage>("userLastMessage", (ctx) => ctx.UserLastMessage),
            ToastCommand.CreateFunc<CustomCommandContext, SocketUserMessage>("botLastMessage", (ctx) => ctx.BotLastMessage),

            ToastCommand.CreateFunc<CustomCommandContext, object[]>("users", (ctx) => ctx.Guild.Users.Select(u => (object)u).ToArray()),
            ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser>("user", (ctx) => ctx.User as SocketGuildUser),
            ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, string>("username", (ctx, x) => x.Username),
            ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, ulong>("userId", (ctx, x) => x.Id),
            ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, int>("tag", (ctx, x) => x.DiscriminatorValue),
            ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, string>("nickname", (ctx, x) => x.GetName(false)),
            ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, bool>("isBot", (ctx, x) => x.IsBot),
            ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, string>("userMention", (ctx, x) => x.Mention),
            ToastCommand.CreateAction<CustomCommandContext, SocketGuildUser, string>("dm", (ctx, x, y) =>
            {
                if (ctx.SendCount >= 5)
                {
                    throw new Exception("메시지를 너무 많이 보내고있어요!");
                }

                x.SendMessageAsync(y).Wait();

                ctx.SendCount++;
            }),
            ToastCommand.CreateAction<CustomCommandContext, SocketGuildUser>("kick", (ctx, x) =>
            {
                if (!ctx.CanKickUser)
                {
                    throw new Exception("권한이 없어 kick 커맨드를 사용할 수 없어요");
                }

                x.KickAsync().Wait();
            }),
            ToastCommand.CreateAction<CustomCommandContext, SocketGuildUser>("ban", (ctx, x) =>
            {
                if (!ctx.CanBanUser)
                {
                    throw new Exception("권한이 없어 ban 커맨드를 사용할 수 없어요");
                }

                x.BanAsync().Wait();
            }),
            ToastCommand.CreateAction<CustomCommandContext, SocketGuildUser, SocketRole>("addRole", (ctx, x, y) =>
            {
                if (!ctx.CanManageRole)
                {
                    throw new Exception("권한이 없어 addRole 커맨드를 사용할 수 없어요");
                }

                x.AddRoleAsync(y).Wait();
            }),
            ToastCommand.CreateAction<CustomCommandContext, SocketGuildUser, SocketRole>("removeRole", (ctx, x, y) =>
            {
                if (!ctx.CanManageRole)
                {
                    throw new Exception("권한이 없어 removeRole 커맨드를 사용할 수 없어요");
                }

                x.RemoveRoleAsync(y).Wait();
            }),

            ToastCommand.CreateFunc<CustomCommandContext, object[]>("channels", (ctx) => ctx.Guild.TextChannels.Select(u => (object) u).ToArray()),
            ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel>("channel", (ctx) => ctx.Channel as SocketTextChannel),
            ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel, string>("channelName", (ctx, x) => x.Name),
            ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel, ulong>("channelId", (ctx, x) => x.Id),
            ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel, string>("category", (ctx, x) => x.Category is not null ? x.Category.Name : "-"),
            ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel, bool>("isNsfw", (ctx, x) => x.IsNsfw),
            ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel, string>("channelMention", (ctx, x) => x.Mention),
            ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel, int>("slowMode", (ctx, x) => x.SlowModeInterval),

            ToastCommand.CreateFunc<CustomCommandContext, object[]>("roles", (ctx) => ctx.Guild.Roles.Select(u => (object) u).ToArray()),
            ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, object[]>("rolesOf", (ctx, x) => x.Roles.Select(u => (object) u).ToArray()),
            ToastCommand.CreateFunc<CustomCommandContext, SocketRole, string>("roleName", (ctx, x) => x.Name),
            ToastCommand.CreateFunc<CustomCommandContext, SocketRole, ulong>("roleId", (ctx, x) => x.Id),
            ToastCommand.CreateFunc<CustomCommandContext, SocketRole, bool>("isHoisted", (ctx, x) => x.IsHoisted),
            ToastCommand.CreateFunc<CustomCommandContext, SocketRole, bool>("isMentionable", (ctx, x) => x.IsMentionable),
            ToastCommand.CreateFunc<CustomCommandContext, SocketRole, string>("roleMention", (ctx, x) => x.Mention),

            ToastCommand.CreateFunc<CustomCommandContext, string>("serverName", (ctx) => ctx.Guild.Name),
            ToastCommand.CreateFunc<CustomCommandContext, ulong>("serverId", (ctx) => ctx.Guild.Id),
            ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel>("systemChannel", (ctx) => ctx.Guild.SystemChannel),
            ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel>("ruleChannel", (ctx) => ctx.Guild.RulesChannel),
            ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel>("publicUpdateChannel", (ctx) => ctx.Guild.PublicUpdatesChannel),
            ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser>("owner", (ctx) => ctx.Guild.Owner),
            ToastCommand.CreateFunc<CustomCommandContext, int>("boostCount", (ctx) => ctx.Guild.PremiumSubscriptionCount),
            ToastCommand.CreateFunc<CustomCommandContext, int>("boostLevel", (ctx) => (int) ctx.Guild.PremiumTier),
            ToastCommand.CreateFunc<CustomCommandContext, bool>("isMfaEnabled", (ctx) => ctx.Guild.MfaLevel == MfaLevel.Enabled),

            ToastCommand.CreateFunc<CustomCommandContext, int, string>("group", (ctx, x) => ctx.Groups[x]),
            ToastCommand.CreateAction<CustomCommandContext, int>("wait", (ctx, x) =>
            {
                if (x > 600 || x < 0)
                {
                    throw new Exception("대기 시간이 너무 길어요!");
                }

                Task.Delay(x * 1000).Wait();
            }),

            ToastCommand.CreateAction<CustomCommandContext>("exit", (ctx) =>
            {
                if (CommandExecuteSession.Sessions.ContainsKey(ctx.User.Id))
                {
                    CommandExecuteSession.Sessions.Remove(ctx.User.Id);
                }
            }),

            ToastCommand.CreateAction<CustomCommandContext, string, FunctionNode>("addEventListener", (ctx, x, y) =>
            {
                switch (x)
                {
                    case "messageReceive":
                        ctx.OnMessageReceived = y;

                        break;
                    case "reactionAdd":
                        ctx.OnReactionAdded = y;

                        break;
                }
            }),
        };

        public static readonly List<ToastConverter> Converters = new()
        {
            ToastConverter.Create<string, SocketGuildUser>((_ctx, x) =>
            {
                CustomCommandContext ctx = (CustomCommandContext)_ctx;

                var user = ctx.Guild.Users.ToList().Find(u => u.Username == x || (u.Nickname is not null && u.Nickname == x));
                if (user is not null)
                {
                    return user;
                }

                Match match = new Regex("<@!([0-9]+)>").Match(x);
                if (match.Success)
                {
                    return ctx.Guild.GetUser(ulong.Parse(match.Groups[1].Value));
                }

                if (x.All(c => char.IsDigit(c)))
                {
                    return ctx.Guild.GetUser(ulong.Parse(x));
                }

                return null;
            }),
            ToastConverter.Create<ulong, SocketGuildUser>((_ctx, x) =>
            {
                CustomCommandContext ctx = (CustomCommandContext)_ctx;

                return ctx.Guild.GetUser(x);
            }),
            ToastConverter.Create<string, SocketTextChannel>((_ctx, x) =>
            {
                CustomCommandContext ctx = (CustomCommandContext)_ctx;

                var channel = ctx.Guild.TextChannels.ToList().Find(u => u.Name == x);
                if (channel is not null)
                {
                    return channel;
                }

                Match match = new Regex("<#([0-9]+)>").Match(x);
                if (match.Success)
                {
                    return ctx.Guild.GetTextChannel(ulong.Parse(match.Groups[1].Value));
                }

                if (x.All(c => char.IsDigit(c)))
                {
                    return ctx.Guild.GetTextChannel(ulong.Parse(x));
                }

                return null;
            }),
            ToastConverter.Create<ulong, SocketTextChannel>((_ctx, x) =>
            {
                CustomCommandContext ctx = (CustomCommandContext)_ctx;

                return ctx.Guild.GetTextChannel(x);
            }),
            ToastConverter.Create<string, SocketRole>((_ctx, x) =>
            {
                CustomCommandContext ctx = (CustomCommandContext)_ctx;

                var channel = ctx.Guild.Roles.ToList().Find(u => u.Name.ToLower() == x.ToLower());
                if (channel is not null)
                {
                    return channel;
                }

                Match match = new Regex("<#([0-9]+)>").Match(x);
                if (match.Success)
                {
                    return ctx.Guild.GetRole(ulong.Parse(match.Groups[1].Value));
                }

                if (x.All(c => char.IsDigit(c)))
                {
                    return ctx.Guild.GetRole(ulong.Parse(x));
                }

                return null;
            }),
            ToastConverter.Create<ulong, SocketRole>((_ctx, x) =>
            {
                CustomCommandContext ctx = (CustomCommandContext)_ctx;

                return ctx.Guild.GetRole(x);
            }),
        };
    }
}
