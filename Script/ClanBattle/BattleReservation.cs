﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using Discord;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.Script
{
    public class BattleReservation
    {
        private const int MinBossNumber = 1;
        private const int MaxBossNumber = 5;
        private const int MaxCommentLength = 30;

        private readonly ClanData m_UserClanData;
        private readonly SocketRole m_UserRole;
        private readonly SocketUserMessage m_UserMessage;
        private readonly SocketReaction m_UserReaction;
        
        private BattleReservation(
            ClanData userClanData,
            ISocketMessageChannel channel,
            SocketUserMessage userMessage = null,
            SocketReaction userReaction = null)
        {
            m_UserClanData = userClanData;
            m_UserRole = (channel as SocketGuildChannel)?.Guild.GetRole(m_UserClanData.ClanRoleID);
            m_UserMessage = userMessage;
            m_UserReaction = userReaction;
        }

        public BattleReservation(ClanData userClanData, SocketUserMessage message)
            : this(userClanData, message.Channel, userMessage: message)
        {
        }

        public BattleReservation(ClanData userClanData, SocketReaction reaction)
            : this(userClanData, reaction.Channel, userReaction: reaction)
        {
        }

        public async Task RunReservationCommand()
        {
            var userMessage = m_UserMessage;

            if (userMessage == null) return;
            var messageContents = userMessage.Content;
            
            if (messageContents.StartsWith("予約"))
            {
                switch (messageContents)
                {
                    case "予約":
                    case "予約確認":
                    case "予約状況":
                        Console.WriteLine("予約確認");
                        await userMessage.Channel.SendMessageAsync(CreateUserReservationDataMessage());
                        //await SendMessageToChannel(userMessage.Channel, CreateUserReservationDataMessage());
                        return;
                }

                if (!IsReservationAllowTime())
                {
                    await SendErrorMessage(ErrorType.OutOfReservationTime,
                        $"{m_UserClanData.ReservationStartTime.Hours}:00", $"{m_UserClanData.ReservationEndTime.Hours}:00");
                    return;
                }

                var reservationData = MessageToReservationData();

                if (reservationData is null)
                {
                    await SendErrorMessage(ErrorType.FailedReservation);
                    return;
                }

                var nowBoss = m_UserClanData.GetNowBoss();
                var allowReservationLap = m_UserClanData.ReservationLap + m_UserClanData.GetNowLap();

                if (reservationData.BattleLap > allowReservationLap
                    || reservationData.BattleLap == allowReservationLap && reservationData.BossNumber > nowBoss)
                {
                    await SendErrorMessage(ErrorType.OutOfReservationBossLaps, allowReservationLap.ToString(), nowBoss.ToString());
                    return;
                }

                RegisterReservationData(reservationData);
                await SuccessAddEmoji();
                await UpdateSystemMessage();

                if (m_UserClanData.GetNowBoss() == reservationData.BossNumber
                    && m_UserClanData.GetNowLap() == reservationData.BattleLap)
                {
                    await new BattleDeclaration(m_UserClanData,m_UserMessage).UpdateDeclarationBotMessage();
                }
            }
            else if (messageContents.StartsWith("削除"))
            {
                var deleteReservationData = MessageToReservationData();
                
                if (deleteReservationData is null)
                {
                    // await FailedToRegisterMessage();
                    return;
                }

                if (DeleteUserReservationData(deleteReservationData))
                {
                    await SuccessAddEmoji();
                    await UpdateSystemMessage();
                }
            }
            else if (messageContents.StartsWith("!rm"))
            {
                var userReservationData = MessageToUserReservationData();

                if (userReservationData == null
                    || !DeleteUserReservationData(userReservationData))
                {
                    return;
                }
            }
        }


        public async Task RunReservationResultCommand()
        {
            if (m_UserMessage.Content.StartsWith("!start"))
            {
                await SendSystemMessage();
            }
        }

        public async Task RunReservationResultReaction()
        {
            switch (m_UserReaction.Emote.Name)
            {
                case "🔄":
                    await UpdateSystemMessage();
                    break;
            }
            await RemoveUserReaction();
        }

        /// <summary>
        /// 凸予約一覧チャンネルにメッセージを送信する。
        /// </summary>
        /// <returns></returns>
        public async Task SendSystemMessage()
        {
            var userClanData = m_UserClanData;
            var userRole = m_UserRole;
            var messageData = CreateAllReservationDataMessage(userClanData);
            var reservationResultChannelID = userClanData.ChannelData
                .GetChannelID(userClanData.ClanID, ChannelFeatureType.ReserveResultID);

            if (reservationResultChannelID == 0)
            {
                return;
            }

            var resultChannel = userRole.Guild
                .GetTextChannel(reservationResultChannelID);

            var sendedMessageData = await resultChannel.SendMessageAsync(messageData);
            DatabaseMessageDataController.UpdateMessageID(userClanData, sendedMessageData.Id, MessageFeatureType.ReserveResultID);
            await AttacheDefaultReaction(sendedMessageData);
        }

        public async Task UpdateSystemMessage()
        {
            var userClanData = m_UserClanData;
            var userRole = m_UserRole;
            var reservationMessageID = userClanData.MessageData
                .GetMessageID(userClanData.ClanID, MessageFeatureType.ReserveResultID);

            if (reservationMessageID == 0)
            {
                return;
            }

            var reservationResultChannelID = userClanData.ChannelData
                .GetChannelID(userClanData.ClanID, ChannelFeatureType.ReserveResultID);

            if(reservationResultChannelID == 0)
            {
                return;
            }

            var guildChannel = userRole.Guild
                .GetChannel(reservationResultChannelID) as SocketTextChannel;
            var socketMessage = guildChannel.GetCachedMessage(reservationMessageID);

            if (socketMessage == null || !(socketMessage is SocketUserMessage))
            {
                var message = await guildChannel.GetMessageAsync(reservationMessageID);

                if (message != null)
                {
                    await guildChannel.DeleteMessageAsync(message);
                    await SendSystemMessage();
                }

                return;
            }

            var serverMessage = socketMessage as SocketUserMessage;
            var messageData = CreateAllReservationDataMessage(userClanData);
            await serverMessage.ModifyAsync(x => x.Content = messageData);
        }

        /// <summary>
        /// 受信した予約データを解析して保存する
        /// 例 : 「予約 35 1 1200万程度」
        /// 「予約 周回 ボス メモ(任意)」
        /// </summary>
        /// <returns></returns>
        private ReservationData MessageToReservationData()
        {
            var userClanData = m_UserClanData;
            var userMessage = m_UserMessage;

            var nowBattleLap = userClanData.GetNowLap();
            var nowBossNumber = userClanData.GetNowBoss();

            var limitReservationLap = m_UserClanData.ReservationLap;
            if (limitReservationLap == 0)
            {
                limitReservationLap = byte.MaxValue;
            }

            var splitMessageContent =userMessage.Content.ZenToHan()
                .Split(new string[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

            if (splitMessageContent.Length < 3
                || !(byte.TryParse(splitMessageContent[1], out byte battleLap) && battleLap > 0)
                || !(byte.TryParse(splitMessageContent[2], out byte bossNumber) && bossNumber <= MaxBossNumber && bossNumber >= MinBossNumber)
                || battleLap < nowBattleLap
                || battleLap == nowBattleLap && bossNumber < nowBossNumber)
            {
                return null;
            }
            var commentData = string.Join(" ",splitMessageContent.Skip(3));

            if (commentData.Length > MaxCommentLength)
            {
                commentData = commentData.Substring(0,MaxCommentLength);
                userMessage.Channel.SendMessageAsync($"コメントが長いので切り取られました。\n 問題がある場合は予約削除をして再度予約してください。");
            }
           
            return new ReservationData()
            {
                PlayerData = new PlayerData
                {
                    ClanData = userClanData,
                    UserID = userMessage.Author.Id
                },
                BattleLap = battleLap,
                BossNumber = bossNumber,
                CommentData = commentData
            };
        }

        private void RegisterReservationData(ReservationData reservationData)
        {
            var allSqlReservationData = DatabaseReservationController.LoadReservationData(reservationData.PlayerData);

            var doesExistReservationData = allSqlReservationData
                .Any(x => x.BossNumber == reservationData.BossNumber && x.BattleLap == reservationData.BattleLap);

            if (!doesExistReservationData)
            {
                DatabaseReservationController.CreateReservationData(reservationData);
            }
            else
            {
                DatabaseReservationController.UpdateReservationData(reservationData);
            }
        }

        /// <summary>
        /// 個人が予約しているデータの取得
        /// </summary>
        /// <returns></returns>
        private ReservationData MessageToUserReservationData()
        {
            var splitMessageContent = m_UserMessage.Content
                .Split(new[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

            if (splitMessageContent.Length != 4
                || !ulong.TryParse(splitMessageContent[1], out ulong userID)
                || !(byte.TryParse(splitMessageContent[2], out byte battleLap) && battleLap > 0)
                || !(byte.TryParse(splitMessageContent[3], out byte bossNumber) && bossNumber <= MaxBossNumber && bossNumber >= MinBossNumber))
            {
                return null;
            }

            var playerData = DatabasePlayerDataController.LoadPlayerData(m_UserRole, userID);

            if (playerData == null)
            {
                return null;
            }

            return DatabaseReservationController.LoadReservationData(playerData)
                .FirstOrDefault(d => d.BattleLap == battleLap && d.BossNumber == bossNumber);
        }

        private bool DeleteUserReservationData(ReservationData reservationData)
        {
            var allSqlReservationData = DatabaseReservationController.LoadReservationData(reservationData.PlayerData);

            var sqlReservationData = allSqlReservationData
                .Where(x => x.BossNumber == reservationData.BossNumber && x.BattleLap == reservationData.BattleLap)
                .ToList();

            if (sqlReservationData.Count == 0)
            {
                return false;
            }

            DatabaseReservationController.DeleteReservationData(sqlReservationData);

            return true;
        }

        private string CreateUserReservationDataMessage()
            => CreateUserReservationDataMessage(
                DatabasePlayerDataController.LoadPlayerData(m_UserRole, m_UserMessage.Author.Id)
            );

        private string CreateUserReservationDataMessage(PlayerData playerData)
        {
            var reservationDataSet = DatabaseReservationController.LoadReservationData(playerData);

            if (reservationDataSet.Count == 0)
            {
                return "予約がありません";
            }

            var messageData = new StringBuilder();
            messageData.AppendLine("```python");
            messageData.AppendLine($"{playerData.GuildUserName}さんの予約状況");
            
            foreach (var (reservationData, loopNum) in reservationDataSet.Select((v, i) => (v, i)))
            {
                messageData.AppendLine(
                    $"{loopNum + 1,2}. {reservationData.BattleLap,2}-{reservationData.BossNumber} {reservationData.CommentData}"
                );
            }

            messageData.Append($"以上の{reservationDataSet.Count}件です```");

            return messageData.ToString();
        }

        private string CreateAllReservationDataMessage(ClanData clanData)
        {
            var bossNumber = clanData.GetNowBoss();
            var battleLap = clanData.GetNowLap();

            var reservationDataSet = DatabaseReservationController.LoadReservationData(clanData);

            reservationDataSet = reservationDataSet
                .Where(b => b.BattleLap > battleLap || (b.BattleLap == battleLap && b.BossNumber >= bossNumber))
                .ToList();


            if (!reservationDataSet.Any())
            {
                return "予約がありません";
            }

            var messageData = new StringBuilder();
            messageData.AppendLine("```python");
            messageData.AppendLine($"{battleLap,2}-{bossNumber} 以降の予約一覧です. ");

            foreach (var reservationData in reservationDataSet)
            {
                messageData.AppendLine(
                    $"{reservationData.BattleLap,2}-{reservationData.BossNumber} " +
                    $"{reservationData.PlayerData.GuildUserName} {reservationData.CommentData}"
                );
            }

            messageData.Append($"以上の{reservationDataSet.Count}件です");
            messageData.Append("```");

            return messageData.ToString();
        }

        private async Task AttacheDefaultReaction(IUserMessage message)
        {
            string[] emojiData = { "🔄" };
            var emojiMatrix = emojiData.Select(x => new Emoji(x)).ToArray();
            await message.AddReactionsAsync(emojiMatrix);
        }

        private async Task RemoveUserReaction()
        {
            var reservationResultChannelID = m_UserClanData.ChannelData
                .GetChannelID(m_UserClanData.ClanID, ChannelFeatureType.ReserveResultID);
            var textChannnel = m_UserRole.Guild.GetTextChannel(reservationResultChannelID);

            if(textChannnel == null)
            {
                return;
            }

            var message = await textChannnel.GetMessageAsync(m_UserReaction.MessageId);

            if (message == null)
            {
                return;
            }
            await message.RemoveReactionAsync(m_UserReaction.Emote, m_UserReaction.User.Value);
        }
        
        private async Task SendErrorMessage(ErrorType type, params string[] parameters)
        {
            var descriptionString = type.GetDescription();
            var sendMessage = string.Empty;
            if (descriptionString == null)
            {
                sendMessage = type.ToString();
            }
            else
            {
                sendMessage = string.Format(descriptionString, parameters);
            }
            await m_UserMessage.Channel.SendMessageAsync(sendMessage);
        }

        private async Task SuccessAddEmoji()
            => await m_UserMessage.AddReactionAsync(new Emoji(ReactionType.Success.GetDescription()));

        /// <summary>
        /// 予約できる時間かどうか判断する。
        /// </summary>
        /// <returns></returns>
        private bool IsReservationAllowTime()
        {
            if (m_UserClanData == null)
            {
                return false;
            }

            var startTime = m_UserClanData.ReservationStartTime;
            var endTime = m_UserClanData.ReservationEndTime;
            var nowTime = DateTime.Now.TimeOfDay;

            if (startTime.Hours == 0 && endTime.Hours == 0)
            {
                return true;
            }

            if(startTime.Hours < Define.Common.DateUpdateHour)
            {
                startTime = startTime.Add(new TimeSpan(1, 0, 0, 0));
            }

            if (endTime.Hours < Define.Common.DateUpdateHour)
            {
                endTime = endTime.Add(new TimeSpan(1, 0, 0, 0));
            }

            if (nowTime.Hours < Define.Common.DateUpdateHour)
            {
                nowTime = nowTime.Add(new TimeSpan(1, 0, 0, 0));
            }

            if (startTime.TotalSeconds >= endTime.TotalSeconds)
            {
                return false;
            }

            if (nowTime.TotalSeconds <= startTime.TotalSeconds || nowTime.TotalSeconds > endTime.TotalSeconds)
            {
                return false;
            }

            return true;
        }

    }
}
