using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriconneBotConsoleApp.Database
{
    class DatabaseTaskKillController
    {
        public IEnumerable<TaskKillData> LoadTaskKillData(ClanData clanData)
        {
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.TaskKillData
                .Include(x => x.PlayerData)
                .Where(x => x.PlayerData.ClanID == clanData.ClanID && !x.DeleteFlag)
                .ToList();
        }

        public TaskKillData LoadTaskKillData(PlayerData playerData)
        {
            using var databaseConnector = new DatabaseConnector();

            if (playerData.PlayerID == 0)
            {
                playerData = databaseConnector.PlayerData
                    .FirstOrDefault(x => x.ClanID == playerData.ClanID && x.UserID == playerData.UserID);
            }

            if (playerData == null)
            {
                return null;
            }

            return databaseConnector.TaskKillData
                .FirstOrDefault(x => x.PlayerID == playerData.PlayerID && !x.DeleteFlag);
        }

        public bool CreateTaskKillData(PlayerData playerData)
        {
            using var databaseConnector = new DatabaseConnector();

            if (playerData.PlayerID == 0)
            {
                playerData = databaseConnector.PlayerData
                    .FirstOrDefault(x => x.ClanID == playerData.ClanID && x.UserID == playerData.UserID);
            }

            if (playerData == null)
            {
                return false;
            }

            var databaseData = LoadTaskKillData(playerData);

            if(databaseData != null)
            {
                return false;
            }

            var transaction = databaseConnector.Database.BeginTransaction();

            try
            {
                databaseConnector.TaskKillData.Add(new TaskKillData()
                {
                    PlayerID = playerData.PlayerID,
                    DeleteFlag = false,
                });
                databaseConnector.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public bool DeleteTaskKillData(PlayerData playerData)
        {
            using var databaseConnector = new DatabaseConnector();

            if (playerData.PlayerID == 0)
            {
                playerData = databaseConnector.PlayerData
                    .FirstOrDefault(x => x.ClanID == playerData.ClanID && x.UserID == playerData.UserID);
            }

            if (playerData == null)
            {
                return false;
            }

            var databaseData = LoadTaskKillData(playerData);

            if (databaseData == null)
            {
                return false;
            }

            var transaction = databaseConnector.Database.BeginTransaction();

            try
            {
                databaseData.DeleteFlag = true;
                databaseConnector.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }

        }
    }
}
