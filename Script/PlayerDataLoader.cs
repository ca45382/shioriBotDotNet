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
                var clanData = new MySQLClanDataController().LoadClanData();
                m_clanData = clanData;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// SQLサーバー側とDiscord側のプレイヤーデータ同期
        /// </summary>
        /// <param name="guild"></param>
        public void UpdatePlayerData(SocketGuild guild)
        {
            var createUserData = new List<PlayerData>();
            var updateUserData = new List<PlayerData>();

            // サーバー上のクランメンバー情報の取得
            var usersOnDiscord = GetServerClanMember(guild);

            // SQL上のプレイヤーデータを読み取る
            var usersOnSQLServer = new MySQLPlayerDataController().LoadPlayerData(guild.Id.ToString());

            // SQLに追加・更新する情報の抽出
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

            // SQLから削除するデータの抽出
            var deleteUsers =
                usersOnSQLServer.Where(
                    mySQLUser => usersOnDiscord.Any(
                        discordUser => mySQLUser.UserID == discordUser.UserID &&
                            mySQLUser.ClanData.ClanRoleID == discordUser.ClanData.ClanRoleID
                    )
                );

            var playerDataControl = new MySQLPlayerDataController();
            playerDataControl.CreatePlayerData(createUserData);
            playerDataControl.UpdatePlayerData(updateUserData);
            playerDataControl.DeletePlayerData(deleteUsers);
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
            var guildClanIDs = ClanIDsOnServer(guild.Id.ToString());

            // 現在のDiscord上の名前を抽出
            // 同一サーバーで複数クランに所属している場合は弾く
            foreach (SocketGuildUser user in guild.Users)
            {
                if (user.IsBot)
                {
                    continue;
                }

                var allUserRoleID = user.Roles
                    .Where(x => guildClanIDs.Contains(x.Id.ToString()))
                    .Select(x => x.Id.ToString())
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
                        ServerID = user.Guild.Id.ToString(),
                        ClanRoleID = roleID
                    },

                    UserID = user.Id.ToString(),
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
        private IReadOnlyList<string> ClanIDsOnServer(string guildID)
            => m_clanData
                .Where(x => x.ServerID == guildID)
                .Select(x => x.ClanRoleID)
                .ToList();
    }
}