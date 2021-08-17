using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast.Managements
{
    public static class CommandExtension
    {
        public static async Task<RestUserMessage> ReplyAsync(this SocketCommandContext context, object content, bool disalbeMention = true, MessageComponent component = null)
        {
            MessageReference reference = new(context.Message.Id, context.Channel.Id, context.Guild?.Id);
            return await context.Channel.SendMessageAsync(text: content.ToString(), allowedMentions: disalbeMention ? AllowedMentions.None : null, messageReference: reference, component: component);
        }

        public static async Task<RestUserMessage> ReplyEmbedAsync(this SocketCommandContext context, object content, bool disalbeMention = true, MessageComponent component = null)
        {
            Embed emb = context.CreateEmbed(content.ToString()).Build();
            MessageReference reference = new(context.Message.Id, context.Channel.Id, context.Guild?.Id);
            return await context.Channel.SendMessageAsync(embed: emb, allowedMentions: disalbeMention ? AllowedMentions.None : null, messageReference: reference, component: component);
        }

        public static async Task<RestUserMessage> ReplyEmbedAsync(this SocketCommandContext context, Embed emb, bool disalbeMention = true, MessageComponent component = null)
        {
            MessageReference reference = new(context.Message.Id, context.Channel.Id, context.Guild?.Id);
            return await context.Channel.SendMessageAsync(embed: emb, allowedMentions: disalbeMention ? AllowedMentions.None : null, messageReference: reference, component: component);
        }

        public static EmbedBuilder CreateEmbed(this SocketCommandContext context, object description = null, string title = null, string imgUrl = null, string url = null, string thumbnailUrl = null, Color? color = null)
        {
            return CreateEmbed(context.User, context.IsPrivate, description, title, imgUrl, url, thumbnailUrl, color);
        }

        public static EmbedBuilder CreateEmbed(this SocketUser user, bool isPrivate, object description = null, string title = null, string imgUrl = null, string url = null, string thumbnailUrl = null, Color? color = null)
        {
            EmbedBuilder emb = new()
            {
                Title = title,
                Color = color ?? new Color(255, 200, 0),
                Footer = new()
                {
                    Text = user.GetName(isPrivate),
                    IconUrl = user.GetAvatar()
                },
                Description = description?.ToString(),
                ImageUrl = imgUrl,
                Url = url,
                ThumbnailUrl = thumbnailUrl,
            };

            return emb;
        }
        
        public static EmbedBuilder AddEmptyField(this EmbedBuilder emb)
        {
            return emb.AddField("**  **", "** **", true);
        }

        public static MemoryStream GetFileStream(this SocketCommandContext context, ref string url)
        {
            if (url == null)
            {
                if (context.Message.Attachments.Any())
                {
                    if (context.Message.Attachments.First().Size > 8000000)
                    {
                        throw new Exception("파일이 너무 크고 아름다워요");
                    }
                    url = context.Message.Attachments.First().Url;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                using HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                if (webResponse.ContentLength > 8000000)
                {
                    throw new Exception("파일이 너무 크고 아름다워요");
                }
            }

            using Utility.TimeOutWebClient wc = new();
            byte[] bytes = wc.DownloadData(url);

            MemoryStream stream = new(bytes);

            return stream;
        }
    }
}
