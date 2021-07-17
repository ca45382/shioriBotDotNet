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
            await new TimeLineConversion(message).RunByMessage();
            await new PriconneEventViewer(message).SendEventInfomationByMessage();

            if (m_PlayerData == null || m_PlayerClanData == null)
            {
                return;
            }

            var messageChannelID = message.Channel.Id;
            var fetureID = m_PlayerClanData.ChannelData.Where(x => x.ChannelID == messageChannelID).FirstOrDefault()?.FeatureID ?? 0;

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

            if (messageChannelID == m_PlayerClanData.ChannelData.GetChannelID(m_PlayerClanData.ClanID, ChannelFeatureType.TaskKillID))
            {
                await new BattleTaskKill(m_PlayerClanData, message).RunByMessageCommands();
            }

            if (messageChannelID == m_PlayerClanData.ChannelData.GetChannelID(m_PlayerClanData.ClanID, ChannelFeatureType.ReportID))
            {
                await new BattleReport(m_PlayerClanData, message).RunByMessage();
            }

            BattleDeclaration battleDeclaration;
            switch (fetureID)
            {
                case (int)ChannelFeatureType.DeclareBoss1ID:
                    battleDeclaration = new BattleDeclaration(m_PlayerClanData, message, BossNumberType.Boss1Number);
                    break;
                case (int)ChannelFeatureType.DeclareBoss2ID:
                    battleDeclaration = new BattleDeclaration(m_PlayerClanData, message, BossNumberType.Boss2Number);
                    break;
                case (int)ChannelFeatureType.DeclareBoss3ID:
                    battleDeclaration = new BattleDeclaration(m_PlayerClanData, message, BossNumberType.Boss3Number);
                    break;
                case (int)ChannelFeatureType.DeclareBoss4ID:
                    battleDeclaration = new BattleDeclaration(m_PlayerClanData, message, BossNumberType.Boss4Number);
                    break;
                case (int)ChannelFeatureType.DeclareBoss5ID:
                    battleDeclaration = new BattleDeclaration(m_PlayerClanData, message, BossNumberType.Boss5Number);
                    break;
                default:
                    battleDeclaration = null;
                    break;
            };

            if (battleDeclaration != null)
            {
                await battleDeclaration.RunDeclarationCommandByMessage();
            }
        }
    }
}
