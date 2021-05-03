using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataTypes;
using System.Collections.Generic;
using System.Linq;

namespace PriconneBotConsoleApp.MySQL
{
    class MySQLDeclarationController
    {
        public bool UpdateDeclarationMessageID(ClanData clanData, string messageID)
        {
            using var mySQLConnector = new MySQLConnector();
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
            return true;
        }

        public IEnumerable<DeclarationData> LoadDeclarationData(ClanData clanData)
        {
            var mySQLConnector = new MySQLConnector();

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

            return mySQLConnector.DeclarationData
                .Include(b => b.PlayerData)
                .ThenInclude(d => d.ClanData)
                .Where(d => d.PlayerData.ClanID == clanID)
                .Where(b => b.DeleteFlag == false)
                .Where(b => b.BattleLap == clanData.BattleLap)
                .Where(b => b.BossNumber == clanData.BossNumber)
                .ToList();
        }

        public IEnumerable<DeclarationData> LoadDeclarationData(PlayerData playerData)
        {
            using var mySQLConnector = new MySQLConnector();

            playerData = mySQLConnector.PlayerData
                .Include(d => d.ClanData)
                .Where(d => d.PlayerID == playerData.PlayerID)
                .FirstOrDefault();

            return mySQLConnector.DeclarationData
                .Include(b => b.PlayerData)
                .ThenInclude(d => d.ClanData)
                .Where(d => d.PlayerData.PlayerID == playerData.PlayerID)
                .Where(b => b.DeleteFlag == false)
                .Where(b => b.BattleLap == playerData.ClanData.BattleLap)
                .Where(b => b.BossNumber == playerData.ClanData.BossNumber)
                .ToList();
        }

        public bool CreateDeclarationData(DeclarationData declarationData)
        {
            var userData = declarationData.PlayerData;
            
            if (declarationData.PlayerID == 0 && userData == null)
            {
                return false;
            }

            using var mySQLConnector = new MySQLConnector();
            var transaction = mySQLConnector.Database.BeginTransaction();

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
                mySQLConnector.Add(declarationData);
                mySQLConnector.SaveChanges();
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

            using var mySQLConnector = new MySQLConnector();
            var transaction = mySQLConnector.Database.BeginTransaction();

            if (declarationData.PlayerID == 0)
            {
                declarationData.PlayerID = PlayerDataToPlayerID(userData);

                if (declarationData.PlayerID == 0)
                {
                    return false;
                }
            }

            var updateData = mySQLConnector.DeclarationData
                .Include(d => d.PlayerData)
                .Where(d => d.PlayerID == declarationData.PlayerID)
                .Where(d => d.BattleLap == declarationData.BattleLap)
                .Where(d => d.BossNumber == declarationData.BossNumber)
                .Where(d => d.FinishFlag == false)
                .Where(b => b.DeleteFlag == false)
                .FirstOrDefault();

            if (updateData != null)
            {
                updateData.FinishFlag = declarationData.FinishFlag;
            }

            mySQLConnector.SaveChanges();
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
            using var mySQLConnector = new MySQLConnector();
            var transaction = mySQLConnector.Database.BeginTransaction();

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

                var userDeleteDataSet = mySQLConnector.DeclarationData
                    .Include(d => d.PlayerData)
                    .Where(d => d.PlayerID == declarationData.PlayerID)
                    .Where(b => b.DeleteFlag == false)
                    .Where(d => d.BossNumber == declarationData.BossNumber)
                    .Where(d => d.BattleLap == declarationData.BattleLap)
                    .Where(d => d.FinishFlag == declarationData.FinishFlag);

                foreach (var updateData in userDeleteDataSet)
                {
                    updateData.DeleteFlag = true;
                }
            }

            mySQLConnector.SaveChanges();
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
            using var mySQLConnector = new MySQLConnector();

            return mySQLConnector.PlayerData
                .Include(d => d.ClanData)
                .ThenInclude(e => e.BotDatabase)
                .Where(d => d.ClanData.ServerID == clanData.ServerID)
                .Where(d => d.ClanData.ClanRoleID == clanData.ClanRoleID)
                .Where(d => d.UserID == playerData.UserID)
                .Select(d => d.PlayerID)
                .FirstOrDefault();
        }
    }
}
