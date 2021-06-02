using Discord.WebSocket;
using PriconneBotConsoleApp.DataTypes;
using PriconneBotConsoleApp.MySQL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PriconneBotConsoleApp.Script
{
    class PlayerDataLoader
    {
        private readonly IEnumerable<ClanData> m_clanData;

        public PlayerDataLoader()
        {
            try
            {
                m_clanData = new MySQLClanDataController().LoadClanData();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void UpdateServerData(SocketGuild guild)
        {
            var serverDataController = new MySQLServerDataController();

            var serverData = serverDataController.LoadServerData(guild);

            if (serverData == null)
            {
                serverDataController.CreateServerData(guild);
            }
            else
            {
                serverDataController.UpdateServerData(guild);
            }
        }

        /// <summary>
        /// SQLサーバー側とDiscord側のプレイヤーデータ同期
        /// </summary>
        /// <param name="guild"></param>
        public void UpdatePlayerData(SocketGuild guild)
        {
            var playerDataControler = new MySQLPlayerDataController();

            // サーバー上のクランメンバー
            var usersOnDiscord = GetServerClanMember(guild);

            // テーブル上のプレイヤーデータ
            var usersOnSQLServer = new MySQLPlayerDataController().LoadPlayerData(guild.Id);

            #region テーブルにユーザを追加・更新
            var createUserData = new List<PlayerData>();
            var updateUserData = new List<PlayerData>();

            foreach (PlayerData discordUser in usersOnDiscord)
            {
                var sameNameFlag = false;
                var existFlag = false;

                foreach (PlayerData mySQLUser in usersOnSQLServer)
                {
                    if (mySQLUser.UserID == discordUser.UserID &&
                        mySQLUser.ClanData.ClanRoleID == discordUser.ClanData.ClanRoleID)
                    {
                        existFlag = true;

                        if (mySQLUser.GuildUserName == discordUser.GuildUserName)
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

            playerDataControler.CreatePlayerData(createUserData);
            playerDataControler.UpdatePlayerData(updateUserData);
            #endregion

            #region テーブルからユーザを削除
            static bool IsSameUser(PlayerData left, PlayerData right)
                => left.UserID == right.UserID && left.ClanData.ClanRoleID == right.ClanData.ClanRoleID;

            var deleteUsers = usersOnSQLServer
                .Where(
                    mySQLUser => !usersOnDiscord.Any(discordUser => IsSameUser(mySQLUser, discordUser))
                );

            playerDataControler.DeletePlayerData(deleteUsers);
            #endregion
        }

        /// <summary>
        /// サーバー内の登録クランに所属しているユーザー情報の入手
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        private IReadOnlyList<PlayerData> GetServerClanMember(SocketGuild guild)
        {
            var clanMember = new List<PlayerData>();

            // SQL側の情報から対象のクランロールを抽出
            var guildClanIDs = ClanIDsOnServer(guild.Id);

            // 現在のDiscord上の名前を抽出
            // 同一サーバーで複数クランに所属している場合は弾く
            foreach (SocketGuildUser user in guild.Users)
            {
                if (user.IsBot)
                {
                    continue;
                }

                var allUserRoleID = user.Roles
                    .Where(x => guildClanIDs.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToList();

                if (allUserRoleID.Count != 1)
                {
                    continue;
                }

                var roleID = allUserRoleID.FirstOrDefault();
                var nickName = user.Nickname ?? user.Username;

                var playerData = new PlayerData
                {
                    ClanData = new ClanData() 
                    {
                        ServerID = user.Guild.Id,
                        ClanRoleID = roleID
                    },

                    UserID = user.Id,
                    GuildUserName = nickName
                };

                clanMember.Add(playerData);
            }

            return clanMember;
        }

        /// <summary>
        /// サーバー内のクランIDの抽出
        /// </summary>
        /// <param name="guildID"></param>
        /// <returns></returns>
        private IReadOnlyList<ulong> ClanIDsOnServer(ulong guildID)
            => m_clanData
                .Where(x => x.ServerID == guildID)
                .Select(x => x.ClanRoleID)
                .ToList();
    }
}