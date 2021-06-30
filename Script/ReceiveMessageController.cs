using System.Threading.Tasks;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.Database;
using System.Linq;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Extension;

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
            m_playerData = DatabasePlayerDataController.LoadPlayerData(guildID, userID);
            
            if (m_playerData == null)
            {
                return;
            }

            var userRole = messageChannel.Guild.GetRole(m_playerData.ClanData.ClanRoleID);

            if (userRole == null)
            {
                return;
            }

            m_playerClanData = DatabaseClanDataController.LoadClanData(userRole);
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

            if (messageChannelID == m_playerClanData.ChannelData.GetChannelID(m_playerClanData.ClanID, ChannelFeatureType.ReserveID))
            {
                await new BattleReservation(m_playerClanData, message).RunReservationCommand();
            }

            if (messageChannelID == m_playerClanData.ChannelData.GetChannelID(m_playerClanData.ClanID, ChannelFeatureType.ReserveResultID))
            {
                await new BattleReservation(m_playerClanData, message).RunReservationResultCommand();
            }

            if (messageChannelID == m_playerClanData.ChannelData.GetChannelID(m_playerClanData.ClanID, ChannelFeatureType.DeclareID))
            {
                await new BattleDeclaration(m_playerClanData, message).RunDeclarationCommandByMessage();
            }

            if(messageChannelID == m_playerClanData.ChannelData.GetChannelID(m_playerClanData.ClanID, ChannelFeatureType.TaskKillID))
            {
                await new BattleTaskKill(m_playerClanData, message).RunByMessageCommands();
            }
        }
    }
}
