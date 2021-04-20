﻿using Discord.WebSocket;
using Discord.Rest;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using PriconneBotConsoleApp.DataTypes;
using PriconneBotConsoleApp.MySQL;
using System.Threading.Tasks;
using System.Linq;

namespace PriconneBotConsoleApp.Script
{
    class BattleReservation
    {
        private ClanData m_userClanData;
        private SocketRole m_userRole;
        private const int MinBossNumber = 1;
        private const int MaxBossNumber = 5;

        private SocketUserMessage m_userMessage;
        
        public BattleReservation(ClanData userClanData, SocketUserMessage message)
        {
            m_userClanData = userClanData;
            m_userMessage = message;

            var socketGuildChannel = message.Channel as SocketGuildChannel;

            m_userRole = socketGuildChannel.Guild.GetRole(ulong.Parse(m_userClanData.ClanRoleID));
        }

        async public Task RunReservationCommand()
        {
            var messageContents = m_userMessage.Content;
            if (messageContents.StartsWith("!start"))
            {
                Console.WriteLine("OK");
            }
            else if (messageContents.StartsWith("予約"))
            {
                if (messageContents == "予約" || messageContents == "予約確認"
                    || messageContents == "予約状況")
                {
                    Console.WriteLine("予約確認");
                    var sendMessageData = CreateUserReservationDataMessage();
                    await SendMessageToChannel(sendMessageData);
                    return;
                }
                var reservationData = MessageToReservationData();
                if (reservationData is null)
                {
                    await FailedToRegisterMessage();
                    return;
                }
                RegisterReservationData(reservationData);

                await SuccessAddEmoji();
                
                return;
            }
            else if (messageContents.StartsWith("削除"))
            {
                var deleteReservationData = MessageToReservationData();
                if (deleteReservationData is null)
                {
                    //await FailedToRegisterMessage();
                    return;
                }
                var result = DeleteUserReservationData(deleteReservationData);
                if (result)
                {
                    await SuccessAddEmoji();
                }
                return;
            }
        }

        async public Task SendSystemMessage()
        {
            var messageData = CreateAllReservationDataMessage(m_userClanData);
            var sendedMessageData = await SendMessageToChannel(messageData);
            new MySQLReservationController().UpdateReservationMessageID(
                m_userClanData, sendedMessageData.Id.ToString());
            return;
        }

        async public Task UpdateSystemMessage()
        {
            var reservationMessageID = m_userClanData.MessageIDs.ReservationMessageID;
            if (reservationMessageID == null)
            {
                return;
            }

        }

        public void RegisterReservation()
        {

        }

        /// <summary>
        /// 受信した予約データを解析して保存する
        /// 例 : 「予約 35 1 1200万程度」
        /// 「予約 周回 ボス メモ(任意)」
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private ReservationData MessageToReservationData()
        {
            var massageContent = ZenToHan(m_userMessage.Content);
            
            var splitMessageContent =
                massageContent.Split(new string[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

            if ((splitMessageContent.Length >= 3 && splitMessageContent.Length <= 4) == false )
            { 
                return null;
            }

            if (!(int.TryParse(splitMessageContent[1], out int bossNumber)
                && bossNumber <= MaxBossNumber && bossNumber >= MinBossNumber))
            {
                return null;
            }
            if (!(int.TryParse(splitMessageContent[2], out int battleLap) 
                && battleLap > 0))
            {
                return null;
            }

            var serverID = m_userClanData.ServerID;
            var clanRoleID = m_userClanData.ClanRoleID;
            var userID = m_userMessage.Author.Id.ToString();
            string commentData = null;

            if (splitMessageContent.Length == 4)
            {
                commentData = splitMessageContent[3];
            }


            return new ReservationData() {
                PlayerData = new PlayerData()
                {
                    //ClanData = new ClanData()
                    //{
                    //    ServerID = serverID,
                    //    ClanRoleID = clanRoleID
                    //},
                    ClanData = m_userClanData,
                    UserID = userID
                },
                BattleLaps = battleLap,
                BossNumber = bossNumber,
                CommentData = commentData
            };
        }

        private void RegisterReservationData(ReservationData reservationData)
        {
            var mySQLReservationController = new MySQLReservationController();
            var allSqlReservationData =
                mySQLReservationController.LoadReservationData(reservationData.PlayerData);

            var sqlReservationData = allSqlReservationData
                .Where(x => x.BossNumber == reservationData.BossNumber)
                .Where(y => y.BattleLaps == reservationData.BattleLaps);

            if (sqlReservationData.Count() == 0)
            {
                mySQLReservationController.CreateReservationData(reservationData);
            }
            else
            {
                mySQLReservationController.UpdateReservationData(reservationData);
            }
        }

        private bool DeleteUserReservationData(ReservationData reservationData)
        {
            var mySQLReservationController = new MySQLReservationController();
            var allSqlReservationData =
                mySQLReservationController.LoadReservationData(reservationData.PlayerData);

            var sqlReservationData = allSqlReservationData
                .Where(x => x.BossNumber == reservationData.BossNumber)
                .Where(y => y.BattleLaps == reservationData.BattleLaps);
            if (sqlReservationData.Count() == 0)
            {
                return false;
            }
            mySQLReservationController.DeleteReservationData(sqlReservationData);

            return true;
        }

        private string CreateUserReservationDataMessage()
        {
            var playerData = new MySQLPlayerDataController()
                .LoadPlayerData(m_userClanData.ServerID, m_userMessage.Author.Id.ToString());

            var messageData =
                CreateUserReservationDataMessage(playerData);

            return messageData;
            
        }

        private string CreateUserReservationDataMessage(PlayerData playerData)
        {
            var reservationDataSet = new MySQLReservationController().LoadReservationData(playerData);

            if (reservationDataSet.Count() == 0)
            {
                return "予約がありません";
            }

            var messageData = "```python\n";
            messageData += playerData.GuildUserName + "さんの予約状況 \n";
            var loopNum = 0;
            foreach (var reservationData in reservationDataSet)
            {
                loopNum += 1;
                messageData += loopNum.ToString().PadLeft(2) + ". " +
                    reservationData.BattleLaps.ToString().PadLeft(2) + "周目 " +
                    reservationData.BossNumber.ToString() + "ボス " +
                    reservationData.CommentData +
                    "\n";
            }
            messageData += $"以上の{loopNum}件です";
            messageData += "```";

            return messageData;
        }

        private string CreateAllReservationDataMessage(ClanData clanData)
        {
            var reservationDataSet = new MySQLReservationController().LoadReservationData(clanData);

            if (reservationDataSet.Count() == 0)
            {
                return "予約がありません";
            }

            var messageData = "```python\n";

            var loopNum = 0;
            foreach (var reservationData in reservationDataSet)
            {
                loopNum += 1;
                messageData += loopNum.ToString().PadLeft(2) + ". " +
                    reservationData.BattleLaps.ToString().PadLeft(2) + "周目 " +
                    reservationData.BossNumber.ToString() + "ボス " +
                    reservationData.PlayerData.GuildUserName + " " +
                    reservationData.CommentData +
                    "\n";
            }
            messageData += $"以上の{loopNum}件です";
            messageData += "```";

            return messageData;
        }

        async private Task<RestMessage> SendMessageToChannel(string messageData)
        {
            var result = await m_userMessage.Channel.SendMessageAsync(messageData);
            return result;
        }

        async private Task FailedToRegisterMessage()
        {
            var textMessage = "予約に失敗しました。";
            await m_userMessage.Channel.SendMessageAsync(textMessage);
            return;
        }

        async private Task SuccessAddEmoji()
        {
            var successEmoji = new Emoji("🆗");
            await m_userMessage.AddReactionAsync(successEmoji);
            return;
        }
        private string ZenToHan(string textData)
        {
            var convertText = textData;
            convertText = Regex.Replace(convertText, "　", p => ((char)(p.Value[0] - '　' + ' ')).ToString());
            convertText = Regex.Replace(convertText, "[０-９]", p => ((char)(p.Value[0] - '０' + '0')).ToString());
            convertText = Regex.Replace(convertText, "[ａ-ｚ]", p => ((char)(p.Value[0] - 'ａ' + 'a')).ToString());
            convertText = Regex.Replace(convertText, "[Ａ-Ｚ]", p => ((char)(p.Value[0] - 'Ａ' + 'A')).ToString());
            return convertText;
        }
    }
}
