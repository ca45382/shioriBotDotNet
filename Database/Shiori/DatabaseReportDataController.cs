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
        public static IEnumerable<ReportData> GetReportData(PlayerData playerData)
        {
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.ReportData.AsQueryable()
                .Where(x => x.PlayerID == playerData.PlayerID && x.DeleteFlag == false)
                .ToList();
        }

        public static IEnumerable<ReportData> GetReportData(ClanData clanData)
        {
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.ReportData.AsQueryable()
                .Include(x => x.PlayerData)
                .Where(x => x.PlayerData.ClanID == clanData.ClanID)
                .ToList();
        }


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
    }
}
