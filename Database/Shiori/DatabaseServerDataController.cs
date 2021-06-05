using System.Collections.Generic;
using System.Linq;
using Discord;
using PriconneBotConsoleApp.DataModel;

namespace PriconneBotConsoleApp.Database
{
    class DatabaseServerDataController
    {
        public IEnumerable<ServerData> LoadServerData()
        {
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.ServerData.ToArray();
        }

        public ServerData LoadServerData(IGuild guild)
        {
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.ServerData
                .FirstOrDefault(b => b.ServerID == guild.Id);
        }

        public void CreateServerData(IGuild guild)
        {
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            var databaseServerData = databaseConnector.ServerData
                .FirstOrDefault(b => b.ServerID == guild.Id);

            if (databaseServerData != null)
            {
                return;
            }

            var newServerData = new ServerData
            {
                ServerID = guild.Id,
                ServerName = guild.Name,
                ServerOwnerID = guild.OwnerId,
            };

            databaseConnector.ServerData.Add(newServerData);
            databaseConnector.SaveChanges();
            transaction.Commit();

        }

        public void UpdateServerData(IGuild guild)
        {
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            var databaseServerData = databaseConnector.ServerData
                .FirstOrDefault(b => b.ServerID == guild.Id);

            if (databaseServerData == null)
            {
                return;
            }

            databaseServerData.ServerName = guild.Name;
            databaseServerData.ServerOwnerID = guild.OwnerId;

            databaseConnector.SaveChanges();
            transaction.Commit();
        }
    }
}
