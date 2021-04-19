using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.EntityFrameworkCore;

using MySql.Data.MySqlClient;
using PriconneBotConsoleApp.DataTypes;

namespace PriconneBotConsoleApp.MySQL
{
    class MySQLReservationController
    {

        public List<ReservationData> LoadReservationData(ClanData clanData)
        {
            var reservationData = new List<ReservationData>();

            using var mySQLConnector = new MySQLConnector();
            var result = mySQLConnector.ReservationData
                .Include(b => b.PlayerData)
                .ThenInclude(d => d.ClanData)
                .Where(b => b.PlayerData.ClanData.ServerID == clanData.ServerID)
                .Where(b => b.PlayerData.ClanData.ClanID == clanData.ClanID)
                .Where(b => b.DeleteFlag == false)
                .ToList();


            reservationData = result;


            return reservationData;
        }

        public List<ReservationData> LoadReservationData(PlayerData playerData)
        {
            var reservationData = new List<ReservationData>();

            using var mySQLConnector = new MySQLConnector();

            var playerID = mySQLConnector.PlayerData
                .Include(b => b.ClanData)
                .Where(b => b.ClanData.ServerID == playerData.ClanData.ServerID)
                .Where(b => b.ClanData.ClanRoleID == playerData.ClanData.ClanRoleID)
                .Where(b => b.UserID == playerData.UserID)
                .Select(b => b.PlayerID)
                .FirstOrDefault();

            var result = mySQLConnector.ReservationData
                .Include(b => b.PlayerData)
                .Where(b => b.PlayerID == playerID)
                .Where(b => b.DeleteFlag == false)
                .ToList();

            reservationData = result;

            return reservationData;
        }

        public void CreateReservationData(ReservationData reservationData)
        {
            var userData = reservationData.PlayerData;

            using var mySQLConnector = new MySQLConnector();
            var transaction = mySQLConnector.Database.BeginTransaction();

            var playerID = mySQLConnector.PlayerData
                .Include(d => d.ClanData)
                .Where(d => d.ClanData.ServerID == userData.ClanData.ServerID)
                .Where(d => d.ClanData.ClanRoleID == userData.ClanData.ClanRoleID)
                .Where(d => d.UserID == userData.UserID)
                .Select(d => d.PlayerID)
                .FirstOrDefault();

            
            mySQLConnector.ReservationData.Add(
                new ReservationData()
                {
                    PlayerID = playerID,
                    BattleLaps = reservationData.BattleLaps,
                    BossNumber = reservationData.BossNumber,
                    CommentData = reservationData.CommentData
                });

            mySQLConnector.SaveChanges();
            transaction.Commit();

            return;
        }

        public void UpdateReservationData(ReservationData reservationData)
        {
            var userData = reservationData.PlayerData;

            using (var mySQLConnector = new MySQLConnector())
            {
                var transaction = mySQLConnector.Database.BeginTransaction();

                var playerID = mySQLConnector.PlayerData
                    .Include(d => d.ClanData)
                    .Where(d => d.ClanData.ServerID == userData.ClanData.ServerID)
                    .Where(d => d.ClanData.ClanRoleID == userData.ClanData.ClanRoleID)
                    .Where(d => d.UserID == userData.UserID)
                    .Select(d => d.PlayerID)
                    .FirstOrDefault();

                if (playerID == 0) return;

                var updateData = mySQLConnector.ReservationData
                    .Include(d => d.PlayerData)
                    .Where(d => d.PlayerID == playerID)
                    .Where(d => d.BattleLaps == reservationData.BattleLaps)
                    .Where(d => d.BossNumber == reservationData.BossNumber)
                    .Where(b => b.DeleteFlag == false)
                    .FirstOrDefault();

                updateData.CommentData = reservationData.CommentData;
                mySQLConnector.SaveChanges();
                transaction.Commit();

            }

            return;
        }

        public void DeleteReservationData(IEnumerable<ReservationData> reservationDataSet)
        {

            using (var mySQLConnector = new MySQLConnector())
            {
                var transaction = mySQLConnector.Database.BeginTransaction();

                foreach ( var reservationData in reservationDataSet)
                {
                    //var playerID = mySQLConnector.PlayerData
                    //    .Include(d => d.ClanData)
                    //    .Where(d => d.ClanData.ServerID == reservationData.PlayerData.ClanData.ServerID)
                    //    .Where(e => e.ClanData.ClanRoleID == reservationData.PlayerData.ClanData.ClanRoleID)
                    //    .Where(f => f.UserID == reservationData.PlayerData.UserID)
                    //    .Select(d => d.PlayerID)
                    //    .FirstOrDefault();

                    var updateData = mySQLConnector.ReservationData
                        .Include(d => d.PlayerData)
                        .Where(d => d.PlayerID == reservationData.PlayerID )
                        .Where(d => d.BossNumber == reservationData.BossNumber)
                        .Where(d => d.BattleLaps == reservationData.BattleLaps)
                        .Where(b => b.DeleteFlag == false)
                        .FirstOrDefault();

                    if (updateData == null) continue;
                    updateData.DeleteFlag = true;
                }

                mySQLConnector.SaveChanges();
                transaction.Commit();
            }
        }
    }
}
