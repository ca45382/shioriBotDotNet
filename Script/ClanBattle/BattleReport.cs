using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Define;
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.Script
{
    public class BattleReport
    {
        private readonly CommandEventArgs m_CommandEventArgs;

        private class PlayerInfo
        {
            public ulong PlayerID;
            public string PlayerGuildName;
            public ReportData[] ReportData;

            public PlayerInfo(ulong playerID, string playerGuildName, ReportData[] reportData = null)
            {
                PlayerID = playerID;
                PlayerGuildName = playerGuildName;
                ReportData = reportData ?? new ReportData[0];
            }

            public string GetNameWithReport()
            {
                if (ReportData.Length == 0)
                {
                    return PlayerGuildName;
                }

                var stringData = string.Join(
                    ',',
                    ReportData.Select(x => $"{x.BossNumber}{((AttackType)x.AttackType).ToLabel()}").ToArray()
                );
                return $"{PlayerGuildName}({stringData})";
            }
        }

        public BattleReport(CommandEventArgs commandEventArgs)
        {
            m_CommandEventArgs = commandEventArgs;
        }

        /// <summary>
        /// 個人の凸報告
        /// </summary>
        public void RegisterReportData()
        {
            var reportData = new ReportData();

            if (Regex.IsMatch(m_CommandEventArgs.Name, @"\d\D{1,3}"))
            {
                var bossNumber = int.Parse(Regex.Match(m_CommandEventArgs.Name, @"\d").Value);

                if (!EnumMapper.TryParse<AttackType>(Regex.Match(m_CommandEventArgs.Name, @"\D{1,3}").Value, out var attackType)
                    || attackType == AttackType.Unknown || attackType == AttackType.CarryOver)
                {
                    return;
                }

                reportData.AttackType = (byte)attackType;
                reportData.BossNumber = (byte)bossNumber;
            }

            if (reportData.BossNumber < CommonDefine.MinBossNumber || reportData.BossNumber > CommonDefine.MaxBossNumber)
            {
                return;
            }

            reportData.PlayerID = m_CommandEventArgs.PlayerData.PlayerID;

            if (reportData == null)
            {
                return;
            }

            var userReportedData = DatabaseReportDataController.GetReportData(m_CommandEventArgs.PlayerData);

            if (userReportedData.Count() >= CommonDefine.MaxReportNumber)
            {
                _ = m_CommandEventArgs.SocketUserMessage.Channel.SendTimedMessageAsync(TimeDefine.ErrorMessageDisplayTime, ErrorType.UpperLimitReport.ToLabel());
                return;
            }

            if (DatabaseReportDataController.CreateReportData(reportData))
            {
                _ = m_CommandEventArgs.SocketUserMessage.AddReactionAsync(new Emoji(ReactionType.Success.ToLabel()));
            }

            return;
        }

        /// <summary>
        /// 個人の最新の凸報告を削除する。
        /// </summary>
        public void DeleteReportData()
        {
            var playerData = new PlayerData();
            if (m_CommandEventArgs.Arguments.Count == 1
                && ulong.TryParse(Regex.Match(m_CommandEventArgs.Arguments[0], @"\d+").Value, out ulong registerUserID))
            {
                playerData = DatabasePlayerDataController.LoadPlayerData(m_CommandEventArgs.Role, registerUserID);
            }
            else
            {
                playerData = m_CommandEventArgs.PlayerData;
            }

            if (playerData == null)
            {
                return;
            }

            var removeData = DatabaseReportDataController.GetReportData(playerData)
                .OrderBy(x => x.DateTime).Last();

            if (DatabaseReportDataController.DeleteReportData(removeData))
            {
                _ = m_CommandEventArgs.SocketUserMessage.AddReactionAsync(new Emoji(ReactionType.Success.ToLabel()));

                if (playerData.UserID != m_CommandEventArgs.User.Id)
                {
                    _ = m_CommandEventArgs.SocketUserMessage.Channel.SendTimedMessageAsync(TimeDefine.SuccessMessageDisplayTime,
                        string.Format(InfomationType.DeleteInsted.ToLabel(), playerData.UserID, TimeDefine.SuccessMessageDisplayTime));
                }
            }
        }

        /// <summary>
        /// 代理報告用
        /// </summary>
        public void RegisterOtherUserReportData()
        {
            if ((!ulong.TryParse(m_CommandEventArgs.Arguments[0], out var registerUserID)
                && !MentionUtils.TryParseUser(m_CommandEventArgs.Arguments[0], out registerUserID))
                || !byte.TryParse(m_CommandEventArgs.Arguments[1], out var bossNumber)
                || bossNumber > CommonDefine.MaxBossNumber || bossNumber < CommonDefine.MinBossNumber
                || !EnumMapper.TryParse<AttackType>(m_CommandEventArgs.Arguments[2], out var attackType))
            {
                return;
            }

            var registerPlayerData = DatabasePlayerDataController.LoadPlayerData(m_CommandEventArgs.Role, registerUserID);

            if (registerPlayerData == null)
            {
                return;
            }

            var reportData = new ReportData()
            {
                AttackType = (byte)attackType,
                BossNumber = bossNumber,
                PlayerID = registerUserID,
            };

            var userReportedData = DatabaseReportDataController.GetReportData(registerPlayerData);

            if (userReportedData.Count() >= CommonDefine.MaxReportNumber)
            {
                _ = m_CommandEventArgs.Channel.SendTimedMessageAsync(TimeDefine.ErrorMessageDisplayTime, ErrorType.UpperLimitReport.ToLabel());
                return;
            }

            if (DatabaseReportDataController.CreateReportData(reportData))
            {
                _ = m_CommandEventArgs.SocketUserMessage.AddReactionAsync(EnumMapper.ToEmoji(ReactionType.Success));
            }

            return;
        }

        /// <summary>
        /// クランの凸報告を削除
        /// </summary>
        public void DeleteAllClanReport()
        {
            var clanReportData = DatabaseReportDataController.GetReportData(m_CommandEventArgs.ClanData);
            if (DatabaseReportDataController.DeleteReportData(clanReportData))
            {
                _ = m_CommandEventArgs.SocketUserMessage.AddReactionAsync(EnumMapper.ToEmoji(ReactionType.Success));
                
            }
        }

        /// <summary>
        /// 凸報告データ送信用
        /// </summary>
        /// <returns></returns>
        public async Task SendClanAttackList()
        {
            var clanAttackEmbed = CreateClanReportData();
            await m_CommandEventArgs.SocketUserMessage.Channel.SendMessageAsync(embed: clanAttackEmbed);
        }

        /// <summary>
        /// クランごとの凸報告データをEmbedで返す。
        /// </summary>
        /// <returns></returns>
        private Embed CreateClanReportData()
        {
            var embedBuilder = new EmbedBuilder();

            var clanPlayerDataList = DatabasePlayerDataController.LoadPlayerData(m_CommandEventArgs.ClanData);
            var reportDataList = DatabaseReportDataController.GetReportData(m_CommandEventArgs.ClanData);

            var playerInfoList = clanPlayerDataList.Select(x => new PlayerInfo(
                x.PlayerID,
                x.GuildUserName,
                reportDataList.Where(r => r.PlayerID == x.PlayerID).ToArray()
            ));

            for (int i = 0; i <= CommonDefine.MaxReportNumber; i++)
            {
                var players = playerInfoList.Where(x => x.ReportData.Length == i);

                if (players.Count() > CommonDefine.MaxClanPlayer)
                {
                    players = players.Take(CommonDefine.MaxClanPlayer);
                }

                var reportStringBuilder = new StringBuilder();
                var reportMessage = string.Join("\n", players.Select(x => x.GetNameWithReport()).ToArray());

                var nameHeader = i == 0 ? "未" : i.ToString();
                var embedFieldBuilder = new EmbedFieldBuilder()
                {
                    Name = $"▼{nameHeader}凸の方({players.Count()}人)",
                    Value = reportMessage.Length == 0 ? "該当者なし" : reportMessage,
                    IsInline = true,
                };

                embedBuilder.AddField(embedFieldBuilder);
            }

            var nowAttackCount = reportDataList.Count();
            var memberAttackCount = clanPlayerDataList.Count() * CommonDefine.MaxReportNumber;
            var MaxAllReportNumber = CommonDefine.MaxClanPlayer * CommonDefine.MaxReportNumber;
            var memberAttackString = memberAttackCount > CommonDefine.MaxReportNumber ? $"{MaxAllReportNumber}+" : memberAttackCount.ToString();
            embedBuilder.Title = $"凸状況({nowAttackCount}凸/ {memberAttackString}凸)";

            embedBuilder.Footer = new EmbedFooterBuilder()
            {
                Text = $"最終更新時刻 : {DateTime.Now:T}",
            };

            return embedBuilder.Build();
        }
    }
}
