using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.Model;
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.Database
{
    public static class DatabaseDeclarationController
    {
        public static IEnumerable<DeclarationData> LoadDeclarationData(ClanData clanData, byte bossNumber)
        {
            using var databaseConnector = new ShioriDBContext();

            var databaseClanData = databaseConnector.ClanData.AsQueryable()
                .Where(d => d.ServerID == clanData.ServerID && d.ClanRoleID == clanData.ClanRoleID)
                .FirstOrDefault();

            if (databaseClanData == null)
            {
                return null;
            }

            return databaseConnector.DeclarationData
                .Include(b => b.PlayerData)
                .Where(d => d.PlayerData.ClanID == databaseClanData.ClanID && !d.DeleteFlag
                    && d.BattleLap == clanData.GetBossLap(bossNumber) && d.BossNumber == bossNumber)
                .ToList();
        }

        public static IEnumerable<DeclarationData> LoadDeclarationData(PlayerData playerData, byte bossNumber)
        {
            using var databaseConnector = new ShioriDBContext();

            playerData = databaseConnector.PlayerData.AsQueryable()
                .FirstOrDefault(d => d.PlayerID == playerData.PlayerID);

            if (playerData == null)
            {
                return null;
            }

            if (playerData.ClanData == null)
            {
                playerData = databaseConnector.PlayerData
                    .Include(x => x.ClanData)
                    .FirstOrDefault(x => x.PlayerID == playerData.PlayerID);
            }

            return databaseConnector.DeclarationData
                .Include(b => b.PlayerData)
                .Where(d => d.PlayerData.PlayerID == playerData.PlayerID && !d.DeleteFlag
                    && d.BattleLap == playerData.ClanData.GetBossLap(bossNumber) && d.BossNumber == bossNumber)
                .ToList();
        }

        public static bool CreateDeclarationData(DeclarationData declarationData)
        {
            var userData = declarationData.PlayerData;

            if (declarationData.PlayerID == 0 && userData == null)
            {
                return false;
            }

            using var databaseConnector = new ShioriDBContext();
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

        public static bool UpdateDeclarationData(DeclarationData declarationData)
        {
            var userData = declarationData.PlayerData;

            if (declarationData.PlayerID == 0 && userData == null)
            {
                return false;
            }

            using var databaseConnector = new ShioriDBContext();
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

        public static bool DeleteDeclarationData(DeclarationData declarationData)
            => DeleteDeclarationData(new[] { declarationData });

        /// <summary>
        /// 宣言データに削除フラグを立てます。
        /// </summary>
        /// <param name="declarationDataSet"></param>
        /// <returns></returns>
        public static bool DeleteDeclarationData(IEnumerable<DeclarationData> declarationDataSet)
        {
            using var databaseConnector = new ShioriDBContext();
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
        private static ulong PlayerDataToPlayerID(PlayerData playerData)
        {
            if (playerData == null || playerData.ClanData == null)
            {
                return 0;
            }

            var clanData = playerData.ClanData;
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.PlayerData
                .Include(d => d.ClanData)
                .Where(d => d.ClanData.ServerID == clanData.ServerID && d.ClanData.ClanRoleID == clanData.ClanRoleID
                    && d.UserID == playerData.UserID
                )
                .Select(d => d.PlayerID)
                .FirstOrDefault();
        }
    }
}
