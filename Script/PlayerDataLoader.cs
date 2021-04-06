using System;
using System.Collections.Generic;
using System.Text;

using MySql.Data.MySqlClient;
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
            //object data;
            try 
            {
                using (var playerDataSQLController = new MySQLPlayerDataController())
                {
                    m_clanData = playerDataSQLController.LoadClanInfo();
                    //data = playerDataSQLController.LoadPlayerData();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return;
        }

        /// <summary>
        /// SQLサーバー側とDiscord側の
        /// </summary>
        /// <param name="guild"></param>
        public void UpdatePlayerData(SocketGuild guild)
        {
            var guildAllUsers = guild.Users;
            var guildClanID = new List<string>() ;
            var usersOnDiscord = new List<PlayerData>();
            var usersOnSQLServer = new List<PlayerData>();
            var createUserData = new List<PlayerData>();
            var deleteUserData = new List<PlayerData>();
            var updateUserData = new List<PlayerData>();

            // SQL側の情報から対象のクランロールを抽出
            foreach (ClanData clanInfo in m_clanData)
            {
                if(clanInfo.ServerID == guild.Id.ToString())
                {
                    guildClanID.Add(clanInfo.ClanRoleID);
                }
            }

            //現在のDiscord上の名前を抽出
            //同一サーバーで複数クランに所属している場合は弾く
            foreach (SocketGuildUser user in guildAllUsers)
            {
                if(user.IsBot) { continue; }

                var userRoles = user.Roles;
                var roleCount = 0;
                string nickName;
                string roleID = null;
                foreach (SocketRole userRole in userRoles)
                {
                    if (guildClanID.Contains(userRole.Id.ToString()))
                    {
                        roleID = userRole.Id.ToString();
                        roleCount += 1;
                    }
                }

                if (roleCount != 1) { continue; }

                if (user.Nickname == null)
                {
                    nickName = user.Username;
                }
                else
                {
                    nickName = user.Nickname;
                }

                usersOnDiscord.Add(new PlayerData()
                {
                    ServerID = user.Guild.Id.ToString(),
                    ClanRoleID = roleID,
                    UserID = user.Id.ToString(),
                    GuildUserName = nickName
                });

                //Console.WriteLine(user.ToString() + roleCount.ToString());
            }
            
            // SQL上のプレイヤーデータを読み取る
            using (var playerDataSQLController = new MySQLPlayerDataController())
            {
                usersOnSQLServer = 
                    playerDataSQLController.LoadPlayerData(guild.Id.ToString());
            }

            //SQLに追加・更新する情報の抽出
            foreach (PlayerData discordUser in usersOnDiscord)
            {
                var sameNameFlag = 0;
                var existFlag = 0;
                foreach (PlayerData mySQLUser in usersOnSQLServer)
                {
                    if (mySQLUser.UserID == discordUser.UserID &&
                        mySQLUser.ClanRoleID == discordUser.ClanRoleID)
                    {
                        existFlag = 1;
                        if(mySQLUser.GuildUserName == discordUser.GuildUserName)
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
                var existFlag = 0;
                foreach(PlayerData discordUser in usersOnDiscord)
                {
                    if (mySQLUser.UserID == discordUser.UserID &&
                        mySQLUser.ClanRoleID == discordUser.ClanRoleID)
                    {
                        existFlag = 1;
                    }
                }
                if (existFlag == 0)
                {
                    deleteUserData.Add(mySQLUser);
                }
            }

            //SQL上のデータの更新
            using (var playerDataSQLController = new MySQLPlayerDataController())
            {
                playerDataSQLController.CreatePlayerData(createUserData);
                playerDataSQLController.UpdatePlayerData(updateUserData);
                playerDataSQLController.DeletePlayerData(deleteUserData);
            }
            return;
        }
    }
}
