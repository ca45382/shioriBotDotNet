using Discord.WebSocket;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PriconneBotConsoleApp.DataType;

namespace PriconneBotConsoleApp.Script
{
    public class RecieveInteractionController
    {
        private readonly ClanData m_ClanData;
        private readonly PlayerData m_PlayerData;
        private readonly SocketTextChannel m_Channel;
        private readonly SocketInteraction m_SocketInteraction;

        public RecieveInteractionController(SocketInteraction socketInteraction)
        {
            m_SocketInteraction = socketInteraction;
            m_Channel = socketInteraction.Channel as SocketTextChannel;
            m_PlayerData = DatabasePlayerDataController.LoadPlayerData(m_Channel.Guild.Id, socketInteraction.User.Id);
            var userRole = m_Channel.Guild.GetRole(m_PlayerData.ClanData.ClanRoleID);
            m_ClanData = DatabaseClanDataController.LoadClanData(userRole);
        }

        public async Task RunSocketInteraction()
        {
            if (m_ClanData == null)
            {
                return;
            }

            var channelFeatureID = m_ClanData.ChannelData.FirstOrDefault(x => x.ChannelID == m_Channel.Id)?.FeatureID ?? 0;

            if (channelFeatureID == 0)
            {
                return;
            }

            BattleDeclaration battleDeclaration = channelFeatureID switch
            {
                (int)ChannelFeatureType.DeclareBoss1ID => new BattleDeclaration(m_ClanData, m_SocketInteraction, BossNumberType.Boss1Number),
                (int)ChannelFeatureType.DeclareBoss2ID => new BattleDeclaration(m_ClanData, m_SocketInteraction, BossNumberType.Boss2Number),
                (int)ChannelFeatureType.DeclareBoss3ID => new BattleDeclaration(m_ClanData, m_SocketInteraction, BossNumberType.Boss3Number),
                (int)ChannelFeatureType.DeclareBoss4ID => new BattleDeclaration(m_ClanData, m_SocketInteraction, BossNumberType.Boss4Number),
                (int)ChannelFeatureType.DeclareBoss5ID => new BattleDeclaration(m_ClanData, m_SocketInteraction, BossNumberType.Boss5Number),
                _ => null,
            };

            if (battleDeclaration != null)
            {
                await battleDeclaration.RunCommandByInteraction();
            }
        }
    }
}
