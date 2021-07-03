using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataModel;

namespace PriconneBotConsoleApp.Database
{
    public static class DatabaseReportDataController
    {
        /// <summary>
        /// 個人の凸報告データを取得する。
        /// </summary>
        /// <param name="playerData"></param>
        /// <returns></returns>
        public static IEnumerable<ReportData> GetReportData(PlayerData playerData)
        {
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.ReportData.AsQueryable()
                .Where(x => x.PlayerID == playerData.PlayerID && x.SubdueFlag == false && x.DeleteFlag == false)
                .ToList();
        }

        /// <summary>
        /// クランの凸報告データを取得する。
        /// </summary>
        /// <param name="clanData"></param>
        /// <returns></returns>
        public static IEnumerable<ReportData> GetReportData(ClanData clanData)
        {
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.ReportData.AsQueryable()
                .Include(x => x.PlayerData)
                .Where(x => x.PlayerData.ClanID == clanData.ClanID && x.SubdueFlag == false && x.DeleteFlag == false)
                .ToList();
        }

        /// <summary>
        /// 凸報告データをデータベースに登録する。
        /// </summary>
        /// <param name="reportData"></param>
        /// <returns></returns>
        public static bool CreateReportData(ReportData reportData)
        {
            if (reportData.PlayerID == 0)
            {
                return false;
            }

            reportData.PlayerData = null;

            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();
            try
            {
                databaseConnector.Add(reportData);
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

        /// <summary>
        /// 凸報告データを削除する。
        /// </summary>
        /// <param name="reportData"></param>
        /// <returns></returns>
        public static bool DeleteReportData(ReportData reportData)
        {
            var deleteData = new List<ReportData>()
            {
                reportData,
            };

            return DeleteReportData(deleteData);
        }

        /// <summary>
        /// 凸報告データを削除する。
        /// </summary>
        /// <param name="reportData"></param>
        /// <returns></returns>
        public static bool DeleteReportData(IEnumerable<ReportData> reportDataList)
        {
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            var deleteDataList =  databaseConnector.ReportData.AsQueryable()               
                .Where(x => reportDataList.Select(y => y.ReportID).Any(z => z == x.ReportID))
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
    }
}
