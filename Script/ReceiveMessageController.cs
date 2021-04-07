using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataTypes;
using PriconneBotConsoleApp.MySQL;

namespace PriconneBotConsoleApp.Script
{
    public class ReceiveMessageController
    {

        private ClanData m_playerClanData;
        private PlayerData m_playerData;
        private SocketUserMessage m_message;

        public ReceiveMessageController(SocketUserMessage message)
        {
            m_message = message;
            var messageChannel = message.Channel as SocketGuildChannel;
            

            var guildID = messageChannel.Guild.Id.ToString();
            var userID = message.Author.Id.ToString();

            using (var mySQLPlayerData = new MySQLPlayerDataController())
            {
                m_playerData = mySQLPlayerData.LoadPlayerData(guildID, userID);

                var userRole = messageChannel.Guild.GetRole(ulong.Parse(m_playerData.ClanRoleID));
                m_playerClanData = mySQLPlayerData.LoadClanInfo(userRole);
            }
        }

        public void RunMessageReceive()
        {
            if (m_message != null)
            {
                RunMessageReceive(m_message);
            }
            
        }

        public void RunMessageReceive(SocketUserMessage message)
        {
            if (message.Channel.Id.ToString() == 
                m_playerClanData.ChannelIDs.ReservationChannelID)
            {
                var battleReservation = new BattleReservation(m_playerClanData);
                battleReservation.RunReservationCommand(message);
            }
        }

    }
}