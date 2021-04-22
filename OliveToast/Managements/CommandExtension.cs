using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast.Managements
{
    public static class CommandExtension
    {
        public static async Task<IUserMessage> MsgReplyAsync(this SocketCommandContext context, object content)
        {
            return await context.Message.ReplyAsync(text: content.ToString(), allowedMentions: AllowedMentions.None);
        }

        public static async Task<IUserMessage> MsgReplyEmbedAsync(this SocketCommandContext context, object content)
        {
            return await context.Message.ReplyAsync(embed: context.CreateEmbed(content.ToString()).Build(), allowedMentions: AllowedMentions.None);
        }

        public static async Task<IUserMessage> MsgReplyEmbedAsync(this SocketCommandContext context, Embed emb)
        {
            return await context.Message.ReplyAsync(embed: emb, allowedMentions: AllowedMentions.None);
        }

        public static EmbedBuilder CreateEmbed(this SocketCommandContext context, object description = null, string title = null, string imgUrl = null, string url = null, string thumbnailUrl = null)
        {
            EmbedBuilder emb = new EmbedBuilder()
            {
                Title = title,
                Color = new Color(255, 200, 0),
                Footer = new()
                {
                    Text = context.IsPrivate ? context.User.Username : (context.User as SocketGuildUser).Nickname ?? context.User.Username,
                    IconUrl = context.User.GetAvatarUrl() ?? context.User.GetDefaultAvatarUrl()
                },
                Description = description?.ToString(),
                ImageUrl = imgUrl,
                Url = url,
                ThumbnailUrl = thumbnailUrl
            };

            return emb;
        }
    }
}
