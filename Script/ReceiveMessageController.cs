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

            m_playerData = new MySQLPlayerDataController().LoadPlayerData(guildID, userID);

            var userRole = messageChannel.Guild.GetRole(ulong.Parse(m_playerData.ClanData.ClanRoleID));

            m_playerClanData = new MySQLClanDataController().LoadClanData(userRole);
        }

        async public Task RunMessageReceive()
        {
            if (m_message != null)
            {
                await RunMessageReceive(m_message);
            }

            return;
            
        }

        async public Task RunMessageReceive(SocketUserMessage message)
        {
            if (message.Channel.Id.ToString() == 
                m_playerClanData.ChannelIDs.ReservationChannelID)
            {
                await new BattleReservation(m_playerClanData, message).RunReservationCommand();
            }

            return;
        }

    }
}