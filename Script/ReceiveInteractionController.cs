using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.Database;

namespace PriconneBotConsoleApp.Script
{
    public class ReceiveInteractionController
    {
        private readonly SocketInteraction m_Interaction;
        private readonly SocketTextChannel m_TextChannel;
        private readonly PlayerData m_PlayerData;
        private readonly ClanData m_ClanData;
        private readonly SocketRole m_Role;

        public ReceiveInteractionController(SocketInteraction interaction)
        {
            m_Interaction = interaction;
            m_TextChannel = interaction.Channel as SocketTextChannel;
            m_PlayerData = DatabasePlayerDataController.LoadPlayerData(m_TextChannel?.Guild.Id ?? 0, interaction.User.Id);

            if (m_PlayerData == null)
            {
                return;
            }

            m_Role = m_TextChannel.Guild.GetRole(m_PlayerData.ClanData.ClanRoleID);
            m_ClanData = DatabaseClanDataController.LoadClanData(m_Role);
        }

        public async Task Run()
        {
            if (m_ClanData == null)
            {
                return;
            }

            var channelFeatureID = m_ClanData.ChannelData
                .FirstOrDefault(x => x.ChannelID == m_TextChannel.Id)
                ?.FeatureID ?? 0;

            if (channelFeatureID == (int)ChannelFeatureType.ReserveResultID)
            {
                await new BattleReservationSummary(m_Role, m_ClanData).RunInteraction(m_Interaction);
            }

            BattleDeclaration battleDeclaration = channelFeatureID switch
            {
                (int)ChannelFeatureType.DeclareBoss1ID => new BattleDeclaration(m_ClanData, m_Interaction, BossNumberType.Boss1Number),
                (int)ChannelFeatureType.DeclareBoss2ID => new BattleDeclaration(m_ClanData, m_Interaction, BossNumberType.Boss2Number),
                (int)ChannelFeatureType.DeclareBoss3ID => new BattleDeclaration(m_ClanData, m_Interaction, BossNumberType.Boss3Number),
                (int)ChannelFeatureType.DeclareBoss4ID => new BattleDeclaration(m_ClanData, m_Interaction, BossNumberType.Boss4Number),
                (int)ChannelFeatureType.DeclareBoss5ID => new BattleDeclaration(m_ClanData, m_Interaction, BossNumberType.Boss5Number),
                _ => null,
            };

            if (battleDeclaration != null)
            {
                await battleDeclaration.RunByInteraction();
            }
        }
    }
}
