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

            if (reactionChannelID == userClanData.ChannelData.GetChannelID(userClanData.ClanID,ChannelFeatureType.DeclareID))
            {
                await new BattleDeclaration(userClanData, reaction)
                    .RunDeclarationCommandByReaction();
            } 
            else if (reactionChannelID == userClanData.ChannelData.GetChannelID(userClanData.ClanID, ChannelFeatureType.ReserveResultID))
            {
                await new BattleReservation(userClanData, reaction)
                    .RunReservationResultReaction();
            }


        }
    }
}
