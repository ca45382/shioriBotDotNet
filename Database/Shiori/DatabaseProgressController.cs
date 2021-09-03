using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.Model;
using PriconneBotConsoleApp.DataType;

namespace PriconneBotConsoleApp.Database
{
    public static class DatabaseProgressController
    {
        public static IEnumerable<ProgressData> GetProgressData(ClanData clanData, BossNumberType bossNumber)
        {
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.ProgressData.AsQueryable()
                .Include(x => x.PlayerData)
                .Where(x => x.PlayerData.ClanID == clanData.ClanID && x.BossNumber == (byte)bossNumber && !x.DeleteFlag)
                .ToArray();
        }

        public static IEnumerable<ProgressData> GetProgressData(PlayerData playerData, BossNumberType bossNumber)
        {
            var playerID = playerData?.PlayerID ?? 0;
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.ProgressData.AsQueryable()
                .Where(x => x.PlayerID == playerID && x.BossNumber == (byte)bossNumber && !x.DeleteFlag)
                .ToArray();
        }

        public static bool CreateProgressData(ProgressData progressData)
        {
            if (progressData.PlayerID == 0)
            {
                return false;
            }

            progressData.PlayerData = null;

            using var databaseConnector = new ShioriDBContext();
            var transaction = databaseConnector.Database.BeginTransaction();

            try
            {
                databaseConnector.Add(progressData);
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

        public static bool ModifyProgressData(ProgressData progressData)
        {
            if (progressData.ProgressID == 0)
            {
                return false;
            }

            using var databaseConnector = new ShioriDBContext();
            var transaction = databaseConnector.Database.BeginTransaction();

            var updateData = databaseConnector.ProgressData.AsQueryable()
                .FirstOrDefault(x => x.ProgressID == progressData.ProgressID);

            if (updateData == null)
            {
                return false;
            }

            updateData.Damage = progressData.Damage;
            updateData.RemainTime = progressData.RemainTime;
            updateData.AttackType = progressData.AttackType;
            updateData.Status = progressData.Status;
            updateData.CarryOverFlag = progressData.CarryOverFlag;
            updateData.ReportID = progressData.ReportID;
            updateData.CommentData = progressData.CommentData;

            try
            {
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

        public static bool DeleteProgressData(IEnumerable<ProgressData> progressData)
        {
            if (progressData == null)
            {
                return false;
            }

            using var databaseConnector = new ShioriDBContext();
            var transaction = databaseConnector.Database.BeginTransaction();

            var deleteDataList = databaseConnector.ProgressData.AsQueryable()
                .Where(x => progressData.Select(y => y.ProgressID).Any(y => y == x.ProgressID))
                .ToList();

            try
            {
                deleteDataList.ForEach(x => x.DeleteFlag = true);
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

        public static bool DeleteProgressData(ProgressData progressData)
        {
            var deleteData = new ProgressData[]
            {
                progressData,
            };

            return DeleteProgressData(deleteData);
        }
    }
}
