using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataModel;

namespace PriconneBotConsoleApp.Database
{
    class DatabasePlayerDataController
    {
        public List<PlayerData> LoadPlayerData(ulong serverID)
        {
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.PlayerData
                .Include(b => b.ClanData)
                .Where(b => b.ClanData.ServerID == serverID)
                .ToList();
        }

        public PlayerData LoadPlayerData(ulong serverID, ulong userID)
        {
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.PlayerData
                .Include(b => b.ClanData)
                .FirstOrDefault(b => b.UserID == userID && b.ClanData.ServerID == serverID);
        }

        public void CreatePlayerData(IEnumerable<PlayerData> playersData)
        {
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();
            
            foreach (PlayerData playerData in playersData)
            {
                var clanID = databaseConnector.ClanData
                    .FirstOrDefault(d => d.ServerID == playerData.ClanData.ServerID && d.ClanRoleID == playerData.ClanData.ClanRoleID)
                    ?.ClanID ?? 0;

                if (clanID == 0)
                {
                    transaction.Rollback();
                    return;
                }

                var newPlayerData = new PlayerData
                {
                    ClanID = clanID,
                    UserID = playerData.UserID,
                    GuildUserName = playerData.GuildUserName
                };

                databaseConnector.PlayerData.Add(newPlayerData);
            }

            databaseConnector.SaveChanges();
            transaction.Commit();
        }

        public void UpdatePlayerData(IEnumerable<PlayerData> playersData)
        {
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();
            
            foreach (PlayerData playerData in playersData)
            {
                var updateData = databaseConnector.PlayerData
                    .Include(d => d.ClanData)
                    .FirstOrDefault(d => d.ClanData.ServerID == playerData.ClanData.ServerID 
                    && d.ClanData.ClanRoleID == playerData.ClanData.ClanRoleID && d.UserID == playerData.UserID);

                if (updateData != null)
                {
                    updateData.GuildUserName = playerData.GuildUserName;
                }
            }

            databaseConnector.SaveChanges();
            transaction.Commit();
        }

        public void DeletePlayerData(IEnumerable<PlayerData> playersData)
        {
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();
            
            foreach (PlayerData playerData in playersData)
            {
                var removeData = databaseConnector.PlayerData
                    .Include(d => d.ClanData)
                    .FirstOrDefault(d => d.ClanData == playerData.ClanData && d.UserID == playerData.UserID);

                if (removeData != null)
                {
                    databaseConnector.PlayerData.Remove(removeData);
                }
            }

            databaseConnector.SaveChanges();
            transaction.Commit();
        }
    }
}
