using System;
using System.Collections.Generic;
using System.Linq;
using PriconneBotConsoleApp.DataTypes;
using PriconneBotConsoleApp.MySQL;
using Discord.WebSocket;

namespace PriconneBotConsoleApp.Script
{
    class PlayerDataLoader
    {
        private List<ClanData> m_clanData;

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
        /// SQLサーバー側とDiscord側の
        /// プレイヤーデータ同期
        /// </summary>
        /// <param name="guild"></param>
        public void UpdatePlayerData(SocketGuild guild)
        {
            var createUserData = new List<PlayerData>();
            var deleteUserData = new List<PlayerData>();
            var updateUserData = new List<PlayerData>();

            //サーバー上のクランメンバー情報の取得
            var usersOnDiscord = GetServerClanMember(guild);

            // SQL上のプレイヤーデータを読み取る
            var usersOnSQLServer = new MySQLPlayerDataController().LoadPlayerData(guild.Id.ToString());

            //SQLに追加・更新する情報の抽出
            foreach (PlayerData discordUser in usersOnDiscord)
            {
                var sameNameFlag = 0;
                var existFlag = 0;

                foreach (PlayerData mySQLUser in usersOnSQLServer)
                {
                    if (mySQLUser.UserID == discordUser.UserID &&
                        mySQLUser.ClanData.ClanRoleID == discordUser.ClanData.ClanRoleID)
                    {
                        existFlag = 1;
                        if (mySQLUser.GuildUserName == discordUser.GuildUserName)
                        {
                            sameNameFlag = 1;
                        }
                    }
                }

                if (existFlag == 0)
                {
                    createUserData.Add(discordUser);
                    continue;
                }
                if (sameNameFlag == 0)
                {
                    updateUserData.Add(discordUser);
                }
            }

            //SQLから削除するデータの抽出
            foreach (PlayerData mySQLUser in usersOnSQLServer)
            {
                var existFlag = false;
                foreach (PlayerData discordUser in usersOnDiscord)
                {
                    if (mySQLUser.UserID == discordUser.UserID &&
                        mySQLUser.ClanData.ClanRoleID == discordUser.ClanData.ClanRoleID)
                    {
                        existFlag = true;
                    }
                }
                if (!existFlag)
                {
                    deleteUserData.Add(mySQLUser);
                }
            }

            var playerDataControl = new MySQLPlayerDataController();
            playerDataControl.CreatePlayerData(createUserData);
            playerDataControl.UpdatePlayerData(updateUserData);
            playerDataControl.DeletePlayerData(deleteUserData);
        }

        /// <summary>
        /// サーバー内の登録クランに所属しているユーザー情報の入手
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        private List<PlayerData> GetServerClanMember(SocketGuild guild)
        {
            var clanMember = new List<PlayerData>();

            // SQL側の情報から対象のクランロールを抽出
            var guildClanIDs = ClanIDInServer(guild.Id.ToString());

            //現在のDiscord上の名前を抽出
            //同一サーバーで複数クランに所属している場合は弾く
            foreach (SocketGuildUser user in guild.Users)
            {
                if (user.IsBot) { continue; }

                var allUserRoleID = user.Roles
                    .Where(x => guildClanIDs.Contains(x.Id.ToString()))
                    .Select(x => x.Id.ToString());

                var roleCount = allUserRoleID.Count();

                if (roleCount != 1) { continue; }

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
        private IEnumerable<string> ClanIDInServer(string guildID)
        {
            var guildClanIDs = m_clanData
                .Where(x => x.ServerID == guildID)
                .Select(x => x.ClanRoleID);

            return guildClanIDs;
        }
    }
}