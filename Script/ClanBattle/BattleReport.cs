using Discord.WebSocket;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.DataModel;
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


        public BattleReport(ClanData clanData, SocketUserMessage userMessage)
        {
            m_ClanData = clanData;
            m_userMessage = userMessage;
            m_Guild = (userMessage.Channel as SocketTextChannel)?.Guild;
            m_ClanRole = m_Guild?.GetRole(clanData.ClanRoleID);
        }

        public void RunByMessage()
        {
            RegisterReportData();
        }

        private void RegisterReportData()
        {
            var reportData = UserRegisterToReportData();
            if (reportData == null)
            {
                return;
            }
            var userReportedData = DatabaseReportDataController.GetReportData(reportData.PlayerData);

            if (userReportedData.Count() >= 3)
            {
                Task.Run(() => SendSystemMessage(m_userMessage.Channel, "報告件数が規定数を超えています。", 5));
                return;
            }

            if (DatabaseReportDataController.CreateReportData(reportData))
            {
                Task.Run(() => SendSystemMessage(m_userMessage.Channel, "成功"));
            }

            return;
        }

        private ReportData UserRegisterToReportData()
        {
            var messageContent = ZenToHan(m_userMessage.Content);

            var userReportData = new ReportData()
            {
                PlayerID = 0,
                DeleteFlag = false,
            };

            if (Regex.IsMatch(messageContent, @"\d\D{1,3}"))
            {
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

            if (userReportData.BossNumber == 0 || userReportData.AttackType == 0)
            {
                return null;
            }

            var playerData = DatabasePlayerDataController.LoadPlayerData(m_ClanRole, m_userMessage.Author.Id);

            if (playerData == null)
            {
                return null;
            }
            userReportData.PlayerData = playerData;
            userReportData.PlayerID = playerData.PlayerID;

            return userReportData;
        }

        
        /// <summary>
        /// メッセージ送信時に数秒後に削除する。
        /// 
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