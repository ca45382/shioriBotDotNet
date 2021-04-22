using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using PriconneBotConsoleApp.DataTypes;

namespace PriconneBotConsoleApp.MySQL
{
    class MySQLPlayerDataController
    {
        public List<PlayerData> LoadPlayerData(string serverID)
        {
            
            var playerData = new List<PlayerData>();

            using (var mySQLConnector = new MySQLConnector())
            {
                playerData = mySQLConnector.PlayerData
                    .Include(b => b.ClanData)
                    .Where(b => b.ClanData.ServerID == serverID)
                    .ToList();
            }

            return playerData;

        }

        public PlayerData LoadPlayerData(string serverID, string userID)
        {
            var playerData = new PlayerData();

            using (var mySQLConnector = new MySQLConnector())
            {
                playerData = mySQLConnector.PlayerData
                    .Include(b => b.ClanData)
                    .Where(b => b.UserID == userID)
                    .Where(b => b.ClanData.ServerID == serverID)
                    .FirstOrDefault();

            }

            return playerData;
        }

        public void CreatePlayerData(IEnumerable<PlayerData> playersData)
        {

            using (var mySQLConnector = new MySQLConnector())
            {
                var transaction = mySQLConnector.Database.BeginTransaction();
                foreach (PlayerData playerData in playersData)
                {
                    var clanID = mySQLConnector.ClanData
                        .Include(d => d.BotDatabase)
                        .Where(d => d.ServerID == playerData.ClanData.ServerID)
                        .Where(d => d.ClanRoleID == playerData.ClanData.ClanRoleID)
                        .Select(d => d.ClanID)
                        .FirstOrDefault();
                    mySQLConnector.PlayerData.Add(
                        new PlayerData()
                        {
                            ClanID = clanID,
                            UserID = playerData.UserID,
                            GuildUserName = playerData.GuildUserName
                        }
                        );
                }
                mySQLConnector.SaveChanges();
                transaction.Commit();
            }

            return;
        }

        public void UpdatePlayerData(IEnumerable<PlayerData> playersData)
        {
            using (var mySQLConnector = new MySQLConnector())
            {
                var transaction = mySQLConnector.Database.BeginTransaction();
                foreach (PlayerData playerData in playersData)
                {
                    var updateData = mySQLConnector.PlayerData
                        .Include(d => d.ClanData)
                        .Where(d => d.ClanData.ServerID == playerData.ClanData.ServerID)
                        .Where(d => d.ClanData.ClanRoleID == playerData.ClanData.ClanRoleID)
                        .Where(d => d.UserID == playerData.UserID)
                        .FirstOrDefault();
                    updateData.GuildUserName = playerData.GuildUserName;
                }
                mySQLConnector.SaveChanges();
                transaction.Commit();
            }

        }

        public void DeletePlayerData(IEnumerable<PlayerData> playersData)
        {
            using (var mySQLConnector = new MySQLConnector())
            {
                var transaction = mySQLConnector.Database.BeginTransaction();
                foreach (PlayerData playerData in playersData)
                {
                    var removeData = mySQLConnector.PlayerData
                        .Include(d => d.ClanData)
                        .Where(d => d.ClanData == playerData.ClanData)
                        .Where(d => d.UserID == playerData.UserID)
                        .FirstOrDefault();
                    mySQLConnector.PlayerData.Remove(removeData);
                }
                mySQLConnector.SaveChanges();
                transaction.Commit();
            }
        }
    }
}
