using Discord.WebSocket;
using PriconneBotConsoleApp.Database;
using PriconneBotConsoleApp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriconneBotConsoleApp.Script
{
    public class BattleTaskKill
    {
        private readonly ClanData m_UserClanData;
        private readonly SocketUserMessage m_UserMessage;
        private readonly SocketRole m_UserRole;

        public BattleTaskKill(ClanData clanData, SocketUserMessage userMessage)
        {
            if (clanData.RoleData == null || clanData.ChannelData == null || clanData.MessageData == null)
            {
                clanData = new DatabaseClanDataController().LoadClanData(m_UserRole);
            } 
            m_UserClanData = clanData;
            m_UserMessage = userMessage;
            m_UserRole = (userMessage.Channel as SocketTextChannel)?.Guild.GetRole(clanData.ClanRoleID);
        }

        /// <summary>
        /// メッセージからコマンドの実行
        /// </summary>
        /// <returns></returns>
        public async Task RunByMessageCommands()
        {
            if (m_UserMessage.Content.StartsWith("!"))
            {
                // 予約の取り消し
                if (m_UserMessage.Content.StartsWith("!rm"))
                {

                }
            }
        }

        private bool RegisterTaskKill()
        {

        }
    }
}
