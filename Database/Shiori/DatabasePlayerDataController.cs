using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataTypes;
using System.Collections.Generic;
using System.Linq;

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
                .ThenInclude(b => b.ServerData)
                .Where(b => b.UserID == userID && b.ClanData.ServerID == serverID)
                .FirstOrDefault();
        }

        public void CreatePlayerData(IEnumerable<PlayerData> playersData)
        {
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();
            
            foreach (PlayerData playerData in playersData)
            {
                var clanID = databaseConnector.ClanData
                    .Include(d => d.ServerData)
                    .Where(d => d.ServerID == playerData.ClanData.ServerID && d.ClanRoleID == playerData.ClanData.ClanRoleID)
                    .Select(d => d.ClanID)
                    .FirstOrDefault();

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
                    .Where(d => d.ClanData.ServerID == playerData.ClanData.ServerID)
                    .Where(d => d.ClanData.ClanRoleID == playerData.ClanData.ClanRoleID)
                    .Where(d => d.UserID == playerData.UserID)
                    .FirstOrDefault();

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
                    .Where(d => d.ClanData == playerData.ClanData)
                    .Where(d => d.UserID == playerData.UserID)
                    .FirstOrDefault();

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
