﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShioriBot.Define;
using ShioriBot.Extension;
using ShioriBot.Model;

namespace ShioriBot.Database
{
    public static class DatabaseReportDataController
    {
        /// <summary>
        /// すべての凸報告データを取得する。
        /// </summary>
        /// <param name="clanData"></param>
        /// <returns></returns>
        public static IEnumerable<ReportData> GetReportData()
        {
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.ReportData.AsQueryable()
                .Where(x => !x.DeleteFlag)
                .ToArray();
        }

        /// <summary>
        /// 個人の凸報告データを取得する。
        /// </summary>
        /// <param name="playerData"></param>
        /// <returns></returns>
        public static IEnumerable<ReportData> GetReportData(PlayerData playerData)
        {
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.ReportData.AsQueryable()
                .Where(x => x.PlayerID == playerData.PlayerID && !x.SubdueFlag && !x.DeleteFlag)
                .ToList();
        }

        /// <summary>
        /// クランの凸報告データを取得する。
        /// </summary>
        /// <param name="clanData"></param>
        /// <returns></returns>
        public static IEnumerable<ReportData> GetReportData(ClanData clanData)
        {
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.ReportData.AsQueryable()
                .Include(x => x.PlayerData)
                .Where(x => x.PlayerData.ClanID == clanData.ClanID && !x.SubdueFlag && !x.DeleteFlag)
                .ToList();
        }

        public static ReportData GetReportData(ProgressData progressData)
            => GetReportDataByID(progressData.ReportID);

        private static ReportData GetReportDataByID(ulong reportID)
        {
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.ReportData.AsQueryable()
                .Include(x => x.PlayerData)
                .FirstOrDefault(x => x.ReportID == reportID);
        }

        /// <summary>
        /// 配列の前から0凸完了～3凸完了
        /// </summary>
        /// <param name="clanData"></param>
        /// <returns></returns>
        public static int[] GetRemainPlayerCount(ClanData clanData)
        {
            using var databaseConnector = new ShioriDBContext();

            var reportCount = new int[CommonDefine.MaxReportNumber + 1];

            databaseConnector.PlayerData.AsQueryable()
                .Where(x => x.ClanID == clanData.ClanID)
                .Include(x => x.ReportData.Where(y => !y.DeleteFlag && !y.SubdueFlag))
                .AsEnumerable().ForEach(x => reportCount[x.ReportData.Count]++);

            return reportCount;
        }

        public static int GetReportCount(PlayerData playerData)
        {
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.ReportData.AsQueryable()
                .Count(x => x.PlayerID == playerData.PlayerID && !x.SubdueFlag && !x.DeleteFlag);
        }

        /// <summary>
        /// 凸報告データをデータベースに登録する。
        /// </summary>
        /// <param name="reportData"></param>
        /// <returns></returns>
        public static ReportData CreateReportData(ReportData reportData)
        {
            if (reportData.PlayerID == 0)
            {
                return null;
            }

            reportData.PlayerData = null;

            using var databaseConnector = new ShioriDBContext();
            var transaction = databaseConnector.Database.BeginTransaction();

            try
            {
                databaseConnector.Add(reportData);
                databaseConnector.SaveChanges();
                transaction.Commit();

                return databaseConnector.ReportData.AsQueryable()
                    .Where(x => x.PlayerID == reportData.PlayerID && !x.DeleteFlag
                        && x.DateTime == databaseConnector.ReportData.AsQueryable().Max(x => x.DateTime))
                    .FirstOrDefault();
            }
            catch
            {
                transaction.Rollback();
                return null;
            }
        }

        public static void UpdateReportData(ReportData reportData)
        {
            if (reportData.ReportID == 0)
            {
                return;
            }

            using var databaseConnector = new ShioriDBContext();
            var transaction = databaseConnector.Database.BeginTransaction();

            var serverData = databaseConnector.ReportData.AsQueryable()
                .FirstOrDefault(x => x.ReportID == reportData.ReportID);

            if (serverData == null)
            {
                transaction.Rollback();
                return;
            }

            serverData.AttackType = reportData.AttackType;
            serverData.BattleLap = reportData.BattleLap;
            serverData.BossNumber = reportData.BossNumber;
            serverData.FinalDamage = reportData.FinalDamage;
            serverData.SubdueFlag = reportData.SubdueFlag;

            try
            {
                databaseConnector.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
            }
        }

        /// <summary>
        /// 凸報告データを削除する。
        /// </summary>
        /// <param name="reportData"></param>
        /// <returns></returns>
        public static bool DeleteReportData(ReportData reportData, bool isValid = false)
        {
            var deleteData = new ReportData[]
            {
                reportData,
            };

            return DeleteReportData(deleteData, isValid);
        }

        /// <summary>
        /// 凸報告データを削除する。
        /// </summary>
        /// <param name="reportData"></param>
        /// <returns></returns>
        public static bool DeleteReportData(IEnumerable<ReportData> reportDataList, bool isValid = false)
        {
            using var databaseConnector = new ShioriDBContext();
            var transaction = databaseConnector.Database.BeginTransaction();

            var deleteDataList = databaseConnector.ReportData.AsQueryable()
                .Where(x => reportDataList.Select(y => y.ReportID).Any(y => y == x.ReportID))
                .ToList();

            deleteDataList.ForEach(x => x.DeleteFlag = true);

            if (isValid)
            {
                deleteDataList.ForEach(x => x.ValidFlag = true);
            }

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
    }
}
