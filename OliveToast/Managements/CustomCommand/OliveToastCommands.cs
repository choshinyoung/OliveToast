using Discord;
using Discord.WebSocket;
using OliveToast.Managements.data;
using OliveToast.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Toast;
using Toast.Nodes;

namespace OliveToast.Managements.CustomCommand
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

            ToastCommand.CreateFunc<CustomCommandContext, SocketUserMessage>("userMessage", (ctx) => ctx.Message),
            ToastCommand.CreateFunc<CustomCommandContext, SocketUserMessage>("botMessage", (ctx) => ctx.BotMessage),
            ToastCommand.CreateFunc<CustomCommandContext, SocketUserMessage>("userLastMessage", (ctx) => ctx.UserLastMessage),
            ToastCommand.CreateFunc<CustomCommandContext, SocketUserMessage>("botLastMessage", (ctx) => ctx.BotLastMessage),

            ToastCommand.CreateFunc<CustomCommandContext, object[]>("users", (ctx) => ctx.Guild.Users.Select(u => (object)u).ToArray()),
            ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser>("user", (ctx) => ctx.User),

            ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel>("channel", (ctx) => ctx.Channel),
            ToastCommand.CreateFunc<CustomCommandContext, object[]>("channels", (ctx) => ctx.Guild.TextChannels.Select(u => (object)u).ToArray()),

            ToastCommand.CreateFunc<CustomCommandContext, object[]>("roles", (ctx) => ctx.Guild.Roles.Select(u => (object)u).ToArray()),
            ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser, object[]>("rolesOf", (ctx, x) => x.Roles.Select(u => (object)u).ToArray()),

            ToastCommand.CreateFunc<CustomCommandContext, EmbedBuilder>("embed", (ctx) => ctx.User.CreateEmbed(false)),
            ToastCommand.CreateFunc<EmbedBuilder, CustomCommandContext, VariableNode, object, EmbedBuilder>("with", (x, ctx, y, z) =>
            {
                switch (y.Name)
                {
                    case "title":
                        return x.WithTitle(ctx.Toaster.ExecuteConverter<string>(z, ctx));
                    case "description":
                        return x.WithDescription(ctx.Toaster.ExecuteConverter<string>(z, ctx));
                    case "color":
                        var color = System.Drawing.ColorTranslator.FromHtml(ctx.Toaster.ExecuteConverter<string>(z, ctx));

                        return x.WithColor(new(color.R, color.G, color.B));
                    case "image":
                        return x.WithImageUrl(ctx.Toaster.ExecuteConverter<string>(z, ctx));
                    case "author":
                        return x.WithAuthor(ctx.Toaster.ExecuteConverter<SocketGuildUser>(z, ctx));
                    case "url":
                        return x.WithUrl(ctx.Toaster.ExecuteConverter<string>(z, ctx));
                    case "thumbnail":
                        return x.WithThumbnailUrl(ctx.Toaster.ExecuteConverter<string>(z, ctx));
                    case "field":
                        object[] field = ctx.Toaster.ExecuteConverter<object[]>(z, ctx);
                        if (field.Length is not 2 and not 3)
                        {
                            throw new Exception("잘못된 필드값이에요");
                        }

                        x.AddField(ctx.Toaster.ExecuteConverter<string>(field[0], ctx), field[1], field.Length == 3 && ctx.Toaster.ExecuteConverter<bool>(field[2], ctx));

                        return x;
                    default:
                        throw new Exception("잘못된 속성 키예요");
                }
            }),

            ToastCommand.CreateAction<CustomCommandContext, object>("send", (ctx, x) =>
            {
                if (ctx.SendCount >= 5)
                {
                    throw new Exception("메시지를 너무 많이 보내고있어요!");
                }

                ulong msgId;
                if (x is EmbedBuilder emb)
                {
                    msgId = ctx.Channel.SendMessageAsync(embed: emb.Build(), allowedMentions: AllowedMentions.None).GetAwaiter().GetResult().Id;
                }
                else
                {
                    msgId = ctx.Channel.SendMessageAsync(ctx.Toaster.ExecuteConverter<string>(x, ctx), allowedMentions: AllowedMentions.None).GetAwaiter().GetResult().Id;
                }
                ctx.SendCount++;

                ctx.BotLastMessage = ctx.Channel.GetMessageAsync(msgId).GetAwaiter().GetResult() as SocketUserMessage;
                ctx.ContextMessages.Add(msgId);
            }, -1),
            ToastCommand.CreateAction<CustomCommandContext, SocketTextChannel, object>("sendTo", (ctx, x, y) =>
            {
                if (ctx.SendCount >= 5)
                {
                    throw new Exception("메시지를 너무 많이 보내고있어요!");
                }

                ulong msgId;
                if (y is EmbedBuilder emb)
                {
                    msgId = x.SendMessageAsync(embed: emb.Build(), allowedMentions: AllowedMentions.None).GetAwaiter().GetResult().Id;
                }
                else
                {
                    msgId = x.SendMessageAsync(ctx.Toaster.ExecuteConverter<string>(y, ctx), allowedMentions: AllowedMentions.None).GetAwaiter().GetResult().Id;
                }
                ctx.SendCount++;

                ctx.BotLastMessage = ctx.Channel.GetMessageAsync(msgId).GetAwaiter().GetResult() as SocketUserMessage;
                ctx.ContextMessages.Add(msgId);
            }, -1),
            ToastCommand.CreateAction<CustomCommandContext, SocketUserMessage, object>("reply", (ctx, x, y) =>
            {
                if (ctx.SendCount >= 5)
                {
                    throw new Exception("메시지를 너무 많이 보내고있어요!");
                }

                ulong msgId;
                if (y is EmbedBuilder emb)
                {
                    msgId = x.ReplyAsync(embed: emb.Build(), allowedMentions: AllowedMentions.None).GetAwaiter().GetResult().Id;
                }
                else
                {
                    msgId = x.ReplyAsync(ctx.Toaster.ExecuteConverter<string>(y, ctx), allowedMentions: AllowedMentions.None).GetAwaiter().GetResult().Id;
                }
                ctx.SendCount++;

                ctx.BotLastMessage = ctx.Channel.GetMessageAsync(msgId).GetAwaiter().GetResult() as SocketUserMessage;
                ctx.ContextMessages.Add(msgId);
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

            ToastCommand.CreateFunc<VariableNode, CustomCommandContext, object, object>("of", (x, ctx, y) =>
            {
                if (!(y is SocketGuildUser or SocketTextChannel or SocketRole))
                {
                    object converted;

                    converted = ctx.Toaster.ExecuteConverter(Converters.First(c => c.From == typeof(string) && c.To == typeof(SocketGuildUser)), y.ToString(), ctx);

                    if (converted is null)
                    {
                        converted = ctx.Toaster.ExecuteConverter(Converters.First(c => c.From == typeof(string) && c.To == typeof(SocketTextChannel)), y.ToString(), ctx);

                        if (converted is null)
                        {
                            converted = ctx.Toaster.ExecuteConverter(Converters.First(c => c.From == typeof(string) && c.To == typeof(SocketRole)), y.ToString(), ctx);

                            if (converted is null)
                            {
                                throw new Exception("알 수 없는 타입이이에요");
                            }
                        }
                    }

                    y = converted;
                }

                return y switch
                {
                    SocketGuildUser user => x.Name switch
                    {
                        "name" => user.Username,
                        "id" => user.Id,
                        "tag" => user.DiscriminatorValue,
                        "nickname" => user.GetName(false),
                        "isBot" => user.IsBot,
                        "mention" => user.Mention,
                        _ => throw new Exception("잘못된 속성 키예요")
                    },
                    SocketTextChannel channel => x.Name switch
                    {
                        "name" => channel.Name,
                        "id" => channel.Id,
                        "category" => channel.Category is not null ? channel.Category.Name : "-",
                        "isNsfw" => channel.IsNsfw,
                        "mention" => channel.Mention,
                        "slowMode" => channel.SlowModeInterval,
                        _ => throw new Exception("잘못된 속성 키예요")
                    },
                    SocketRole role => x.Name switch
                    {
                        "name" => role.Name,
                        "id" => role.Id,
                        "isHoisted" => role.IsHoisted,
                        "isMentionable" => role.IsMentionable,
                        "mention" => role.Mention,
                        _ => throw new Exception("잘못된 속성 키예요")
                    },
                    _ => throw new Exception("알 수 없는 타입이이에요"),
                };
                ;
            }, -1),

            ToastCommand.CreateFunc<CustomCommandContext, string>("serverName", (ctx) => ctx.Guild.Name),
            ToastCommand.CreateFunc<CustomCommandContext, ulong>("serverId", (ctx) => ctx.Guild.Id),
            ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel>("systemChannel", (ctx) => ctx.Guild.SystemChannel),
            ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel>("ruleChannel", (ctx) => ctx.Guild.RulesChannel),
            ToastCommand.CreateFunc<CustomCommandContext, SocketTextChannel>("publicUpdateChannel", (ctx) => ctx.Guild.PublicUpdatesChannel),
            ToastCommand.CreateFunc<CustomCommandContext, SocketGuildUser>("owner", (ctx) => ctx.Guild.Owner),
            ToastCommand.CreateFunc<CustomCommandContext, int>("boostCount", (ctx) => ctx.Guild.PremiumSubscriptionCount),
            ToastCommand.CreateFunc<CustomCommandContext, int>("boostLevel", (ctx) => (int)ctx.Guild.PremiumTier),
            ToastCommand.CreateFunc<CustomCommandContext, bool>("isMfaEnabled", (ctx) => ctx.Guild.MfaLevel == MfaLevel.Enabled),

            ToastCommand.CreateAction<CustomCommandContext, SocketGuildUser, string>("dm", (ctx, x, y) =>
            {
                if (ctx.isDmSent)
                {
                    throw new Exception("메시지를 너무 많이 보내고있어요!");
                }

                x.SendMessageAsync(y).Wait();

                ctx.isDmSent = true;
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

            ToastCommand.CreateFunc<CustomCommandContext, string, object>("get", (ctx, x) => OliveGuild.Get(ctx.Guild.Id).CommandDb[x]),
            ToastCommand.CreateAction<CustomCommandContext, string, object>("set", (ctx, x, y) =>
            {
                Dictionary<string, object> db = OliveGuild.Get(ctx.Guild.Id).CommandDb;

                if (db.ContainsKey(x))
                {
                    db[x] = y;
                }
                else
                {
                    if (db.Count > 100)
                    {
                        throw new Exception("데이터베이스에 저장된 값이 너무 많아요");
                    }

                    db.Add(x, y);
                }

                OliveGuild.Set(ctx.Guild.Id, guild => guild.CommandDb, db);
            }),
            ToastCommand.CreateAction<CustomCommandContext, string>("deleteKey", (ctx, x) =>
            {
                Dictionary<string, object> db = OliveGuild.Get(ctx.Guild.Id).CommandDb;

                db.Remove(x);

                OliveGuild.Set(ctx.Guild.Id, guild => guild.CommandDb, db);
            }),
            ToastCommand.CreateFunc<CustomCommandContext, object[]>("dbKeys", (ctx) => OliveGuild.Get(ctx.Guild.Id).CommandDb.Keys.Select(k => (object)k).ToArray()),
        };

        public static readonly List<ToastConverter> Converters = new()
        {
            ToastConverter.Create<string, SocketGuildUser>((_ctx, x) =>
            {
                CustomCommandContext ctx = (CustomCommandContext)_ctx;

                var user = ctx.Guild.Users.ToList().Find(u => u.Username.ToLower() == x.ToLower() || (u.Nickname is not null && u.Nickname.ToLower() == x.ToLower()));
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

                var channel = ctx.Guild.TextChannels.ToList().Find(u => u.Name.ToLower() == x.ToLower());
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

                var role = ctx.Guild.Roles.ToList().Find(u => u.Name.ToLower() == x.ToLower());
                if (role is not null)
                {
                    return role;
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
