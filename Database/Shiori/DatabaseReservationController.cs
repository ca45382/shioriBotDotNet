using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataModel;

namespace PriconneBotConsoleApp.Database
{
    class DatabaseReservationController
    {
        public bool UpdateReservationMessageID(ClanData clanData, ulong messageID)
        {
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            var clanID = databaseConnector.ClanData
                .FirstOrDefault(d => d.ServerID == clanData.ServerID && d.ClanRoleID == clanData.ClanRoleID)
                ?.ClanID ?? 0;

            if (clanID == 0) 
            {
                transaction.Rollback();
                return false;
            }

            var updateData = databaseConnector.MessageIDs
                .FirstOrDefault(d => d.ClanID == clanID);

            if (updateData == null)
            {
                transaction.Rollback();
                return false;
            }

            updateData.ReservationMessageID = messageID;
            databaseConnector.SaveChanges();
            transaction.Commit();

            return true;
        }

        public List<ReservationData> LoadReservationData(ClanData clanData)
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

        public List<ReservationData> LoadReservationData(PlayerData playerData)
        {
            if (playerData == null)
            {
                return null;
            }

            using var databaseConnector = new DatabaseConnector();

            var playerID = LoadPlayerID(databaseConnector.PlayerData, playerData);
            
            return databaseConnector.ReservationData.AsQueryable()
                .Where(b => b.PlayerID == playerID && b.DeleteFlag == false)
                .OrderBy(o => o.BattleLap).ThenBy(d => d.BossNumber)
                .ToList();
        }

        public List<ReservationData> LoadBossLapReservationData(ClanData clanData)
        {
            using var databaseConnector = new DatabaseConnector();

            var clanID = databaseConnector.ClanData
                .FirstOrDefault(d => d.ServerID == clanData.ServerID && d.ClanRoleID == clanData.ClanRoleID)
                ?.ClanID ?? 0;

            if (clanID == 0)
            {
                return null;
            }

            return databaseConnector.ReservationData
                .Include(b => b.PlayerData)
                .Where(b => b.PlayerData.ClanID == clanID && !b.DeleteFlag
                    && b.BattleLap == clanData.BattleLap && b.BossNumber == clanData.BossNumber)
                .OrderBy(d => d.CreateDateTime)
                .ToList();
        }

        /// <summary>
        /// 予約データの作成。
        /// </summary>
        /// <param name="reservationData"></param>
        public void CreateReservationData(ReservationData reservationData)
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
        public void UpdateReservationData(ReservationData reservationData)
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

        public void DeleteReservationData(ReservationData reservationData)
            => DeleteReservationData(new[] { reservationData });

        public void DeleteReservationData(IEnumerable<ReservationData> reservationDataSet)
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

        private ulong LoadPlayerID(IQueryable<PlayerData> queryable, PlayerData playerData)
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
