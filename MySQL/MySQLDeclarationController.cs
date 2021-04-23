using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using PriconneBotConsoleApp.DataTypes;
using Microsoft.EntityFrameworkCore;

namespace PriconneBotConsoleApp.MySQL
{
    class MySQLDeclarationController
    {

        public bool UpdateDeclarationMessageID(ClanData clanData, string messageID)
        {
            using (var mySQLConnector = new MySQLConnector())
            {
                var transaction = mySQLConnector.Database.BeginTransaction();

                var clanID = mySQLConnector.ClanData
                    .Include(d => d.BotDatabase)
                    .Where(d => d.ServerID == clanData.ServerID)
                    .Where(d => d.ClanRoleID == clanData.ClanRoleID)
                    .Select(d => d.ClanID)
                    .FirstOrDefault();

                if (clanID == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                var updateData = mySQLConnector.MessageIDs
                   .Include(d => d.ClanData)
                   .Where(d => d.ClanID == clanID)
                   .FirstOrDefault();

                if (updateData == null)
                {
                    transaction.Rollback();
                    return false;
                }

                updateData.DeclarationMessageID = messageID;
                mySQLConnector.SaveChanges();
                transaction.Commit();

            }
            return true;
        }

        public IEnumerable<DeclarationData> LoadDeclarationData(ClanData clanData)
        {
            IEnumerable<DeclarationData> declarationDataSet = null;

            using (var mySQLConnector = new MySQLConnector())
            {
                var clanID = mySQLConnector.ClanData
                    .Include(d => d.BotDatabase)
                    .Where(d => d.ServerID == clanData.ServerID)
                    .Where(d => d.ClanRoleID == clanData.ClanRoleID)
                    .Select(d => d.ClanID)
                    .FirstOrDefault();

                if (clanID == 0)
                {
                    return null;
                }

                declarationDataSet = mySQLConnector.DeclarationData
               .Include(b => b.PlayerData)
               .ThenInclude(d => d.ClanData)
               .Where(d => d.PlayerData.ClanID == clanID)
               .Where(b => b.DeleteFlag == false)
               .Where(b => b.BattleLap == clanData.BattleLap)
               .Where(b => b.BossNumber == clanData.BossNumber)
               .ToList();
            }
            return declarationDataSet;

        }

        public bool CreateDeclarationData(DeclarationData declarationData)
        {
            var userData = declarationData.PlayerData;
            if (declarationData.PlayerID == 0 && userData == null) return false;

            using (var mySQLConnector = new MySQLConnector())
            {
                var transaction = mySQLConnector.Database.BeginTransaction();

                var playerID = mySQLConnector.PlayerData
                .Include(d => d.ClanData)
                .Where(d => d.ClanData.ServerID == userData.ClanData.ServerID)
                .Where(d => d.ClanData.ClanRoleID == userData.ClanData.ClanRoleID)
                .Where(d => d.UserID == userData.UserID)
                .Select(d => d.PlayerID)
                .FirstOrDefault();

                declarationData.PlayerID = playerID;
                try
                {
                    mySQLConnector.Add(declarationData);
                    mySQLConnector.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }

            }

            return true;
        }

        public bool UpdateDeclarationData(DeclarationData declarationData)
        {
            var userData = declarationData.PlayerData;
            if (declarationData.PlayerID == 0 && userData == null) return false;

            using (var mySQLConnector = new MySQLConnector())
            {
                var transaction = mySQLConnector.Database.BeginTransaction();

                var playerID = mySQLConnector.PlayerData
                .Include(d => d.ClanData)
                .Where(d => d.ClanData.ServerID == userData.ClanData.ServerID)
                .Where(d => d.ClanData.ClanRoleID == userData.ClanData.ClanRoleID)
                .Where(d => d.UserID == userData.UserID)
                .Select(d => d.PlayerID)
                .FirstOrDefault();

                if (playerID == 0) return false;

                var updateData = mySQLConnector.DeclarationData
                    .Include(d => d.PlayerData)
                    .Where(d => d.PlayerID == playerID)
                    .Where(d => d.BattleLap == declarationData.BattleLap)
                    .Where(d => d.BossNumber == declarationData.BossNumber)
                    .Where(b => b.DeleteFlag == false)
                    .FirstOrDefault();

                updateData.FinishFlag = declarationData.FinishFlag;
                mySQLConnector.SaveChanges();
                transaction.Commit();

            }

            return true;
        }

        public bool DeleteDeclarationData(IEnumerable<DeclarationData> declarationDataSet)
        {
            using (var mySQLConnector = new MySQLConnector())
            {
                var transaction = mySQLConnector.Database.BeginTransaction();

                foreach (var declarationData in declarationDataSet)
                {
                    var userDeleteDataSet = mySQLConnector.DeclarationData
                        .Include(d => d.PlayerData)
                        .Where(d => d.PlayerID == declarationData.PlayerID)
                        .Where(b => b.DeleteFlag == false)
                        .Where(d => d.BossNumber == declarationData.BossNumber)
                        .Where(d => d.BattleLap == declarationData.BattleLap)
                        .ToList();

                    if (userDeleteDataSet.Count() == 0) continue;
                    foreach (var updateData in userDeleteDataSet)
                    {
                        updateData.DeleteFlag = true;
                    }
                }

                mySQLConnector.SaveChanges();
                transaction.Commit();
            }

            return true;
        }
    }
}
