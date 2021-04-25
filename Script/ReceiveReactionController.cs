using System.Threading.Tasks;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataTypes;
using PriconneBotConsoleApp.MySQL;

namespace PriconneBotConsoleApp.Script
{
    class ReceiveReactionController
    {

        private ClanData m_playerClanData;
        private PlayerData m_playerData;
        private SocketReaction m_reaction;

        public ReceiveReactionController(SocketReaction reaction)
        {
            m_reaction = reaction;
            var reactionChannel = reaction.Channel as SocketGuildChannel;

            m_playerData = new MySQLPlayerDataController()
                .LoadPlayerData(reactionChannel.Guild.Id.ToString(),
                    reaction.UserId.ToString());

            var userRole = reactionChannel.Guild
                .GetRole(ulong.Parse(m_playerData.ClanData.ClanRoleID));
            m_playerClanData = new MySQLClanDataController().LoadClanData(userRole);
        }

        async public Task RunReactionReceive()
        {
            if (m_reaction != null)
            {
                await RunReactionReceive(m_reaction); 
            }

            return;
        }

        async public Task RunReactionReceive(SocketReaction reaction)
        {
            var userClanData = m_playerClanData;
            var reactionChannelID = reaction.Channel.Id.ToString();
            
            if (reactionChannelID ==
                m_playerClanData.ChannelIDs.DeclarationChannelID)
            {
                await new BattleDeclaration(userClanData, reaction)
                    .RunDeclarationCommandByReaction();
            }
        }
    }
}
