using System.Threading.Tasks;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.Database;
using System.Linq;
using PriconneBotConsoleApp.DataType;

namespace PriconneBotConsoleApp.Script
{
    class ReceiveReactionController
    {
        private readonly ClanData m_playerClanData;
        private readonly PlayerData m_playerData;
        private readonly SocketReaction m_reaction;

        public ReceiveReactionController(SocketReaction reaction)
        {
            m_reaction = reaction;
            var reactionChannel = reaction.Channel as SocketGuildChannel;

            m_playerData = new DatabasePlayerDataController()
                .LoadPlayerData((ulong)reactionChannel?.Guild.Id, reaction.UserId);
            var userRole = reactionChannel?.Guild.GetRole((ulong)m_playerData?.ClanData.ClanRoleID);

            if (userRole == null)
            {
                return;
            }

            m_playerClanData = new DatabaseClanDataController().LoadClanData(userRole);
        }

        public async Task RunReactionReceive()
        {
            if (m_reaction != null)
            {
                await RunReactionReceive(m_reaction); 
            }
        }

        public async Task RunReactionReceive(SocketReaction reaction)
        {
            if (m_playerClanData == null)
            {
                return;
            }

            var userClanData = m_playerClanData;
            var reactionChannelID = reaction.Channel.Id;

            if (reactionChannelID == userClanData.ChannelData
                .FirstOrDefault(x => x.FeatureID == (uint)ChannelFeatureType.DeclareID)
                .ChannelID)
            {
                await new BattleDeclaration(userClanData, reaction)
                    .RunDeclarationCommandByReaction();
            } 
            else if (reactionChannelID == userClanData.ChannelData
                .FirstOrDefault(x => x.FeatureID == (uint)ChannelFeatureType.ReserveResultID)
                .ChannelID)
            {
                await new BattleReservation(userClanData, reaction)
                    .RunReservationResultReaction();
            }


        }
    }
}
