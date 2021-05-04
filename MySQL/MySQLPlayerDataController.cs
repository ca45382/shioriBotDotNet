using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataTypes;
using System.Collections.Generic;
using System.Linq;

namespace PriconneBotConsoleApp.MySQL
{
    class MySQLPlayerDataController
    {
        public List<PlayerData> LoadPlayerData(ulong serverID)
        {
            using var mySQLConnector = new MySQLConnector();

            return mySQLConnector.PlayerData
                .Include(b => b.ClanData)
                .Where(b => b.ClanData.ServerID == serverID)
                .ToList();
        }

        public PlayerData LoadPlayerData(ulong serverID, ulong userID)
        {
            using var mySQLConnector = new MySQLConnector();

            return mySQLConnector.PlayerData
                .Include(b => b.ClanData)
                .ThenInclude(b => b.BotDatabase)
                .Where(b => b.UserID == userID && b.ClanData.ServerID == serverID)
                .FirstOrDefault();
        }

        public void CreatePlayerData(IEnumerable<PlayerData> playersData)
        {
            using var mySQLConnector = new MySQLConnector();
            var transaction = mySQLConnector.Database.BeginTransaction();
            
            foreach (PlayerData playerData in playersData)
            {
                var clanID = mySQLConnector.ClanData
                    .Include(d => d.BotDatabase)
                    .Where(d => d.ServerID == playerData.ClanData.ServerID && d.ClanRoleID == playerData.ClanData.ClanRoleID)
                    .Select(d => d.ClanID)
                    .FirstOrDefault();

                var newPlayerData = new PlayerData
                {
                    ClanID = clanID,
                    UserID = playerData.UserID,
                    GuildUserName = playerData.GuildUserName
                };

                mySQLConnector.PlayerData.Add(newPlayerData);
            }

            mySQLConnector.SaveChanges();
            transaction.Commit();
        }

        public void UpdatePlayerData(IEnumerable<PlayerData> playersData)
        {
            using var mySQLConnector = new MySQLConnector();
            var transaction = mySQLConnector.Database.BeginTransaction();
            
            foreach (PlayerData playerData in playersData)
            {
                var updateData = mySQLConnector.PlayerData
                    .Include(d => d.ClanData)
                    .Where(d => d.ClanData.ServerID == playerData.ClanData.ServerID)
                    .Where(d => d.ClanData.ClanRoleID == playerData.ClanData.ClanRoleID)
                    .Where(d => d.UserID == playerData.UserID)
                    .FirstOrDefault();

                if (updateData != null)
                {
                    updateData.GuildUserName = playerData.GuildUserName;
                }
            }

            mySQLConnector.SaveChanges();
            transaction.Commit();
        }

        public void DeletePlayerData(IEnumerable<PlayerData> playersData)
        {
            using var mySQLConnector = new MySQLConnector();
            var transaction = mySQLConnector.Database.BeginTransaction();
            
            foreach (PlayerData playerData in playersData)
            {
                var removeData = mySQLConnector.PlayerData
                    .Include(d => d.ClanData)
                    .Where(d => d.ClanData == playerData.ClanData)
                    .Where(d => d.UserID == playerData.UserID)
                    .FirstOrDefault();

                if (removeData != null)
                {
                    mySQLConnector.PlayerData.Remove(removeData);
                }
            }

            mySQLConnector.SaveChanges();
            transaction.Commit();
        }
    }
}
