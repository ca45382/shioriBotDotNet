using System.Threading.Tasks;
using Discord.WebSocket;
using System.Linq;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Extension;
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
            var reactionChannel = reaction.Channel as SocketGuildChannel;

            m_PlayerData = DatabasePlayerDataController
                .LoadPlayerData((ulong)reactionChannel?.Guild.Id, reaction.UserId);
            var userRole = reactionChannel?.Guild.GetRole((ulong)m_PlayerData?.ClanData.ClanRoleID);

            if (userRole == null)
            {
                return;
            }

            m_PlayerClanData = DatabaseClanDataController.LoadClanData(userRole);
        }

        public async Task RunReactionReceive()
        {
            if (m_ReceiveReaction != null)
            {
                await RunReactionReceive(m_ReceiveReaction); 
            }
        }

        public async Task RunReactionReceive(SocketReaction reaction)
        {
            if (m_PlayerClanData == null)
            {
                return;
            }

            var userClanData = m_PlayerClanData;
            var reactionChannelID = reaction.Channel.Id;
            var fetureID = userClanData.ChannelData.FirstOrDefault(x => x.ChannelID == reactionChannelID)?.FeatureID ?? 0;

            if (reactionChannelID == userClanData.ChannelData.GetChannelID(userClanData.ClanID, ChannelFeatureType.ReserveResultID))
            {
                await new BattleReservation(userClanData, reaction)
                    .RunReservationResultReaction();
            }

            BattleDeclaration battleDeclaration = fetureID switch
            {
                (int)ChannelFeatureType.DeclareBoss1ID => new BattleDeclaration(m_PlayerClanData, reaction, BossNumberType.Boss1Number),
                (int)ChannelFeatureType.DeclareBoss2ID => new BattleDeclaration(m_PlayerClanData, reaction, BossNumberType.Boss2Number),
                (int)ChannelFeatureType.DeclareBoss3ID => new BattleDeclaration(m_PlayerClanData, reaction, BossNumberType.Boss3Number),
                (int)ChannelFeatureType.DeclareBoss4ID => new BattleDeclaration(m_PlayerClanData, reaction, BossNumberType.Boss4Number),
                (int)ChannelFeatureType.DeclareBoss5ID => new BattleDeclaration(m_PlayerClanData, reaction, BossNumberType.Boss5Number),
                _ => null,
            };

            if (battleDeclaration != null)
            {
                await battleDeclaration.RunDeclarationCommandByReaction();
            }
        }
    }
}
