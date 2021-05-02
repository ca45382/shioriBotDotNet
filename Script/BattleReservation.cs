using Discord;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataTypes;
using PriconneBotConsoleApp.MySQL;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriconneBotConsoleApp.Script
{
    class BattleReservation : BaseClass
    {
        private const int MinBossNumber = 1;
        private const int MaxBossNumber = 5;

        private const int LimitReservationLap = 2;

        private const int ReservableStartTimeHour = 18;

        private readonly ClanData m_userClanData;
        private readonly SocketRole m_userRole;
        private readonly SocketUserMessage m_userMessage;
        private readonly SocketReaction m_userReaction;
        
        private BattleReservation(
            ClanData userClanData,
            ISocketMessageChannel channel,
            SocketUserMessage userMessage = null,
            SocketReaction userReaction = null)
        {
            m_userClanData = userClanData;
            m_userRole = (channel as SocketGuildChannel)?.Guild.GetRole(ulong.Parse(m_userClanData.ClanRoleID));
            m_userMessage = userMessage;
            m_userReaction = userReaction;
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
            var userMessage = m_userMessage;

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
                        await SendMessageToChannel(userMessage.Channel, CreateUserReservationDataMessage());
                        return;
                }

                if (DateTime.Now.Hour < ReservableStartTimeHour)
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

                if (m_userClanData.BossNumber == reservationData.BossNumber 
                    && m_userClanData.BattleLap == reservationData.BattleLap)
                {
                    await new BattleDeclaration(m_userClanData,m_userMessage).UpdateDeclarationBotMessage();
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
            if (m_userMessage.Content.StartsWith("!start"))
            {
                await SendSystemMessage();
            }
        }

        public async Task SendSystemMessage()
        {
            var userClanData = m_userClanData;
            var userRole = m_userRole;
            var messageData = CreateAllReservationDataMessage(userClanData);

            var resultChannel = userRole.Guild
                .GetTextChannel(ulong.Parse(userClanData.ChannelIDs.ReservationResultChannelID));

            var sendedMessageData = await SendMessageToChannel(resultChannel, messageData);

            new MySQLReservationController()
                .UpdateReservationMessageID(userClanData, sendedMessageData.Id.ToString());
        }

        public async Task UpdateSystemMessage()
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
                var message = await guildChannel.GetMessageAsync( ulong.Parse(reservationMessageID));

                if (message != null)
                {
                    await guildChannel.DeleteMessageAsync(message);
                    await SendSystemMessage();
                }

                return;
            }

            var serverMessage = socketMessage as SocketUserMessage;
            var messageData = CreateAllReservationDataMessage(userClanData);
            await EditMessage(serverMessage, messageData);
        }

        /// <summary>
        /// 受信した予約データを解析して保存する
        /// 例 : 「予約 35 1 1200万程度」
        /// 「予約 周回 ボス メモ(任意)」
        /// </summary>
        /// <returns></returns>
        private ReservationData MessageToReservationData()
        {
            var userClanData = m_userClanData;
            var userMessage = m_userMessage;
            
            var splitMessageContent =
                ZenToHan(userMessage.Content).Split(new string[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

            if (splitMessageContent.Length < 3
                || !(int.TryParse(splitMessageContent[1], out int battleLap) && battleLap > 0)
                || !(int.TryParse(splitMessageContent[2], out int bossNumber) && bossNumber <= MaxBossNumber && bossNumber >= MinBossNumber)
                || battleLap < userClanData.BattleLap
                || battleLap > userClanData.BattleLap + LimitReservationLap
                || battleLap == userClanData.BattleLap && bossNumber < userClanData.BossNumber
                || battleLap == userClanData.BattleLap + LimitReservationLap && bossNumber > userClanData.BossNumber)
            {
                return null;
            }
            
            return new ReservationData() {
                PlayerData = new PlayerData
                {
                    ClanData = userClanData,
                    UserID = userMessage.Author.Id.ToString()
                },
                BattleLap = battleLap,
                BossNumber = bossNumber,
                CommentData = string.Join(" ", splitMessageContent.Skip(3))
            };
        }

        private void RegisterReservationData(ReservationData reservationData)
        {
            var mySQLReservationController = new MySQLReservationController();

            var allSqlReservationData = mySQLReservationController
                .LoadReservationData(reservationData.PlayerData);

            var doesExistReservationData = allSqlReservationData
                .Any(x => x.BossNumber == reservationData.BossNumber && x.BattleLap == reservationData.BattleLap);

            if (!doesExistReservationData)
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
            var splitMessageContent = m_userMessage.Content
                .Split(new[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

            if (splitMessageContent.Length != 4
                || !ulong.TryParse(splitMessageContent[1], out ulong userID)
                || !(int.TryParse(splitMessageContent[2], out int battleLap) && battleLap > 0)
                || !(int.TryParse(splitMessageContent[3], out int bossNumber) && bossNumber <= MaxBossNumber && bossNumber >= MinBossNumber))
            {
                return null;
            }

            var playerData = new MySQLPlayerDataController()
                .LoadPlayerData(m_userClanData.ServerID, userID.ToString());

            if (playerData == null)
            {
                return null;
            }

            return new MySQLReservationController()
                .LoadReservationData(playerData)
                .FirstOrDefault(d => d.BattleLap == battleLap && d.BossNumber == bossNumber);
        }

        private bool DeleteUserReservationData(ReservationData reservationData)
        {
            var mySQLReservationController = new MySQLReservationController();
            var allSqlReservationData = mySQLReservationController.LoadReservationData(reservationData.PlayerData);

            var sqlReservationData = allSqlReservationData
                .Where(x => x.BossNumber == reservationData.BossNumber && x.BattleLap == reservationData.BattleLap)
                .ToList();

            if (sqlReservationData.Count == 0)
            {
                return false;
            }

            mySQLReservationController.DeleteReservationData(sqlReservationData);

            return true;
        }

        private string CreateUserReservationDataMessage()
            => CreateUserReservationDataMessage(
                new MySQLPlayerDataController().LoadPlayerData(m_userClanData.ServerID, m_userMessage.Author.Id.ToString())
            );

        private string CreateUserReservationDataMessage(PlayerData playerData)
        {
            var reservationDataSet = new MySQLReservationController().LoadReservationData(playerData);

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
            var bossNumber = clanData.BossNumber;
            var battleLap = clanData.BattleLap;

            var reservationDataSet = new MySQLReservationController().LoadReservationData(clanData);

            reservationDataSet = reservationDataSet
                .Where(b => b.BattleLap > battleLap || (b.BattleLap == battleLap && b.BossNumber >= bossNumber))
                .ToList();


            if (!reservationDataSet.Any())
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

        private async Task FailedToRegisterMessage()
            => await m_userMessage.Channel.SendMessageAsync("予約に失敗しました。");

        private async Task OutOfReservationTime()
            => await m_userMessage.Channel.SendMessageAsync("予約できません。予約可能時間は18:00～23:59です。");

        private async Task SuccessAddEmoji()
            => await m_userMessage.AddReactionAsync(new Emoji("🆗"));

    }
}
