using Discord;
using Discord.Commands;
using Discord.Rest;
using Newtonsoft.Json;
using OliveToast.Managements.data;
using OliveToast.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OliveToast.Managements.CustomCommand
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

        public enum CommandType
        {
            CustomCommand, JoinMessage, LeaveMessage
        }

        public static Dictionary<ulong, CommandCreateSession> Sessions = new();

        public Status SessionStatus;
        public string Command;
        public OliveGuild.CustomCommand CustomCommand;

        public RestUserMessage Message;
        public SocketCommandContext UserMessageContext;

        public DateTime LastActiveTime;

        public CommandType Type;

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
                    EmbedBuilder emb;

                    if (session.CustomCommand.IsRegex)
                    {
                        try
                        {
                            Regex regex = new(content);
                            
                            foreach (string str in RegexTextStrings.Strings)
                            {
                                Match match = regex.Match(str);

                                if (match.Success && match.Value.Length == str.Length)
                                {
                                    throw new Exception();
                                }
                            }
                        }
                        catch
                        {
                            emb = session.UserMessageContext.CreateEmbed(title: "오류 발생!", description: "유효하지 않은 정규식 입력이에요");
                            await session.UserMessageContext.ReplyEmbedAsync(emb.Build());

                            return true;
                        }
                    }

                    session.Command = content;
                    session.SessionStatus = Status.AnswerInput;

                    emb = session.Message.Embeds.First().ToEmbedBuilder();
                    emb.Description = "응답을 입력해주세요";
                    emb.AddField("커맨드", content, true);

                    ComponentBuilder component = new ComponentBuilder()
                        .WithButton("응답 없이 계속하기", $"{userId}.{(int)CommandEventHandler.InteractionType.CreateCommand}.{(int)ResponseType.ContinueWithoutAnswer}")
                        .WithButton("취소", $"{userId}.{(int)CommandEventHandler.InteractionType.CreateCommand}.{(int)ResponseType.Cancel}", ButtonStyle.Danger);

                    await session.Message.ModifyAsync(msg => 
                    { 
                        msg.Embeds = new[] { emb.Build() };
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
                        msg.Embeds = new[] { emb.Build() };
                        msg.Components = component.Build();
                    });

                    break;
                case Status.ExecuteLinesInput:
                    try
                    {
                        CustomCommandExecutor.GetToaster().Parse(content);
                    }
                    catch (Exception e)
                    {
                        emb = session.UserMessageContext.CreateEmbed(title: "오류 발생!", description: e.Message);
                        await session.UserMessageContext.ReplyEmbedAsync(emb.Build());

                        return true;
                    }

                    session.CustomCommand.ToastLines.Add(content);

                    emb = session.Message.Embeds.First().ToEmbedBuilder();
                    if (emb.Fields.Find(e => e.Name == "토스트 커맨드") is var field and not null)
                    {
                        field.Value = string.Concat(session.CustomCommand.ToastLines.Select(l => $"```\n{l}\n```"));
                    }
                    else
                    {
                        emb.AddField("토스트 커맨드", string.Concat(session.CustomCommand.ToastLines.Select(l => $"```\n{l.Slice(60)}\n```")));
                    }

                    await session.Message.ModifyAsync(msg =>
                    {
                        msg.Embeds = new[] { emb.Build() };
                    });

                    if (session.CustomCommand.ToastLines.Count >= 15)
                    {
                        await ButtonResponse(userId, ResponseType.Complete);
                    }

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
                        .WithButton("취소", $"{userId}.{(int)CommandEventHandler.InteractionType.CreateCommand}.{(int)ResponseType.Cancel}", ButtonStyle.Danger)
                        /* .WithButton("커맨드 사용 방법", style: ButtonStyle.Link, url: "https://olivetoast.shinyou.ng/") */;

                    await session.Message.ModifyAsync(msg =>
                    {
                        msg.Components = component.Build();
                    });

                    break;
                case ResponseType.Complete:
                    EmbedBuilder emb;
                    if (session.CustomCommand.Answer is null && session.CustomCommand.ToastLines.Count == 0 && session.Type == CommandType.CustomCommand)
                    {
                        emb = session.UserMessageContext.CreateEmbed(description: "응답이 없는 커맨드는 토스트 커맨드가 한 줄 이상 있어야돼요");
                        await session.UserMessageContext.Channel.SendMessageAsync(embed: emb.Build());

                        break;
                    }

                    Sessions.Remove(userId);

                    switch (session.Type)
                    {
                        case CommandType.CustomCommand:
                            emb = session.UserMessageContext.CreateEmbed(title: "커맨드 생성 완료");

                            emb.AddField("커맨드", session.Command, true);
                            if (session.CustomCommand.Answer is not null)
                            {
                                emb.AddField("응답", session.CustomCommand.Answer, true);
                            }
                            emb.AddField("정규식 사용 여부", session.CustomCommand.IsRegex.ToEmoji(), true);

                            if (session.CustomCommand.ToastLines.Count > 0)
                            {
                                emb.AddField("토스트 커맨드", string.Concat(session.CustomCommand.ToastLines.Select(l => $"```\n{l.Slice(60)}\n```")));
                            }

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

                            await session.UserMessageContext.ReplyEmbedAsync(emb.Build());

                            break;
                        case CommandType.JoinMessage:
                            OliveGuild.GuildSetting setting = OliveGuild.Get(session.UserMessageContext.Guild.Id).Setting;

                            setting.JoinMessage = session.CustomCommand.Answer;
                            setting.JoinMessageToastLines = session.CustomCommand.ToastLines;

                            OliveGuild.Set(session.UserMessageContext.Guild.Id, g => g.Setting, setting);

                            emb = session.UserMessageContext.CreateEmbed("입장메시지를 설정했어요");

                            if (session.CustomCommand.Answer is not null)
                            {
                                emb.AddField("응답", session.CustomCommand.Answer);
                            }

                            if (session.CustomCommand.ToastLines.Count > 0)
                            {
                                emb.AddField("토스트 커맨드", string.Concat(session.CustomCommand.ToastLines.Select(l => $"```\n{l.Slice(60)}\n```")));
                            }

                            await session.UserMessageContext.ReplyEmbedAsync(emb.Build());

                            break;
                        case CommandType.LeaveMessage:
                            setting = OliveGuild.Get(session.UserMessageContext.Guild.Id).Setting;

                            setting.LeaveMessage = session.CustomCommand.Answer;
                            setting.LeaveMessageToastLines = session.CustomCommand.ToastLines;

                            OliveGuild.Set(session.UserMessageContext.Guild.Id, g => g.Setting, setting);

                            emb = session.UserMessageContext.CreateEmbed("퇴장메시지를 설정했어요");

                            if (session.CustomCommand.Answer is not null)
                            {
                                emb.AddField("응답", session.CustomCommand.Answer);
                            }

                            if (session.CustomCommand.ToastLines.Count > 0)
                            {
                                emb.AddField("토스트 커맨드", string.Concat(session.CustomCommand.ToastLines.Select(l => $"```\n{l.Slice(60)}\n```")));
                            }

                            await session.UserMessageContext.ReplyEmbedAsync(emb.Build());

                            break;
                    }

                    break;
                case ResponseType.Cancel:
                    Sessions.Remove(userId);

                    await session.UserMessageContext.ReplyEmbedAsync("커맨드 생성을 취소했어요");

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
                        msg.Embeds = new[] { emb.Build() };
                        msg.Components = component.Build();
                    });

                    break;
            }
        }
    }

    class RegexTextStrings
    {
        public static readonly List<string> Strings;

        static RegexTextStrings()
        {
            string content = File.ReadAllText("Configs/regexTestStrings.json");

            Strings = JsonConvert.DeserializeObject<List<string>>(content);
        }
    }

    class CommandDeleteSession
    {
        public enum ResponseType
        {
            DeleteCommand, DeleteAnswers, DeleteSingleAnswer, Cancel
        }

        public static Dictionary<ulong, CommandDeleteSession> Sessions = new();

        public SocketCommandContext Context;

        public int CommandIndex;
        public int AnswerIndex;

        public string Command;
        public string Answer;

        public CommandDeleteSession(SocketCommandContext context, int cIndex, int aIndex)
        {
            Context = context;

            CommandIndex = cIndex;
            AnswerIndex = aIndex;
        }

        public CommandDeleteSession(SocketCommandContext context, string command, string answer)
        {
            Context = context;

            Command = command;
            Answer = answer;
        }

        public CommandDeleteSession(SocketCommandContext context, string command)
        {
            Context = context;

            Command = command;
        }
    }
}
