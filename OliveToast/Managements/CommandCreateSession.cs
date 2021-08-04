using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toast.Nodes;

namespace OliveToast.Managements
{
    class CommandCreateSession
    {
        public enum Status
        {
            CommandInput, AnswerInput, ExecuteLinesInput
        }

        public enum ResponseType
        {
            ChangeRegex, Complete, Cancel, ContinueWithoutAnswer
        }

        public static Dictionary<ulong, CommandCreateSession> Sessions = new();

        public Status SessionStatus;
        public string Command;
        public OliveGuild.CustomCommand CustomCommand;

        public RestUserMessage Message;
        public SocketCommandContext UserMessageContext;

        public DateTime LastActiveTime;

        public static async Task<bool> MessageResponse(ulong userId, ulong channelId, string content)
        {
            if (!Sessions.ContainsKey(userId) || Sessions[userId].UserMessageContext.Channel.Id != channelId)
            {
                return false;
            }

            CommandCreateSession session = Sessions[userId];
            session.LastActiveTime = DateTime.Now;

            switch (session.SessionStatus)
            {
                case Status.CommandInput:
                    session.Command = content;
                    session.SessionStatus = Status.AnswerInput;

                    EmbedBuilder emb = session.Message.Embeds.First().ToEmbedBuilder();
                    emb.Description = "응답을 입력해주세요";
                    emb.AddField("커맨드", content, true);

                    ComponentBuilder component = new ComponentBuilder()
                        .WithButton("응답 없이 계속하기", $"{userId}.{(int)CommandEventHandler.InteractionType.CreateCommand}.{(int)ResponseType.ContinueWithoutAnswer}")
                        .WithButton("취소", $"{userId}.{(int)CommandEventHandler.InteractionType.CreateCommand}.{(int)ResponseType.Cancel}", ButtonStyle.Danger);

                    await session.Message.ModifyAsync(msg => 
                    { 
                        msg.Embed = emb.Build();
                        msg.Components = component.Build();
                    });

                    break;
                case Status.AnswerInput:
                    session.CustomCommand.Answer = content;
                    session.SessionStatus = Status.ExecuteLinesInput;

                    emb = session.Message.Embeds.First().ToEmbedBuilder();
                    emb.Description = "토스트 커맨드를 한 줄씩 입력하고 `완료` 버튼을 눌러주세요";
                    emb.AddField("응답", content, true);

                    component = new ComponentBuilder()
                        .WithButton("완료", $"{userId}.{(int)CommandEventHandler.InteractionType.CreateCommand}.{(int)ResponseType.Complete}", ButtonStyle.Success)
                        .WithButton("취소", $"{userId}.{(int)CommandEventHandler.InteractionType.CreateCommand}.{(int)ResponseType.Cancel}", ButtonStyle.Danger);

                    await session.Message.ModifyAsync(msg =>
                    {
                        msg.Embed = emb.Build();
                        msg.Components = component.Build();
                    });

                    break;
                case Status.ExecuteLinesInput:
                    session.CustomCommand.RawToastLines.Add(content);

                    break;
                default:
                    return false;
            }

            return true;
        }

        public static async Task ButtonResponse(ulong userId, ResponseType response)
        {
            if (!Sessions.ContainsKey(userId))
            {
                return;
            }

            CommandCreateSession session = Sessions[userId];
            session.LastActiveTime = DateTime.Now;

            switch (response)
            {
                case ResponseType.ChangeRegex:
                    session.CustomCommand.IsRegex = !session.CustomCommand.IsRegex;

                    ComponentBuilder component = new ComponentBuilder()
                        .WithButton(session.CustomCommand.IsRegex ? "일반 텍스트로 변경" : "정규식으로 변경", $"{userId}.{(int)CommandEventHandler.InteractionType.CreateCommand}.{(int)ResponseType.ChangeRegex}")
                        .WithButton("취소", $"{userId}.{(int)CommandEventHandler.InteractionType.CreateCommand}.{(int)ResponseType.Cancel}", ButtonStyle.Danger);

                    await session.Message.ModifyAsync(msg =>
                    {
                        msg.Components = component.Build();
                    });

                    break;
                case ResponseType.Complete:
                    if (session.CustomCommand.Answer is null && session.CustomCommand.RawToastLines.Count == 0)
                    {
                        await session.UserMessageContext.MsgReplyEmbedAsync("응답이 없는 커맨드는 토스트 커맨드가 한 줄 이상 있어야돼요");

                        break;
                    }

                    EmbedBuilder emb = session.UserMessageContext.CreateEmbed(title: "커맨드 생성 완료");

                    emb.AddField("커맨드", session.Command, true);
                    if (session.CustomCommand.Answer is not null)
                    {
                        emb.AddField("응답", session.CustomCommand.Answer, true);
                    }
                    emb.AddField("정규식 사용 여부", session.CustomCommand.IsRegex.ToEmoji(), true);

                    if (session.CustomCommand.RawToastLines.Count > 0)
                    {
                        emb.AddField("토스트 커맨드", string.Concat(session.CustomCommand.RawToastLines.Select(l => $"```\n{l}\n```")));
                    }

                    Sessions.Remove(userId);

                    var commands = OliveGuild.Get(session.UserMessageContext.Guild.Id).Commands;

                    if (commands.ContainsKey(session.Command))
                    {
                        commands[session.Command].Add(session.CustomCommand);
                    }
                    else
                    {
                        commands.Add(session.Command, new() { session.CustomCommand });
                    }
                    OliveGuild.Set(session.UserMessageContext.Guild.Id, g => g.Commands, commands);

                    await session.UserMessageContext.MsgReplyEmbedAsync(emb.Build());

                    break;
                case ResponseType.Cancel:
                    Sessions.Remove(userId);

                    await session.UserMessageContext.MsgReplyEmbedAsync("커맨드 생성을 취소했어요");

                    break;
                case ResponseType.ContinueWithoutAnswer:
                    session.CustomCommand.Answer = null;
                    session.SessionStatus = Status.ExecuteLinesInput;

                    emb = session.Message.Embeds.First().ToEmbedBuilder();
                    emb.Description = "토스트 커맨드를 한 줄씩 입력하고 `완료` 버튼을 눌러주세요";

                    component = new ComponentBuilder()
                        .WithButton("완료", $"{userId}.{(int)CommandEventHandler.InteractionType.CreateCommand}.{(int)ResponseType.Complete}", ButtonStyle.Success)
                        .WithButton("취소", $"{userId}.{(int)CommandEventHandler.InteractionType.CreateCommand}.{(int)ResponseType.Cancel}", ButtonStyle.Danger);

                    await session.Message.ModifyAsync(msg =>
                    {
                        msg.Embed = emb.Build();
                        msg.Components = component.Build();
                    });

                    break;
            }
        }
    }
}
