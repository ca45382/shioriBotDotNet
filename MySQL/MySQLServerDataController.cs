﻿using System.Collections.Generic;
using System.Linq;
using Discord;
using PriconneBotConsoleApp.DataTypes;

namespace PriconneBotConsoleApp.MySQL
{
    class MySQLServerDataController
    {
        public IEnumerable<ServerData> LoadServerData()
        {
            using var mySQLConnector = new MySQLConnector();

            return mySQLConnector.ServerData
                .ToArray();
        }

        public ServerData LoadServerData(IGuild guild)
        {
            using var mySQLConnector = new MySQLConnector();

            return mySQLConnector.ServerData
                .AsEnumerable()
                .Where(b => b.ServerID == guild.Id)
                .FirstOrDefault();
        }

        public void CreateServerData(IGuild guild)
        {
            using var mySQLConnector = new MySQLConnector();
            var transaction = mySQLConnector.Database.BeginTransaction();

            var mySQLServerData = mySQLConnector.ServerData
                .AsEnumerable()
                .Where(b => b.ServerID == guild.Id)
                .FirstOrDefault();

            if (mySQLServerData is not null)
            {
                return;
            }

            var newServerData = new ServerData
            {
                ServerID = guild.Id,
                ServerName = guild.Name,
                ServerOwnerID = guild.OwnerId,
            };

            mySQLConnector.ServerData.Add(newServerData);

            mySQLConnector.SaveChanges();
            transaction.Commit();

        }

        public void UpdateServerData(IGuild guild)
        {
            using var mySQLConnector = new MySQLConnector();
            var transaction = mySQLConnector.Database.BeginTransaction();

            var mySQLServerData = mySQLConnector.ServerData
                .AsEnumerable()
                .Where(b => b.ServerID == guild.Id)
                .FirstOrDefault();

            if (mySQLServerData is null)
            {
                return;
            }

            mySQLServerData.ServerName = guild.Name;
            mySQLServerData.ServerOwnerID = guild.OwnerId;

            mySQLConnector.SaveChanges();
            transaction.Commit();
        }
    }
}