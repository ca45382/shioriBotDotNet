using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using Discord;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataTypes;
using PriconneBotConsoleApp.Enum;
using PriconneBotConsoleApp.Database;

namespace PriconneBotConsoleApp.Script
{
    class BattleReservation : BaseClass
    {
        private const int MinBossNumber = 1;
        private const int MaxBossNumber = 5;

        private const int LimitReservationLap = 2;

        private const int ReservableStartTimeHour = 18;

        private const int MaxCommentLength = 30;

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
            m_userRole = (channel as SocketGuildChannel)?.Guild.GetRole(m_userClanData.ClanRoleID);
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

#if !DEBUG
                if (DateTime.Now.Hour < ReservableStartTimeHour)
                {
                    await SendErrorMessage(ErrorType.OutOfReservationTime, $"{ReservableStartTimeHour}:00", "24:00");
                    return;
                }
#endif

                var reservationData = MessageToReservationData();

                if (reservationData is null)
                {
                    await SendErrorMessage(ErrorType.FailedReservation);
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

        public async Task RunReservationResultReaction()
        {
            switch (m_userReaction.Emote.Name)
            {
                case "🔄":
                    await UpdateSystemMessage();
                    break;
            }
            await RemoveUserReaction();
        }

        public async Task SendSystemMessage()
        {
            var userClanData = m_userClanData;
            var userRole = m_userRole;
            var messageData = CreateAllReservationDataMessage(userClanData);

            var resultChannel = userRole.Guild
                .GetTextChannel(userClanData.ChannelIDs.ReservationResultChannelID);

            var sendedMessageData = await SendMessageToChannel(resultChannel, messageData);

            new DatabaseReservationController()
                .UpdateReservationMessageID(userClanData, sendedMessageData.Id);
            await AttacheDefaultReaction(sendedMessageData);
        }

        public async Task UpdateSystemMessage()
        {
            var userClanData = m_userClanData;
            var userRole = m_userRole;
            var reservationMessageID = userClanData.MessageIDs.ReservationMessageID;

            if (reservationMessageID == 0)
            {
                return;
            }

            var guildChannel = userRole.Guild
                .GetChannel(userClanData.ChannelIDs.ReservationResultChannelID) as SocketTextChannel;

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
                || !(byte.TryParse(splitMessageContent[1], out byte battleLap) && battleLap > 0)
                || !(byte.TryParse(splitMessageContent[2], out byte bossNumber) && bossNumber <= MaxBossNumber && bossNumber >= MinBossNumber)
                || battleLap < userClanData.BattleLap
                || battleLap > userClanData.BattleLap + LimitReservationLap
                || battleLap == userClanData.BattleLap && bossNumber < userClanData.BossNumber
                || battleLap == userClanData.BattleLap + LimitReservationLap && bossNumber > userClanData.BossNumber)
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
            var mySQLReservationController = new DatabaseReservationController();

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

        /// <summary>
        /// 個人が予約しているデータの取得
        /// </summary>
        /// <returns></returns>
        private ReservationData MessageToUserReservationData()
        {
            var splitMessageContent = m_userMessage.Content
                .Split(new[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

            if (splitMessageContent.Length != 4
                || !ulong.TryParse(splitMessageContent[1], out ulong userID)
                || !(byte.TryParse(splitMessageContent[2], out byte battleLap) && battleLap > 0)
                || !(byte.TryParse(splitMessageContent[3], out byte bossNumber) && bossNumber <= MaxBossNumber && bossNumber >= MinBossNumber))
            {
                return null;
            }

            var playerData = new DatabasePlayerDataController()
                .LoadPlayerData(m_userClanData.ServerID, userID);

            if (playerData == null)
            {
                return null;
            }

            return new DatabaseReservationController()
                .LoadReservationData(playerData)
                .FirstOrDefault(d => d.BattleLap == battleLap && d.BossNumber == bossNumber);
        }

        private bool DeleteUserReservationData(ReservationData reservationData)
        {
            var mySQLReservationController = new DatabaseReservationController();
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
                new DatabasePlayerDataController().LoadPlayerData(m_userClanData.ServerID, m_userMessage.Author.Id)
            );

        private string CreateUserReservationDataMessage(PlayerData playerData)
        {
            var reservationDataSet = new DatabaseReservationController().LoadReservationData(playerData);

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

            var reservationDataSet = new DatabaseReservationController().LoadReservationData(clanData);

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
            var textChannnel = m_userRole.Guild.GetTextChannel(m_userClanData.ChannelIDs.ReservationResultChannelID);

            var message = await textChannnel.GetMessageAsync(m_userReaction.MessageId);

            if (message == null)
            {
                return;
            }
            await message.RemoveReactionAsync(m_userReaction.Emote, m_userReaction.User.Value);
        }
        
        private async Task SendErrorMessage(ErrorType type, params string[] parameters)
        {
            var descriptionAttribute = type.GetType().GetField(type.ToString()).GetCustomAttribute<DescriptionAttribute>(false);
            var sendMessage = string.Empty;
            if (descriptionAttribute == null)
            {
                sendMessage = type.ToString();
            }
            else
            {
                sendMessage = string.Format(descriptionAttribute.Description, parameters);
            }
            await m_userMessage.Channel.SendMessageAsync(sendMessage);
        }

        private async Task SuccessAddEmoji()
            => await m_userMessage.AddReactionAsync(new Emoji("🆗"));

    }
}
