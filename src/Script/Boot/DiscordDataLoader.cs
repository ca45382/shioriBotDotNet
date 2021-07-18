using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using ShioriBot.Net.Model;
using ShioriBot.Net.Database;

namespace ShioriBot.Net.Script
{
    public class DiscordDataLoader
    {
        private readonly IEnumerable<ClanData> m_ServerClanData;
        private readonly SocketGuild m_Guild;

        private class ClanMember
        {
            public ulong SocketRoleID;
            public IEnumerable<IUser> userList;
        }

        public DiscordDataLoader(SocketGuild guild)
        {
            m_Guild = guild;

            try
            {
                m_ServerClanData = DatabaseClanDataController.LoadClanData(m_Guild);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void UpdateServerData()
        {
            var serverData = DatabaseServerDataController.LoadServerData(m_Guild);

            if (serverData == null)
            {
                DatabaseServerDataController.CreateServerData(m_Guild);
            }
            else
            {
                DatabaseServerDataController.UpdateServerData(m_Guild);
            }
        }

        public void UpdateClanData()
        {
            foreach (var clanData in m_ServerClanData)
            {
                var updateClanData = clanData;
                var clanRole = m_Guild.Roles
                    .FirstOrDefault(x => x.Id == clanData.ClanRoleID);

                if (clanRole == null)
                {
                    // TODO : ここにクランがなくなった際の処理を実装
                    continue;
                }

                if(string.Compare(clanData.ClanName, clanRole.Name) != 0)
                {
                    updateClanData.ClanName = clanRole.Name;
                    DatabaseClanDataController.UpdateClanData(updateClanData);
                }
            }
        }

        /// <summary>
        /// SQLサーバー側とDiscord側のプレイヤーデータ同期
        /// </summary>
        /// <param name="guild"></param>
        public void UpdatePlayerData()
        {

            var usersOnDiscord = GetServerClanMember();
            var usersOnDatabase = DatabasePlayerDataController.LoadPlayerData(m_Guild.Id);

            // テーブルにユーザを追加・更新
            var createUserData = new List<PlayerData>();
            var updateUserData = new List<PlayerData>();

            foreach (PlayerData discordUser in usersOnDiscord)
            {
                var sameNameFlag = false;
                var existFlag = false;

                foreach (PlayerData userDatabase in usersOnDatabase)
                {
                    if (IsSameUser(discordUser,userDatabase))
                    {
                        discordUser.PlayerID = userDatabase.PlayerID;
                        existFlag = true;

                        if (userDatabase.GuildUserName == discordUser.GuildUserName)
                        {
                            sameNameFlag = true;
                        }
                    }
                }

                if (!existFlag)
                {
                    createUserData.Add(discordUser);
                }
                else if (!sameNameFlag)
                {
                    updateUserData.Add(discordUser);
                }
            }

            DatabasePlayerDataController.CreatePlayerData(createUserData);
            DatabasePlayerDataController.UpdatePlayerData(updateUserData);

            var deleteUsers = usersOnDatabase
                .Where(mySQLUser => !usersOnDiscord.Any(discordUser => IsSameUser(mySQLUser, discordUser))
                );

            // テーブルからユーザを削除
            DatabasePlayerDataController.DeletePlayerData(deleteUsers);
        }

        /// <summary>
        /// サーバー内の登録クランに所属しているユーザー情報の入手
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        private IEnumerable<PlayerData> GetServerClanMember()
        {
            var roleIDList = m_ServerClanData.Select(x => x.ClanRoleID);
            var socketRoleList = m_Guild.Roles.Where(x => roleIDList.Any(y => y == x.Id));

            List<PlayerData> serverClanMember = new();

            foreach (var socketRole in socketRoleList)
            {
                var userList = socketRole.Members;
                var clanData = m_ServerClanData.FirstOrDefault(x => x.ClanRoleID == socketRole.Id);

                foreach (var socketUser in userList)
                {
                    if (socketUser.IsBot)
                    {
                        continue;
                    }

                    PlayerData playerData = new()
                    {
                        ClanID = clanData.ClanID,
                        UserID = socketUser.Id,
                        GuildUserName = socketUser.Nickname ?? socketUser.Username,
                    };

                    serverClanMember.Add(playerData);
                }
            }

            return serverClanMember;
        }

        private bool IsSameUser(PlayerData left, PlayerData right)
                => left.UserID == right.UserID && left.ClanID == right.ClanID;
    }
}