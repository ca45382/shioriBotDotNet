using System;
using System.Linq;
using System.Text;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.Model;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Define;
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.Script
{
    public class BattleReservation
    {
        private const int MaxCommentLength = 30;

        private readonly CommandEventArgs m_CommandEventArgs;

        public BattleReservation(CommandEventArgs commandEventArgs)
            => m_CommandEventArgs = commandEventArgs;

        /// <summary>
        /// 個人の予約一覧を表示する。引数は無し。
        /// </summary>
        public void PlayerReserveList()
            => m_CommandEventArgs.Channel.SendMessageAsync(CreateUserReservationDataMessage(m_CommandEventArgs.PlayerData));

        public void RegisterReserveData()
        {
            if (!IsReservationAllowTime())
            {
                _ = m_CommandEventArgs.Channel.SendTimedMessageAsync(
                    TimeDefine.ErrorMessageDisplayTime,
                    string.Format(ErrorType.OutOfReservationTime.ToLabel(),
                        $"{m_CommandEventArgs.ClanData.ReservationStartTime.Hours}",
                        $"{m_CommandEventArgs.ClanData.ReservationEndTime.Hours}"
                    )
                );

                return;
            }

            var reservationData = MessageToReservationData();

            if (reservationData is null)
            {
                _ = m_CommandEventArgs.Channel.SendTimedMessageAsync(
                    TimeDefine.ErrorMessageDisplayTime,
                    ErrorType.FailedReservation.ToLabel()
                    );

                return;
            }

            var allowMinReservationLap = m_CommandEventArgs.ClanData.GetBossLap(reservationData.BossNumber);

            if (reservationData.BattleLap < allowMinReservationLap)
            {
                _ = m_CommandEventArgs.Channel.SendTimedMessageAsync(
                    TimeDefine.ErrorMessageDisplayTime,
                    string.Format(ErrorType.OutOfMinReservationBossLaps.ToLabel(), allowMinReservationLap.ToString())
                    );

                return;
            }

            var allowMaxReservationLap = m_CommandEventArgs.ClanData.ReservationLap == 0
                ? ClanBattleDefine.MaxLapNumber
                : (m_CommandEventArgs.ClanData.ReservationLap + m_CommandEventArgs.ClanData.GetMinBossLap());

            if (reservationData.BattleLap > allowMaxReservationLap)
            {
                _ = m_CommandEventArgs.Channel.SendTimedMessageAsync(
                    TimeDefine.ErrorMessageDisplayTime,
                    string.Format(ErrorType.OutOfMaxReservationBossLaps.ToLabel(), allowMaxReservationLap.ToString())
                    );

                return;
            }

            RegisterReservationData(reservationData);
            _ = m_CommandEventArgs.SocketUserMessage.AddReactionAsync(ReactionType.Success.ToEmoji());
            _ = new BattleReservationSummary(m_CommandEventArgs.Role, m_CommandEventArgs.ClanData).UpdateMessage();

            if (m_CommandEventArgs.ClanData.GetBossLap(reservationData.BossNumber) == reservationData.BattleLap)
            {
                _ = new BattleDeclaration(m_CommandEventArgs.Role, (BossNumberType)reservationData.BossNumber).UpdateDeclarationBotMessage();
            }
        }

        public void DeleteReserveData()
        {
            var deleteReservationData = MessageToReservationData();

            if (deleteReservationData == null)
            {
                return;
            }

            if (DeleteUserReservationData(deleteReservationData))
            {
                _ = m_CommandEventArgs.SocketUserMessage.AddReactionAsync(ReactionType.Success.ToEmoji());
                _ = new BattleReservationSummary(m_CommandEventArgs.Role, m_CommandEventArgs.ClanData).UpdateMessage();

                if (m_CommandEventArgs.ClanData.GetBossLap(deleteReservationData.BossNumber) == deleteReservationData.BattleLap)
                {
                    _ = new BattleDeclaration(m_CommandEventArgs.Role, (BossNumberType)deleteReservationData.BossNumber).UpdateDeclarationBotMessage();
                }
            }
        }

        /// <summary>
        /// 受信した予約データを解析して保存する
        /// 例 : 「予約 35 1 1200万程度」
        /// 「予約 周回 ボス メモ(任意)」
        /// </summary>
        /// <returns></returns>
        private ReservationData MessageToReservationData()
        {
            if (!byte.TryParse(m_CommandEventArgs.Arguments[1], out var bossNumber)
                || !byte.TryParse(m_CommandEventArgs.Arguments[0], out var battleLap)
                || !CommonDefine.IsValidBossNumber(bossNumber)
                || !CommonDefine.IsValidBattleLap(battleLap))
            {
                return null;
            }

            var commentData = string.Join(' ', m_CommandEventArgs.Arguments.Skip(2));

            if (commentData.Length > MaxCommentLength)
            {
                commentData = commentData.Substring(0, MaxCommentLength);
                _ = m_CommandEventArgs.Channel.SendTimedMessageAsync(TimeDefine.WarningMessageDisplayTime, WarningType.TooLongComment.ToLabel());
            }

            return new ReservationData()
            {
                PlayerID = m_CommandEventArgs.PlayerData.PlayerID,
                BattleLap = battleLap,
                BossNumber = bossNumber,
                CommentData = commentData,
            };
        }

        private void RegisterReservationData(ReservationData reservationData)
        {
            var allSqlReservationData = DatabaseReservationController.LoadReservationData(m_CommandEventArgs.PlayerData);

            var DatabaseReservationData = allSqlReservationData
                .FirstOrDefault(x => x.BossNumber == reservationData.BossNumber && x.BattleLap == reservationData.BattleLap);

            if (DatabaseReservationData == null)
            {
                DatabaseReservationController.CreateReservationData(reservationData);
            }
            else
            {
                reservationData.ReserveID = DatabaseReservationData.ReserveID;
                DatabaseReservationController.UpdateReservationData(reservationData);
            }
        }

        private bool DeleteUserReservationData(ReservationData reservationData)
        {
            var userReservationDataList = DatabaseReservationController.LoadReservationData(m_CommandEventArgs.PlayerData);

            var sqlReservationData = userReservationDataList
                .Where(x => x.BossNumber == reservationData.BossNumber && x.BattleLap == reservationData.BattleLap)
                .ToList();

            if (sqlReservationData.Count == 0)
            {
                return false;
            }

            DatabaseReservationController.DeleteReservationData(sqlReservationData);

            return true;
        }

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

        /// <summary>
        /// 予約できる時間かどうか判断する。
        /// </summary>
        /// <returns></returns>
        private bool IsReservationAllowTime()
        {
            if (m_CommandEventArgs.ClanData == null)
            {
                return false;
            }

            var startTime = m_CommandEventArgs.ClanData.ReservationStartTime;
            var endTime = m_CommandEventArgs.ClanData.ReservationEndTime;
            var nowTime = DateTime.Now.TimeOfDay;

            if (startTime.Hours == 0 && endTime.Hours == 0)
            {
                return true;
            }

            if (startTime.Hours < TimeDefine.GameDateOffset.Hours)
            {
                startTime = startTime.Add(new TimeSpan(1, 0, 0, 0));
            }

            if (endTime.Hours < TimeDefine.GameDateOffset.Hours)
            {
                endTime = endTime.Add(new TimeSpan(1, 0, 0, 0));
            }

            if (nowTime.Hours < TimeDefine.GameDateOffset.Hours)
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
