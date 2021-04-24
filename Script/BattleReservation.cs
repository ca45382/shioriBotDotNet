using Discord.WebSocket;
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
    class BattleReservation:BaseClass
    {
        private const int MinBossNumber = 1;
        private const int MaxBossNumber = 5;

        private const int LimitReservationLap = 2;

        private const int ReservableStartTimeHour = 18;

        private ClanData m_userClanData;
        private SocketRole m_userRole;
        private SocketUserMessage m_userMessage;
        private SocketReaction m_userReaction;
        
        public BattleReservation(ClanData userClanData, SocketUserMessage message)
        {
            m_userClanData = userClanData;
            m_userMessage = message;

            var socketGuildChannel = message.Channel as SocketGuildChannel;

            m_userRole = socketGuildChannel.Guild.GetRole(ulong.Parse(m_userClanData.ClanRoleID));
        }

        public BattleReservation(ClanData userClanData, SocketReaction reaction)
        {
            m_userClanData = userClanData;
            m_userReaction = reaction;
            var socketGuildChannel = m_userReaction.Channel as SocketGuildChannel;

            m_userRole = socketGuildChannel.Guild.GetRole(ulong.Parse(m_userClanData.ClanRoleID));
        }

        async public Task RunReservationCommand()
        {
            var userMessage = m_userMessage;

            if (userMessage == null) return;
            var messageContents = userMessage.Content;
            
            if (messageContents.StartsWith("予約"))
            {
                if (messageContents == "予約" || messageContents == "予約確認"
                    || messageContents == "予約状況")
                {
                    Console.WriteLine("予約確認");
                    var sendMessageData = CreateUserReservationDataMessage();
                    await SendMessageToChannel(userMessage.Channel,sendMessageData);
                    return;
                }

                var currentHour = DateTime.Now.Hour;
                if (currentHour < ReservableStartTimeHour )
                {
                    await OutOfReservationTime();
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
                await UpdateSystemMessage();
                
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
                    await UpdateSystemMessage();
                }
                return;
            }
            else if (messageContents.StartsWith("!rm"))
            {
                var userReservationData = MessageToUserReservationData();
                if (userReservationData == null)
                {
                    return;
                }
                var result = DeleteUserReservationData(userReservationData);

                if (result == false)
                {
                    return;
                }

            }

            return;
        }


        async public Task RunReservationResultCommand()
        {
            var userMessage = m_userMessage;

            var messageContents = userMessage.Content;
            if (messageContents.StartsWith("!start"))
            {
                await SendSystemMessage();
            }
            return;
        }

        async public Task SendSystemMessage()
        {
            var userClanData = m_userClanData;
            var userRole = m_userRole;

            var messageData = CreateAllReservationDataMessage(userClanData);
            var resultChannel = userRole.Guild.GetTextChannel(
                ulong.Parse(userClanData.ChannelIDs.ReservationResultChannelID));

            var sendedMessageData = await SendMessageToChannel(resultChannel, messageData);
            new MySQLReservationController().UpdateReservationMessageID(
                userClanData, sendedMessageData.Id.ToString());

            return;
        }

        async public Task UpdateSystemMessage()
        {
            var userClanData = m_userClanData;
            var userRole = m_userRole;

            var reservationMessageID = userClanData.MessageIDs.ReservationMessageID;
            if (reservationMessageID == null)
            {
                return;
            }
            var guildChannel = userRole.Guild.GetChannel(
                ulong.Parse(userClanData.ChannelIDs.ReservationResultChannelID)) as SocketTextChannel;


            var socketMessage = guildChannel.GetCachedMessage(
                ulong.Parse(reservationMessageID));

            if (socketMessage == null || !(socketMessage is SocketUserMessage))
            {
                var message = await guildChannel.GetMessageAsync(
                    ulong.Parse(reservationMessageID));
                if (message == null)
                {
                    return;
                }

                await guildChannel.DeleteMessageAsync(message);
                await SendSystemMessage();
                return;
            }

            SocketUserMessage serverMessage;
            serverMessage = socketMessage as SocketUserMessage;

            var messageData = CreateAllReservationDataMessage(userClanData);
            await EditMessage(serverMessage, messageData);
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
            var userClanData = m_userClanData;
            var userMessage = m_userMessage;

            var massageContent = ZenToHan(userMessage.Content);
            
            var splitMessageContent =
                massageContent.Split(new string[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

            if ((splitMessageContent.Length >= 3 && splitMessageContent.Length <= 4) == false )
            { 
                return null;
            }

            if (!(int.TryParse(splitMessageContent[1], out int battleLap)
                && battleLap > 0))
            {
                return null;
            }
            if (!(int.TryParse(splitMessageContent[2], out int bossNumber)
                && bossNumber <= MaxBossNumber && bossNumber >= MinBossNumber))
            {
                return null;
            }

            if (battleLap < userClanData.BattleLap || battleLap > ( userClanData.BattleLap + LimitReservationLap) 
                || (battleLap == userClanData.BattleLap && bossNumber < userClanData.BossNumber )
                || (battleLap == (userClanData.BattleLap + LimitReservationLap) && bossNumber > userClanData.BossNumber ))
            {
                return null;
            }
            
            var userID = userMessage.Author.Id.ToString();
            string commentData = null;

            if (splitMessageContent.Length == 4)
            {
                commentData = splitMessageContent[3];
            }


            return new ReservationData() {
                PlayerData = new PlayerData()
                {
                    ClanData = userClanData,
                    UserID = userID
                },
                BattleLap = battleLap,
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
                .Where(y => y.BattleLap == reservationData.BattleLap);

            if (sqlReservationData.Count() == 0)
            {
                mySQLReservationController.CreateReservationData(reservationData);
            }
            else
            {
                mySQLReservationController.UpdateReservationData(reservationData);
            }
        }


        private ReservationData MessageToUserReservationData()
        {

            var userClanData = m_userClanData;
            var userMessage = m_userMessage;

            var splitMessageContent =
                userMessage.Content.Split(new string[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

            if (splitMessageContent.Length != 4)
            {
                return null;
            }
            if (!(ulong.TryParse(splitMessageContent[1], out ulong userID)))
            {
                return null;
            }
            if (!(int.TryParse(splitMessageContent[2], out int battleLap)
                && battleLap > 0))
            {
                return null;
            }
            if (!(int.TryParse(splitMessageContent[3], out int bossNumber)
                && bossNumber <= MaxBossNumber && bossNumber >= MinBossNumber))
            {
                return null;
            }

            var playerData = new MySQLPlayerDataController().LoadPlayerData(
                m_userClanData.ServerID, userID.ToString());

            if (playerData == null)
            {
                return null;
            }

            var allSqlReservationData =
                new MySQLReservationController().LoadReservationData(playerData);

            var reservationData = allSqlReservationData
                .FirstOrDefault(d => d.BattleLap == battleLap && d.BossNumber == bossNumber);

            return reservationData;

        }

        private bool DeleteUserReservationData(ReservationData reservationData)
        {
            var mySQLReservationController = new MySQLReservationController();
            var allSqlReservationData =
                mySQLReservationController.LoadReservationData(reservationData.PlayerData);

            var sqlReservationData = allSqlReservationData
                .Where(x => x.BossNumber == reservationData.BossNumber)
                .Where(y => y.BattleLap == reservationData.BattleLap);
            if (sqlReservationData.Count() == 0)
            {
                return false;
            }
            mySQLReservationController.DeleteReservationData(sqlReservationData);

            return true;
        }

        private string CreateUserReservationDataMessage()
        {
            var userClanData = m_userClanData;
            var userMessage = m_userMessage;

            var playerData = new MySQLPlayerDataController()
                .LoadPlayerData(userClanData.ServerID, userMessage.Author.Id.ToString());

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
                    reservationData.BattleLap.ToString().PadLeft(2) + "周目 " +
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
            var bossNumber = clanData.BossNumber;
            var battleLap = clanData.BattleLap;

            var reservationDataSet = new MySQLReservationController().LoadReservationData(clanData);

            reservationDataSet = reservationDataSet
                .Where(b => b.BattleLap > battleLap || (b.BattleLap == battleLap && b.BossNumber >= bossNumber))
                .ToList();


            if (reservationDataSet.Count() == 0)
            {
                return "予約がありません";
            }
            

            var messageData = "```python\n";
            messageData += $"{battleLap}周目, {bossNumber}ボス以降の予約一覧です. \n";

            var loopNum = 0;
            foreach (var reservationData in reservationDataSet)
            {
                loopNum += 1;
                messageData += loopNum.ToString().PadLeft(2) + ". " +
                    reservationData.BattleLap.ToString().PadLeft(2) + "周目 " +
                    reservationData.BossNumber.ToString() + "ボス " +
                    reservationData.PlayerData.GuildUserName + " " +
                    reservationData.CommentData +
                    "\n";
            }
            messageData += $"以上の{loopNum}件です";
            messageData += "```";

            return messageData;
        }

        private bool SendDeleteUserReserve(ReservationData reservationData)
        {
            var userMessage = m_userMessage;

            return true;
        }
        async private Task FailedToRegisterMessage()
        {
            var userMessage = m_userMessage;
            var textMessage = "予約に失敗しました。";

            
            await userMessage.Channel.SendMessageAsync(textMessage);
            return;
        }

        private async Task OutOfReservationTime()
        {
            var userMessage = m_userMessage;
            var textMessage = "予約できません。予約可能時間は18:00～23:59です。";


            await userMessage.Channel.SendMessageAsync(textMessage);
            return;
        }

        async private Task SuccessAddEmoji()
        {
            var message = m_userMessage;

            var successEmoji = new Emoji("🆗");
            await message.AddReactionAsync(successEmoji);
            return;
        }

    }
}
