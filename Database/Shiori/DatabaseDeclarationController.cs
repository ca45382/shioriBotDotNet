using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataModel;

namespace PriconneBotConsoleApp.Database
{
    class DatabaseDeclarationController
    {
        public bool UpdateDeclarationMessageID(ClanData clanData, ulong messageID)
        {
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            var clanID = databaseConnector.ClanData
                .Include(d => d.ServerData)
                .Where(d => d.ServerID == clanData.ServerID && d.ClanRoleID == clanData.ClanRoleID)
                .Select(d => d.ClanID)
                .FirstOrDefault();

            if (clanID == 0)
            {
                transaction.Rollback();
                return false;
            }

            var updateData = databaseConnector.MessageIDs
                .Include(d => d.ClanData)
                .Where(d => d.ClanID == clanID)
                .FirstOrDefault();

            if (updateData == null)
            {
                transaction.Rollback();
                return false;
            }

            updateData.DeclarationMessageID = messageID;
            databaseConnector.SaveChanges();
            transaction.Commit();
            return true;
        }

        public IEnumerable<DeclarationData> LoadDeclarationData(ClanData clanData)
        {
            var databaseConnector = new DatabaseConnector();

            var clanID = databaseConnector.ClanData
                .Include(d => d.ServerData)
                .Where(d => d.ServerID == clanData.ServerID && d.ClanRoleID == clanData.ClanRoleID)
                .Select(d => d.ClanID)
                .FirstOrDefault();

            if (clanID == 0)
            {
                return null;
            }

            return databaseConnector.DeclarationData
                .Include(b => b.PlayerData)
                .ThenInclude(d => d.ClanData)
                .Where(d => d.PlayerData.ClanID == clanID && !d.DeleteFlag
                    && d.BattleLap == clanData.BattleLap && d.BossNumber == clanData.BossNumber
                )
                .ToList();
        }

        public IEnumerable<DeclarationData> LoadDeclarationData(PlayerData playerData)
        {
            using var databaseConnector = new DatabaseConnector();

            playerData = databaseConnector.PlayerData
                .Include(d => d.ClanData)
                .Where(d => d.PlayerID == playerData.PlayerID)
                .FirstOrDefault();

            return databaseConnector.DeclarationData
                .Include(b => b.PlayerData)
                .ThenInclude(d => d.ClanData)
                .Where(d => d.PlayerData.PlayerID == playerData.PlayerID && !d.DeleteFlag 
                    && d.BattleLap == playerData.ClanData.BattleLap && d.BossNumber == playerData.ClanData.BossNumber
                )
                .ToList();
        }

        public bool CreateDeclarationData(DeclarationData declarationData)
        {
            var userData = declarationData.PlayerData;
            
            if (declarationData.PlayerID == 0 && userData == null)
            {
                return false;
            }

            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            if (declarationData.PlayerID == 0)
            {
                declarationData.PlayerID = PlayerDataToPlayerID(userData);
                    
                if (declarationData.PlayerID == 0)
                {
                    return false;
                }
            }

            try
            {
                databaseConnector.Add(declarationData);
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

        public bool UpdateDeclarationData(DeclarationData declarationData)
        {
            var userData = declarationData.PlayerData;

            if (declarationData.PlayerID == 0 && userData == null)
            {
                return false;
            }

            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            if (declarationData.PlayerID == 0)
            {
                declarationData.PlayerID = PlayerDataToPlayerID(userData);

                if (declarationData.PlayerID == 0)
                {
                    return false;
                }
            }

            var updateData = databaseConnector.DeclarationData
                .Include(d => d.PlayerData)
                .Where(d =>
                   d.PlayerID == declarationData.PlayerID && d.BattleLap == declarationData.BattleLap
                   && d.BossNumber == declarationData.BossNumber && !d.FinishFlag && !d.DeleteFlag
                )
                .FirstOrDefault();

            if (updateData != null)
            {
                updateData.FinishFlag = declarationData.FinishFlag;
            }

            databaseConnector.SaveChanges();
            transaction.Commit();

            return true;
        }

        public bool DeleteDeclarationData(DeclarationData declarationData)
            => DeleteDeclarationData(new[] { declarationData });

        /// <summary>
        /// 宣言データに削除フラグを立てます。
        /// </summary>
        /// <param name="declarationDataSet"></param>
        /// <returns></returns>
        public bool DeleteDeclarationData(IEnumerable<DeclarationData> declarationDataSet)
        {
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            foreach (var declarationData in declarationDataSet)
            {
                if (declarationData.PlayerID == 0)
                {
                    declarationData.PlayerID = PlayerDataToPlayerID(declarationData.PlayerData);

                    if (declarationData.PlayerID == 0)
                    {
                        return false;
                    }
                }

                var userDeleteDataSet = databaseConnector.DeclarationData
                    .Include(d => d.PlayerData)
                    .Where(d => d.PlayerID == declarationData.PlayerID && !d.DeleteFlag
                        && d.BossNumber == declarationData.BossNumber && d.BattleLap == declarationData.BattleLap
                        && d.FinishFlag == declarationData.FinishFlag
                    );

                foreach (var updateData in userDeleteDataSet)
                {
                    updateData.DeleteFlag = true;
                }
            }

            databaseConnector.SaveChanges();
            transaction.Commit();

            return true;
        }

        /// <summary>
        /// PLayerDataクラスからPlayerIDを返します。
        /// ClanDataのクラスもPlayerDataの中に必要です。
        /// </summary>
        /// <param name="playerData"></param>
        /// <returns></returns>
        private ulong PlayerDataToPlayerID(PlayerData playerData)
        {
            if (playerData == null || playerData.ClanData == null)
            {
                return 0;
            }

            var clanData = playerData.ClanData;
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.PlayerData
                .Include(d => d.ClanData)
                .ThenInclude(e => e.ServerData)
                .Where(d => d.ClanData.ServerID == clanData.ServerID && d.ClanData.ClanRoleID == clanData.ClanRoleID
                    && d.UserID == playerData.UserID 
                )
                .Select(d => d.PlayerID)
                .FirstOrDefault();
        }
    }
}
