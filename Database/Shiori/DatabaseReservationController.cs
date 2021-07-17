using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Define;
using PriconneBotConsoleApp.Extension;

namespace PriconneBotConsoleApp.Database
{
    public static class DatabaseReservationController
    {

        public static List<ReservationData> LoadReservationData(ClanData clanData)
        {
            using var databaseConnector = new DatabaseConnector();

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
            if (playerData == null)
            {
                return null;
            }

            using var databaseConnector = new DatabaseConnector();

            var playerID = LoadPlayerID(databaseConnector.PlayerData, playerData);
            
            return databaseConnector.ReservationData.AsQueryable()
                .Where(b => b.PlayerID == playerID && !b.DeleteFlag)
                .OrderBy(o => o.BattleLap).ThenBy(d => d.BossNumber)
                .ToList();
        }

        public static List<ReservationData> LoadBossLapReservationData(ClanData clanData, int bossNumber)
        {
            if ( bossNumber < Common.MinBossNumber || bossNumber > Common.MaxBossNumber)
            {
                return null;
            }

            using var databaseConnector = new DatabaseConnector();

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
            var playerData = reservationData.PlayerData;
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();
            var playerID = LoadPlayerID(databaseConnector.PlayerData, playerData);

            if (playerID == 0)
            {
                transaction.Rollback();
                return;
            }

            databaseConnector.ReservationData.Add(
                new ReservationData()
                {
                    PlayerID = playerID,
                    BattleLap = reservationData.BattleLap,
                    BossNumber = reservationData.BossNumber,
                    CommentData = reservationData.CommentData
                });

            databaseConnector.SaveChanges();
            transaction.Commit();
        }

        /// <summary>
        /// 予約データを更新する。
        /// </summary>
        /// <param name="reservationData"></param>
        public static void UpdateReservationData(ReservationData reservationData)
        {
            var playerData = reservationData.PlayerData;
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();
            var playerID = LoadPlayerID(databaseConnector.PlayerData, playerData);

            if (playerID == 0)
            {
                transaction.Rollback();
                return;
            }

            var updateData = databaseConnector.ReservationData
                .FirstOrDefault(d => d.PlayerID == playerID && d.BattleLap == reservationData.BattleLap
                    && d.BossNumber == reservationData.BossNumber && !d.DeleteFlag );

            if (updateData == null)
            {
                transaction.Rollback();
                return;
            }

            updateData.CommentData = reservationData.CommentData;
            databaseConnector.SaveChanges();
            transaction.Commit();
        }

        public static void DeleteReservationData(ReservationData reservationData)
            => DeleteReservationData(new[] { reservationData });

        public static void DeleteReservationData(IEnumerable<ReservationData> reservationDataSet)
        {
            using var databaseConnector = new DatabaseConnector();
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

        private static ulong LoadPlayerID(IQueryable<PlayerData> queryable, PlayerData playerData)
        {
            return queryable
                .Include(b => b.ClanData)
                .FirstOrDefault(b => b.ClanData.ServerID == playerData.ClanData.ServerID
                    && b.ClanData.ClanRoleID == playerData.ClanData.ClanRoleID
                    && b.UserID == playerData.UserID)
                ?.PlayerID ?? 0;
        }
    }
}
