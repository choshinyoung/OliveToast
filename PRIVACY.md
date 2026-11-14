# 올리브토스트 개인정보처리방침

시행일: 2026년 11월 14일

올리브토스트(이하 "봇")는 Discord 서버에서 동작하는 오픈소스 봇입니다. 이 문서는 봇이 어떤 데이터를 읽고, 무엇을 저장하며, 언제 삭제하는지 설명합니다. 모든 동작은 [소스코드](https://github.com/choshinyoung/OliveToast)에서 확인할 수 있습니다.

[English version](#olivetoast-privacy-policy)

## 1. 메시지 내용

봇은 Discord로부터 서버 메시지의 내용을 전달받아 아래 목적으로만 사용합니다. 메시지 내용은 데이터베이스나 파일에 기록하지 않으며, 예외는 3번 항목의 커스텀 커맨드 저장소뿐입니다.

| 기능 | 사용하는 경우 | 처리 방식 |
|---|---|---|
| 커맨드 | 접두사 또는 봇 멘션으로 시작하는 메시지 | 실행 후 보관하지 않음 |
| 게임 (끝말잇기, 타자 연습) | 게임이 진행 중인 채널에서 참가자가 보낸 메시지 | 게임이 끝나면 보관하지 않음 |
| 커스텀 커맨드 | 서버 관리자가 커맨드 카테고리를 켠 서버 | 관리자가 등록한 조건과 일치하는지 비교한 뒤 보관하지 않음 |
| 메시지 로그 | 서버 관리자가 로그 카테고리를 켜고 로그 채널을 지정한 서버 | 아래 참고 |

### 메시지 로그

로그 기능은 기본으로 꺼져 있습니다. 서버 관리자가 켠 서버에서만 다음과 같이 동작합니다.

- 해당 서버에서 주고받은 메시지의 작성자, 내용, 첨부파일 주소를 봇의 메모리에 임시로 보관합니다. 디스크나 데이터베이스에는 기록하지 않습니다.
- 메시지가 수정되거나 삭제되면 보관하던 이전 내용을 그 서버의 관리자가 지정한 로그 채널에 보냅니다. 다른 용도로는 사용하지 않습니다.
- 보관한 메시지는 작성 후 7일이 지나면 삭제합니다. 서버당 최근 10,000개까지만 보관하며, 넘으면 오래된 것부터 삭제합니다.
- 메시지가 삭제되었을 때, 관리자가 로그 카테고리를 껐을 때, 봇이 서버에서 나갔을 때, 봇이 재시작될 때에도 즉시 삭제합니다.

로그를 켜지 않은 서버의 메시지는 보관하지 않습니다.

## 2. 서버 멤버 정보

봇은 Discord로부터 서버 멤버 목록과 입장, 퇴장 이벤트를 전달받아 아래 기능에 사용합니다. 멤버 정보는 메모리에서만 사용하며, 이름, 별명, 역할, 가입일은 저장하지 않습니다.

- 입장 및 퇴장 메시지, 관리자가 설정한 입장 시 동작(예: 역할 지급)
- 추첨, 레벨 순위, 커스텀 커맨드에서의 멤버 조회

## 3. 저장하는 데이터

아래 데이터는 봇이 구동되는 서버의 데이터베이스에 저장합니다.

| 데이터 | 저장 조건 | 내용 |
|---|---|---|
| 서버 설정 | 봇이 있는 서버 | 서버 ID, 켜진 카테고리, 채널 ID와 역할 ID, 입장 및 퇴장 메시지 |
| 레벨 | 레벨 카테고리를 켠 서버 | 유저 ID, 레벨, 경험치 (메시지 수만 세며 내용은 사용하지 않음) |
| 커스텀 커맨드 | 관리자가 등록한 경우 | 조건, 응답, 스크립트, 등록한 유저 ID와 등록 당시 권한 |
| 커스텀 커맨드 저장소 | 관리자가 만든 스크립트가 값을 저장하는 경우 | 서버당 약 100개까지의 값과 소유자 유저 ID. 스크립트에 따라 메시지에서 가져온 값이 포함될 수 있음 |
| 커스텀 커맨드 사용 설정 | 커맨드 카테고리를 켠 서버에서 메시지를 보낸 유저 | 유저 ID, 커스텀 커맨드 반응 여부 |
| 봇 관리자 및 차단 목록 | 봇 운영자가 지정한 경우 | 유저 ID |

커맨드 사용 통계(커맨드 이름, 유저 ID, 서버 ID)와 진행 중인 게임 및 커맨드 세션은 메모리에만 있으며 봇이 재시작되면 사라집니다.

## 4. 외부 전송

봇은 데이터를 판매하지 않으며, 광고나 AI 학습에 사용하지 않습니다. 아래 경우에만 외부 서비스로 요청을 보냅니다.

| 서비스 | 보내는 정보 | 경우 |
|---|---|---|
| koreanbots.dev | 봇이 있는 서버 수 | 봇이 시작될 때, 서버에 들어가거나 나갈 때 |
| koreanbots.dev | 유저 ID | 커맨드 사용 후 투표 여부를 확인할 때 |
| koreanbots.dev, Scratch, Mojang | 유저가 입력한 검색어 | 검색 커맨드를 사용할 때 |

## 5. 삭제와 거부

- 봇이 서버에서 나가거나 추방되면 그 서버의 설정, 레벨, 커스텀 커맨드, 커스텀 커맨드 저장소를 즉시 삭제합니다.
- 서버 관리자는 `비활성화` 커맨드로 로그, 레벨, 커스텀 커맨드를 끌 수 있습니다.
- 유저는 `커맨드 사용 중지` 커맨드로 자신의 메시지에 커스텀 커맨드가 반응하지 않게 할 수 있습니다.
- 그 밖에 자신의 데이터 삭제를 원하면 아래 문의처로 요청해 주세요.

## 6. 문의

- [OliveToast 서포트 서버](https://discord.gg/5eq44Xtx3k)
- [GitHub Issues](https://github.com/choshinyoung/OliveToast/issues)

## 7. 변경

이 방침이 바뀌면 이 문서와 시행일을 갱신합니다.

---

# OliveToast Privacy Policy

Effective date: November 14, 2026

OliveToast ("the bot") is an open-source bot for Discord servers. This document explains what data the bot reads, what it stores, and when it deletes it. Everything described here can be verified in the [source code](https://github.com/choshinyoung/OliveToast).

## 1. Message content

The bot receives the content of server messages from Discord and uses it only for the purposes below. Message content is not written to a database or to disk; the only exception is the custom command storage described in section 3.

| Feature | When content is used | Handling |
|---|---|---|
| Commands | Messages that start with the prefix or a mention of the bot | Not kept after execution |
| Games (word chain, typing practice) | Messages from participants in a channel with an active game | Not kept after the game ends |
| Custom commands | Servers where an admin enabled the Command category | Compared against the triggers registered by admins, then not kept |
| Message log | Servers where an admin enabled the Log category and set a log channel | See below |

### Message log

Logging is off by default. Only in servers where an admin has enabled it:

- The author, content, and attachment URLs of messages sent in that server are held temporarily in the bot's memory. They are never written to disk or to a database.
- When a message is edited or deleted, the previous content is posted to the log channel chosen by that server's admins. It is not used for anything else.
- Held messages are deleted 7 days after they were sent. At most the 10,000 most recent messages per server are held; older ones are deleted first.
- They are also deleted immediately when the message is deleted, when an admin disables the Log category, when the bot leaves the server, and when the bot restarts.

Messages from servers that have not enabled logging are not held.

## 2. Server member information

The bot receives the member list and join/leave events from Discord for the features below. Member information is used in memory only; names, nicknames, roles, and join dates are not stored.

- Welcome and leave messages, and admin-configured actions on join (for example, assigning a role)
- Raffles, level leaderboards, and member lookups in custom commands

## 3. Stored data

The following data is stored in a database on the server that runs the bot.

| Data | Stored when | Contents |
|---|---|---|
| Server settings | The bot is in the server | Server ID, enabled categories, channel and role IDs, welcome and leave messages |
| Levels | The Level category is enabled | User ID, level, XP (messages are counted; content is not used) |
| Custom commands | An admin registers one | Trigger, response, script, the creator's user ID and permissions at creation |
| Custom command storage | An admin's script saves a value | Up to about 100 values per server with the owner's user ID. Depending on the script, a value may be derived from a message |
| Custom command preference | A user sends a message in a server with the Command category enabled | User ID, whether custom commands respond to the user |
| Bot admin and block lists | Set by the bot operator | User ID |

Command usage statistics (command name, user ID, server ID) and active game or command sessions exist in memory only and are lost when the bot restarts.

## 4. Third parties

The bot does not sell data and does not use it for advertising or AI training. Requests are sent to external services only in these cases:

| Service | Data sent | When |
|---|---|---|
| koreanbots.dev | Number of servers the bot is in | When the bot starts, joins a server, or leaves a server |
| koreanbots.dev | User ID | When checking vote status after a command |
| koreanbots.dev, Scratch, Mojang | The search term entered by the user | When a search command is used |

## 5. Deletion and opt-out

- When the bot leaves or is removed from a server, that server's settings, levels, custom commands, and custom command storage are deleted immediately.
- Server admins can turn off logging, levels, and custom commands with the `비활성화` command.
- Users can stop custom commands from responding to their messages with the `커맨드 사용 중지` command.
- For any other deletion request, contact us below.

## 6. Contact

- [OliveToast support server](https://discord.gg/5eq44Xtx3k)
- [GitHub Issues](https://github.com/choshinyoung/OliveToast/issues)

## 7. Changes

If this policy changes, this document and its effective date will be updated.
