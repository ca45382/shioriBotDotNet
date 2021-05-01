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
            
            if (m_playerData == null)
            {
                return;
            }

            var userRole = messageChannel.Guild.GetRole(ulong.Parse(m_playerData.ClanData.ClanRoleID));

            if (userRole == null)
            {
                return;
            }

            m_playerClanData = new MySQLClanDataController().LoadClanData(userRole);
        }

        public async Task RunMessageReceive()
        {
            if (m_message != null)
            {
                await RunMessageReceive(m_message);
            }
        }

        public async Task RunMessageReceive(SocketUserMessage message)
        {

            await new TimeLineConversion(message).RunByMessage() ;

            if (m_playerData == null || m_playerClanData == null)
            {
                return;
            }

            var userClanData = m_playerClanData;
            var messageChannelID = message.Channel.Id.ToString();
            if (messageChannelID ==
                m_playerClanData.ChannelIDs.ReservationChannelID)
            {
                await new BattleReservation(userClanData, message).RunReservationCommand();
            }
            if (messageChannelID ==
                m_playerClanData.ChannelIDs.ReservationResultChannelID)
            {
                await new BattleReservation(userClanData, message).RunReservationResultCommand();
            }
            if (messageChannelID ==
                m_playerClanData.ChannelIDs.DeclarationChannelID)
            {
                await new BattleDeclaration(userClanData, message).RunDeclarationCommandByMessage();
            }

            
        }

    }
}