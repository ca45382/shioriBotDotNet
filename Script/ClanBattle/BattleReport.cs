using Discord;
using Discord.WebSocket;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Define;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PriconneBotConsoleApp.Script
{
    public class BattleReport : BaseClass
    {

        private readonly ClanData m_ClanData;
        private readonly SocketUserMessage m_userMessage;
        private readonly SocketRole m_ClanRole;
        private readonly SocketGuild m_Guild;

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
                // TODO : 
                var stringData = string.Join(
                    ',', 
                    ReportData.Select(x => $"{x.BossNumber}{AttackNumberToString(x.AttackType)}").ToArray()
                );
                return $"{PlayerGuildName}({stringData})";
            }
        }

        public BattleReport(ClanData clanData, SocketUserMessage userMessage)
        {
            m_ClanData = clanData;
            m_userMessage = userMessage;
            m_Guild = (userMessage.Channel as SocketTextChannel)?.Guild;
            m_ClanRole = m_Guild?.GetRole(clanData.ClanRoleID);
        }

        public async Task RunByMessage()
        {
            if (m_userMessage.Content.StartsWith("!"))
            {
                if (m_userMessage.Content.StartsWith("!add"))
                {
                    RegisterOtherUserReportData();
                }
                else if (m_userMessage.Content.StartsWith("!list"))
                {
                    await SendClanAttackList();
                }
                else if (m_userMessage.Content.StartsWith("!rm"))
                {
                    DeleteReportData();
                }

            }
            else
            {
                RegisterReportData();
            }
        }

        /// <summary>
        /// 個人の凸報告
        /// </summary>
        private void RegisterReportData()
        {
            var playerData = DatabasePlayerDataController.LoadPlayerData(m_ClanRole, m_userMessage.Author.Id);

            if (playerData == null)
            {
                return;
            }

            var reportData = StringToReportData(ZenToHan(m_userMessage.Content), playerData.PlayerID);

            if (reportData == null)
            {
                return;
            }

            var userReportedData = DatabaseReportDataController.GetReportData(playerData);

            if (userReportedData.Count() >= 3)
            {
                Task.Run(() => SendSystemMessage(m_userMessage.Channel, "報告件数が規定数を超えています。", 5));
                return;
            }

            if (DatabaseReportDataController.CreateReportData(reportData))
            {
                Task.Run(() => m_userMessage.AddReactionAsync(new Emoji(EnumMapper.I.GetString(ReactionType.Success))));
            }

            return;
        }

        /// <summary>
        /// 個人の最新の凸報告を削除する。
        /// </summary>
        private void DeleteReportData()
        {
            var splitContent = ZenToHan(m_userMessage.Content).Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var playerData = new PlayerData();
            if (splitContent.Length == 2
                && ulong.TryParse(Regex.Match(splitContent[1], @"\d+").Value, out ulong registerUserID))
            {
                playerData = DatabasePlayerDataController.LoadPlayerData(m_ClanRole, registerUserID);
            }
            else
            {
                playerData = DatabasePlayerDataController.LoadPlayerData(m_ClanRole, m_userMessage.Author.Id);
            }

            if (playerData == null)
            {
                return;
            }
            
            var recentReportData = DatabaseReportDataController.GetReportData(playerData)
                .OrderBy(x => x.DateTime).ToList();
            var removeData = recentReportData.Last();
            if (DatabaseReportDataController.DeleteReportData(removeData))
            {
                var taskList = new List<Task>();
                taskList.Add(m_userMessage.AddReactionAsync(new Emoji(EnumMapper.I.GetString(ReactionType.Success))));
                if (playerData.UserID != m_userMessage.Author.Id)
                {
                    taskList.Add(SendSystemMessage(
                        m_userMessage.Channel,
                        $"<@{playerData.UserID}>の凸報告を代理削除しました。\nこのメッセージは30秒後削除されます。",
                        30
                        ));
                }
                Task.Run(() => Task.WhenAll(taskList.ToArray())); 
            }
        }

        /// <summary>
        /// 代理報告用
        /// </summary>
        private void RegisterOtherUserReportData()
        {
            var splitContent = ZenToHan(m_userMessage.Content).Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (splitContent.Count() != 3)
            {
                return;
            }

            if (!ulong.TryParse(Regex.Match(splitContent[1], @"\d+").Value, out ulong registerUserID))
            {
                return;
            }

            var registerPlayerData = DatabasePlayerDataController.LoadPlayerData(m_ClanRole, registerUserID);

            if (registerPlayerData == null)
            {
                return;
            }

            var reportData = StringToReportData(splitContent[2], registerPlayerData.PlayerID);

            if (reportData == null)
            {
                return;
            }

            var userReportedData = DatabaseReportDataController.GetReportData(registerPlayerData);

            if (userReportedData.Count() >= 3)
            {
                Task.Run(() => SendSystemMessage(m_userMessage.Channel, "報告件数が規定数を超えています。", 5));
                return;
            }

            if (DatabaseReportDataController.CreateReportData(reportData))
            {
                Task.Run(() => m_userMessage.AddReactionAsync(new Emoji(EnumMapper.I.GetString(ReactionType.Success))));
            }

            return;

        }

        /// <summary>
        /// 凸報告データ送信用
        /// </summary>
        /// <returns></returns>
        private async Task SendClanAttackList()
        {
            var clanAttackEmbed = CreateClanReportData();
            await m_userMessage.Channel.SendMessageAsync(embed: clanAttackEmbed);
            return;
        }

        /// <summary>
        /// メッセージ情報から報告データに変換する。
        /// </summary>
        /// <param name="messageContent">メッセージ内容</param>
        /// <returns></returns>
        private ReportData StringToReportData(string messageContent, ulong playerID)
        {
            //var messageContent = ZenToHan(m_userMessage.Content);

            var userReportData = new ReportData()
            {
                PlayerID = 0,
                DeleteFlag = false,
            };

            if (Regex.IsMatch(messageContent, @"\d\D{1,3}"))
            {
                // TODO : 1-5をコンストで示す方法を調べる
                var bossNumber = int.Parse(Regex.Match(messageContent, @"\d").Value );
                var attackNumber = StringToAttackNumber(Regex.Match(messageContent, @"\D{1,3}").Value);

                // TODO:マジックナンバーを定数化
                if (attackNumber == 0 || attackNumber == 99)
                {
                    return null;
                }
                userReportData.PlayerID = 1;
                userReportData.AttackType = (byte)attackNumber;
                userReportData.BossNumber = (byte)bossNumber;
            }

            if (userReportData.BossNumber < Common.MinBossNumber || userReportData.BossNumber > Common.MaxBossNumber)
            {
                return null;
            }

            userReportData.PlayerID = playerID;

            return userReportData;
        }
        
        /// <summary>
        /// クランごとの凸報告データをEmbedで返す。
        /// </summary>
        /// <returns></returns>
        private Embed CreateClanReportData()
        {
            // TODO : 3を定数化
            var MaxReportNumber = 3;
            // TODO : 最大クラン人数 30人
            var MaxClanPlayer = 30;
            var embedBuilder = new EmbedBuilder();

            var clanPlayerDataList = DatabasePlayerDataController.LoadPlayerData(m_ClanData);
            var reportDataList = DatabaseReportDataController.GetReportData(m_ClanData);

            var playerInfoList = clanPlayerDataList.Select(x => new PlayerInfo(
                x.PlayerID, 
                x.GuildUserName, 
                reportDataList.Where(r => r.PlayerID == x.PlayerID).ToArray()
            ));
            
            
            for (int i = 0; i <= MaxReportNumber; i++)
            {
                var players = playerInfoList.Where(x => x.ReportData.Length == i);

                if (players.Count() > MaxClanPlayer)
                {
                    players = players.Take(MaxClanPlayer);
                }

                var reportStringBuilder = new StringBuilder();
                var reportMessage = string.Join("\n", players.Select(x => x.GetNameWithReport()).ToArray());

                var nameHeader = i == 0 ? "未" : i.ToString();
                var embedFieldBuilder = new EmbedFieldBuilder() {
                    Name = $"▼{nameHeader}凸の方({players.Count()}人)",
                    Value = reportMessage.Length == 0 ? "該当者なし" : reportMessage,
                    IsInline = true,
                };

                embedBuilder.AddField(embedFieldBuilder);
            }

            var nowAttackCount = reportDataList.Count();
            var memberAttackCount = clanPlayerDataList.Count() * MaxReportNumber;
            var MaxAllReportNumber = MaxClanPlayer * MaxReportNumber;
            var memberAttackString = memberAttackCount > MaxReportNumber ? $"{MaxAllReportNumber}+" : memberAttackCount.ToString();
            embedBuilder.Title = $"凸状況({nowAttackCount}凸/ {memberAttackString}凸)";

            embedBuilder.Footer = new EmbedFooterBuilder()
            {
                Text = $"最終更新時刻 : {DateTime.Now:T}",
            };

            return embedBuilder.Build();
        }

        /// <summary>
        /// メッセージ送信時に数秒後に削除する。
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="sendMessage"></param>
        /// <param name="deleteSeconds"></param>
        /// <returns></returns>
        private async Task SendSystemMessage(ISocketMessageChannel channel, string sendMessage, int deleteSeconds = 0)
        {
            var delayTime = new TimeSpan(0, 0, deleteSeconds);
            var messageData = await channel.SendMessageAsync(sendMessage);
            if (deleteSeconds == 0)
            {
                return;
            }
            await Task.Delay(delayTime);
            try
            {
                await messageData.DeleteAsync();
            }
            catch
            {
                
            }
        }
    }
}