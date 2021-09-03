using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.Model;

namespace PriconneBotConsoleApp.Database
{
    public static class DatabasePlayerDataController
    {
        public static List<PlayerData> LoadPlayerData(ulong serverID)
        {
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.PlayerData
                .Include(b => b.ClanData)
                .Where(b => b.ClanData.ServerID == serverID)
                .ToList();
        }

        public static IEnumerable<PlayerData> LoadPlayerData(SocketRole roleData)
        {
            using var databaseConnector = new ShioriDBContext();
            var clanData = databaseConnector.ClanData.AsQueryable()
                .Where(x => x.ClanRoleID == roleData.Id && x.ServerID == roleData.Guild.Id)
                .FirstOrDefault();

            if (clanData == null)
            {
                return null;
            }

            return LoadPlayerData(clanData);
        }

        public static IEnumerable<PlayerData> LoadPlayerData(ClanData clanData)
        {
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.PlayerData.AsQueryable()
                .Where(x => x.ClanID == clanData.ClanID)
                .ToList();
        }

        public static PlayerData LoadPlayerData(ulong serverID, ulong userID)
        {
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.PlayerData
                .Include(b => b.ClanData)
                .FirstOrDefault(b => b.UserID == userID && b.ClanData.ServerID == serverID);
        }

        public static PlayerData LoadPlayerData(SocketRole roleData, ulong userID)
        {
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.PlayerData
                .Include(x => x.ClanData)
                .FirstOrDefault(x => x.ClanData.ServerID == roleData.Guild.Id
                && x.ClanData.ClanRoleID == roleData.Id && x.UserID == userID);
        }

        public static void CreatePlayerData(IEnumerable<PlayerData> playersData)
        {
            using var databaseConnector = new ShioriDBContext();
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

        public static void UpdatePlayerData(IEnumerable<PlayerData> playersData)
        {
            using var databaseConnector = new ShioriDBContext();
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

        public static void DeletePlayerData(IEnumerable<PlayerData> playersData)
        {
            using var databaseConnector = new ShioriDBContext();
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
