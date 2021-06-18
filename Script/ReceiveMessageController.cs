using System.Threading.Tasks;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.Database;
using System.Linq;
using PriconneBotConsoleApp.DataType;

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
            await new PriconneEventViewer(message).SendEventInfomationByMessage();

            if (m_playerData == null || m_playerClanData == null)
            {
                return;
            }

            var messageChannelID = message.Channel.Id;

            if (messageChannelID == m_playerClanData.ChannelData
                .FirstOrDefault(x => x.FeatureID == (uint)ChannelFeatureType.ReserveID)
                .ChannelID)
            {
                await new BattleReservation(m_playerClanData, message).RunReservationCommand();
            }

            if (messageChannelID == m_playerClanData.ChannelData
                .FirstOrDefault(x => x.FeatureID == (uint)ChannelFeatureType.ReserveResultID)
                .ChannelID)
            {
                await new BattleReservation(m_playerClanData, message).RunReservationResultCommand();
            }

            if (messageChannelID == m_playerClanData.ChannelData
                .FirstOrDefault(x => x.FeatureID == (uint)ChannelFeatureType.DeclareID)
                .ChannelID)
            {
                await new BattleDeclaration(m_playerClanData, message).RunDeclarationCommandByMessage();
            }
        }
    }
}
