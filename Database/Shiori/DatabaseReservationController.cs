using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.Define;
using PriconneBotConsoleApp.Model;

namespace PriconneBotConsoleApp.Database
{
    public static class DatabaseReservationController
    {

        public static IEnumerable<ReservationData> LoadReservationData()
        {
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.ReservationData.AsQueryable()
                .Where(x => !x.DeleteFlag)
                .ToArray();
        }

        public static List<ReservationData> LoadReservationData(ClanData clanData)
        {
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.ReservationData
                .Include(b => b.PlayerData)
                .ThenInclude(d => d.ClanData)
                .Where(b => b.PlayerData.ClanData.ServerID == clanData.ServerID
                    && b.PlayerData.ClanData.ClanID == clanData.ClanID && !b.DeleteFlag)
                .OrderBy(o => o.BattleLap).ThenBy(d => d.BossNumber).ThenBy(d => d.CreateDateTime)
                .ToList();
        }

        public static List<ReservationData> LoadReservationData(PlayerData playerData)
        {
            if (playerData == null|| playerData.PlayerID == 0)
            {
                return null;
            }

            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.ReservationData.AsQueryable()
                .Where(b => b.PlayerID == playerData.PlayerID && !b.DeleteFlag)
                .OrderBy(o => o.BattleLap).ThenBy(d => d.BossNumber)
                .ToList();
        }

        public static IEnumerable<ReservationData> LoadReservationData(ClanData clanData, int bossNumber)
        {
            if (!CommonDefine.IsValidBossNumber(bossNumber))
            {
                return null;
            }

            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.ReservationData.AsQueryable()
                .Include(x => x.PlayerData)
                .Where(x => x.PlayerData.ClanID == clanData.ClanID && !x.DeleteFlag && x.BossNumber == bossNumber)
                .ToArray();
        }

        public static List<ReservationData> LoadBossLapReservationData(ClanData clanData, int bossNumber)
        {
            if (!CommonDefine.IsValidBossNumber(bossNumber))
            {
                return null;
            }

            using var databaseConnector = new ShioriDBContext();

            var databaseClanData = databaseConnector.ClanData
                .FirstOrDefault(d => d.ServerID == clanData.ServerID && d.ClanRoleID == clanData.ClanRoleID);

            if (databaseClanData == null)
            {
                return null;
            }

            return databaseConnector.ReservationData
                .Include(b => b.PlayerData)
                .Where(b => b.PlayerData.ClanID == databaseClanData.ClanID && !b.DeleteFlag
                    && b.BossNumber == bossNumber && b.BattleLap == clanData.GetBossLap(bossNumber))
                .OrderBy(d => d.CreateDateTime)
                .ToList();
        }

        /// <summary>
        /// 予約データの作成。
        /// </summary>
        /// <param name="reservationData"></param>
        public static void CreateReservationData(ReservationData reservationData)
        {
            if (reservationData.PlayerID == 0)
            {
                return;
            }

            using var databaseConnector = new ShioriDBContext();
            var transaction = databaseConnector.Database.BeginTransaction();

            databaseConnector.ReservationData.Add(reservationData);

            try
            {
                databaseConnector.SaveChanges();
                transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 予約データを更新する。
        /// </summary>
        /// <param name="reservationData"></param>
        public static void UpdateReservationData(ReservationData reservationData)
        {
            using var databaseConnector = new ShioriDBContext();
            var transaction = databaseConnector.Database.BeginTransaction();
            var updateData = databaseConnector.ReservationData.FirstOrDefault(x => x.ReserveID == reservationData.ReserveID);

            try
            {
                updateData.CommentData = reservationData.CommentData;
                databaseConnector.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
            }
        }

        public static void DeleteReservationData(ReservationData reservationData)
            => DeleteReservationData(new[] { reservationData });

        public static void DeleteReservationData(IEnumerable<ReservationData> reservationDataSet)
        {
            using var databaseConnector = new ShioriDBContext();
            var transaction = databaseConnector.Database.BeginTransaction();

            foreach (var reservationData in reservationDataSet)
            {
                var updateData = databaseConnector.ReservationData.AsQueryable()
                    .FirstOrDefault(d => d.PlayerID == reservationData.PlayerID && d.BossNumber == reservationData.BossNumber
                        && d.BattleLap == reservationData.BattleLap && !d.DeleteFlag);

                if (updateData != null)
                {
                    updateData.DeleteFlag = true;
                }
            }

            databaseConnector.SaveChanges();
            transaction.Commit();
        }
    }
}
