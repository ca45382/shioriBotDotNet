using Discord.WebSocket;
using PriconneBotConsoleApp.DataTypes;
using PriconneBotConsoleApp.Database;
using System.Threading.Tasks;

namespace PriconneBotConsoleApp.Script
{
    public class ReceiveMessageController
    {
        private readonly ClanData m_playerClanData;
        private readonly PlayerData m_playerData;
        private readonly SocketUserMessage m_message;

        public ReceiveMessageController(SocketUserMessage message)
        {
            m_message = message;
            var messageChannel = message.Channel as SocketGuildChannel;
            var guildID = messageChannel.Guild.Id;
            var userID = message.Author.Id;
            m_playerData = new DatabasePlayerDataController().LoadPlayerData(guildID, userID);
            
            if (m_playerData == null)
            {
                return;
            }

            var userRole = messageChannel.Guild.GetRole(m_playerData.ClanData.ClanRoleID);

            if (userRole == null)
            {
                return;
            }

            m_playerClanData = new DatabaseClanDataController().LoadClanData(userRole);
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

            var messageChannelID = message.Channel.Id;

            if (messageChannelID == m_playerClanData.ChannelIDs.ReservationChannelID)
            {
                await new BattleReservation(m_playerClanData, message).RunReservationCommand();
            }

            if (messageChannelID == m_playerClanData.ChannelIDs.ReservationResultChannelID)
            {
                await new BattleReservation(m_playerClanData, message).RunReservationResultCommand();
            }

            if (messageChannelID == m_playerClanData.ChannelIDs.DeclarationChannelID)
            {
                await new BattleDeclaration(m_playerClanData, message).RunDeclarationCommandByMessage();
            }
        }
    }
}
