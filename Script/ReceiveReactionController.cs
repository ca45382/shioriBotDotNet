using System.Threading.Tasks;
using Discord.WebSocket;
using System.Linq;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.Database;

namespace PriconneBotConsoleApp.Script
{
    public class ReceiveReactionController
    {
        private readonly ClanData m_PlayerClanData;
        private readonly PlayerData m_PlayerData;
        private readonly SocketReaction m_ReceiveReaction;

        public ReceiveReactionController(SocketReaction reaction)
        {
            m_ReceiveReaction = reaction;
            var reactionChannel = m_ReceiveReaction.Channel as SocketGuildChannel;
            m_PlayerData = DatabasePlayerDataController
                .LoadPlayerData(reactionChannel?.Guild.Id ?? 0, m_ReceiveReaction.UserId);
            var userRole = reactionChannel?.Guild.GetRole(m_PlayerData?.ClanData.ClanRoleID ?? 0);

            if (userRole == null)
            {
                return;
            }

            m_PlayerClanData = DatabaseClanDataController.LoadClanData(userRole);
        }

        public async Task RunReactionReceive()
        {
            if (m_PlayerClanData == null)
            {
                return;
            }

            var reactionChannelID = m_ReceiveReaction.Channel.Id;
            var channelFeatureID = m_PlayerClanData.ChannelData
                .FirstOrDefault(x => x.ChannelID == reactionChannelID)?.FeatureID ?? 0;

            if (channelFeatureID == (uint)ChannelFeatureType.ReserveResultID)
            {
                await new BattleReservation(m_PlayerClanData, m_ReceiveReaction)
                    .RunReservationResultReaction();
            }

            BattleDeclaration battleDeclaration = channelFeatureID switch
            {
                (int)ChannelFeatureType.DeclareBoss1ID => new BattleDeclaration(m_PlayerClanData, m_ReceiveReaction, BossNumberType.Boss1Number),
                (int)ChannelFeatureType.DeclareBoss2ID => new BattleDeclaration(m_PlayerClanData, m_ReceiveReaction, BossNumberType.Boss2Number),
                (int)ChannelFeatureType.DeclareBoss3ID => new BattleDeclaration(m_PlayerClanData, m_ReceiveReaction, BossNumberType.Boss3Number),
                (int)ChannelFeatureType.DeclareBoss4ID => new BattleDeclaration(m_PlayerClanData, m_ReceiveReaction, BossNumberType.Boss4Number),
                (int)ChannelFeatureType.DeclareBoss5ID => new BattleDeclaration(m_PlayerClanData, m_ReceiveReaction, BossNumberType.Boss5Number),
                _ => null,
            };

            if (battleDeclaration != null)
            {
                await battleDeclaration.RunDeclarationCommandByReaction();
            }
        }
    }
}
