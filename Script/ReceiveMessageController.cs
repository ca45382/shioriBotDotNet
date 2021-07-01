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
        private readonly ClanData m_PlayerClanData;
        private readonly PlayerData m_PlayerData;
        private readonly SocketUserMessage m_ReceiveMessage;

        public ReceiveMessageController(SocketUserMessage message)
        {
            m_ReceiveMessage = message;
            var messageChannel = message.Channel as SocketGuildChannel;
            var guildID = messageChannel.Guild.Id;
            var userID = message.Author.Id;
            m_PlayerData = DatabasePlayerDataController.LoadPlayerData(guildID, userID);
            
            if (m_PlayerData == null)
            {
                return;
            }

            var userRole = messageChannel.Guild.GetRole(m_PlayerData.ClanData.ClanRoleID);

            if (userRole == null)
            {
                return;
            }

            m_PlayerClanData = DatabaseClanDataController.LoadClanData(userRole);
        }

        public async Task RunMessageReceive()
        {
            if (m_ReceiveMessage != null)
            {
                await RunMessageReceive(m_ReceiveMessage);
            }
        }

        public async Task RunMessageReceive(SocketUserMessage message)
        {
            await new TimeLineConversion(message).RunByMessage() ;
            await new PriconneEventViewer(message).SendEventInfomationByMessage();

            if (m_PlayerData == null || m_PlayerClanData == null)
            {
                return;
            }

            var messageChannelID = message.Channel.Id;

            if (messageChannelID == m_PlayerClanData.ChannelData.GetChannelID(m_PlayerClanData.ClanID, ChannelFeatureType.ReserveID))
            {
                await new BattleReservation(m_PlayerClanData, message).RunReservationCommand();
            }

            if (messageChannelID == m_PlayerClanData.ChannelData.GetChannelID(m_PlayerClanData.ClanID, ChannelFeatureType.ReserveResultID))
            {
                await new BattleReservation(m_PlayerClanData, message).RunReservationResultCommand();
            }

            if (messageChannelID == m_PlayerClanData.ChannelData.GetChannelID(m_PlayerClanData.ClanID, ChannelFeatureType.DeclareID))
            {
                await new BattleDeclaration(m_PlayerClanData, message).RunDeclarationCommandByMessage();
            }

            if(messageChannelID == m_PlayerClanData.ChannelData.GetChannelID(m_PlayerClanData.ClanID, ChannelFeatureType.TaskKillID))
            {
                await new BattleTaskKill(m_PlayerClanData, message).RunByMessageCommands();
            }

            if (messageChannelID == m_playerClanData.ChannelData.GetChannelID(m_playerClanData.ClanID, ChannelFeatureType.ReportID))
            {
                new BattleReport(m_playerClanData, message).RunByMessage();
            }
        }
    }
}
