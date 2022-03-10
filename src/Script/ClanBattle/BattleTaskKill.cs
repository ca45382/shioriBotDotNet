using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using ShioriBot.Database;
using ShioriBot.Model;
using ShioriBot.DataType;
using ShioriBot.Extension;

namespace ShioriBot.Script
{
    public class BattleTaskKill
    {
        private readonly ClanData m_UserClanData;
        private readonly SocketUserMessage m_UserMessage;
        private readonly SocketRole m_UserRole;
        private readonly SocketRole m_TaskKillRole;
        private readonly SocketGuild m_Guild;

        public BattleTaskKill(ClanData clanData, SocketUserMessage userMessage)
        {
            if (clanData.RoleData == null || clanData.ChannelData == null || clanData.MessageData == null)
            {
                clanData = DatabaseClanDataController.LoadClanData(m_UserRole);
            }
            m_UserClanData = clanData;
            m_UserMessage = userMessage;
            m_Guild = (userMessage.Channel as SocketTextChannel)?.Guild;
            m_UserRole = m_Guild?.GetRole(clanData.ClanRoleID);
            m_TaskKillRole = m_Guild?.GetRole(clanData.RoleData.GetRoleID(clanData.ClanID, RoleFeatureType.TaskKillRoleID));
        }

        public BattleTaskKill(SocketRole role)
        {
            m_UserRole = role;
            m_UserClanData = DatabaseClanDataController.LoadClanData(m_UserRole);
            m_Guild = m_UserRole.Guild;
            m_TaskKillRole = m_Guild?.GetRole(m_UserClanData.RoleData.GetRoleID(m_UserClanData.ClanID, RoleFeatureType.TaskKillRoleID));
        }

        public async Task SyncTaskKillData()
        {
            if (m_TaskKillRole == null)
            {
                return;
            }

            await SyncTaskKillRole();
        }
        /// <summary>
        /// メッセージからコマンドの実行
        /// </summary>
        /// <returns></returns>
        public async Task RunByMessageCommands()
        {
            if (m_TaskKillRole == null)
            {
                return;
            }
            var result = false;
            var taskList = new List<Task>();
            if (m_UserMessage.Content.StartsWith("!"))
            {
                // 予約の取り消し
                if (m_UserMessage.Content.StartsWith("!rm"))
                {
                    result = DeleteTaskKillData();

                    if (result)
                    {
                        taskList.Add(SuccessAddEmoji());
                    }

                }
                else if(m_UserMessage.Content.StartsWith("!init"))
                {
                    result = DeleteClanData();

                    if (result)
                    {
                        taskList.Add(SuccessAddEmoji());
                    }
                }

            }
            else
            {
                result = RegisterTaskKillData();

                if (result)
                {
                    taskList.Add(SuccessAddEmoji());
                }

            }

            taskList.Add(SyncTaskKillRole());
            await Task.WhenAll(taskList);
        }

        private bool RegisterTaskKillData()
        {
            var playerData = DatabasePlayerDataController.LoadPlayerData(m_UserRole, m_UserMessage.Author.Id);

            if (playerData == null)
            {
                return false;
            }

            return DatabaseTaskKillController.CreateTaskKillData(playerData);
        }

        private bool DeleteTaskKillData()
        {
            var playerData = DatabasePlayerDataController.LoadPlayerData(m_UserRole, m_UserMessage.Author.Id);

            if (playerData == null)
            {
                return false;
            }

            var taskKillData = DatabaseTaskKillController.LoadTaskKillData(playerData);

            return DatabaseTaskKillController.DeleteTaskKillData(taskKillData);
        }

        private bool DeleteClanData()
        {
            return DatabaseTaskKillController.DeleteTaskKillData(m_UserClanData);
        }

        private async Task SyncTaskKillRole()
        {
            var taskKilledUserIDs = DatabaseTaskKillController.LoadTaskKillData(m_UserClanData).Select(x => x.PlayerData.UserID);
            var roledUserIDs = m_TaskKillRole.Members.Select(x => x.Id);

            var assignUserIDs = taskKilledUserIDs.Except(roledUserIDs);
            var withdrowUserIDs = roledUserIDs.Except(taskKilledUserIDs);

            var updateTask = new List<Task>();

            foreach (var assignUserID in assignUserIDs)
            {
                updateTask.Add(m_Guild.GetUser(assignUserID).AddRoleAsync(m_TaskKillRole));
            }
            foreach (var withdrowUserID in withdrowUserIDs)
            {
                updateTask.Add(m_Guild.GetUser(withdrowUserID).RemoveRoleAsync(m_TaskKillRole));
            }

            await Task.WhenAll(updateTask);
        }

        private async Task SuccessAddEmoji()
            => await m_UserMessage.AddReactionAsync(ReactionType.Success.ToEmoji());
    }
}
