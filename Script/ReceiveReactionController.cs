using Discord.WebSocket;
using PriconneBotConsoleApp.DataTypes;
using PriconneBotConsoleApp.MySQL;
using System.Threading.Tasks;

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

            m_playerData = new MySQLPlayerDataController()
                .LoadPlayerData((ulong)reactionChannel?.Guild.Id, reaction.UserId);

            if (m_playerData == null)
            {
                return;
            }

            var userRole = reactionChannel?.Guild.GetRole((ulong)m_playerData?.ClanData.ClanRoleID);

            if (userRole == null)
            {
                return;
            }

            m_playerClanData = new MySQLClanDataController().LoadClanData(userRole);
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
            
            if (reactionChannelID == userClanData.ChannelIDs.DeclarationChannelID)
            {
                await new BattleDeclaration(userClanData, reaction)
                    .RunDeclarationCommandByReaction();
            }
        }
    }
}
