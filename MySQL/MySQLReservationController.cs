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
            var result = mySQLConnector.reservationData
                .Include(b => b.PlayerData)
                .ThenInclude(d => d.ClanData)
                .Where(b => b.PlayerData.ClanData.ServerID == clanData.ServerID)
                .Where(b => b.PlayerData.ClanData.ClanID == clanData.ClanID)
                .Where(b => b.DeleteFlag == false)
                .ToList();

            reservationData = result;


            return reservationData;
        }

        public List<ReservationData> LoadResevationData(PlayerData playerData)
        {
            var reservationData = new List<ReservationData>();

            using var mySQLConnector = new MySQLConnector();
            var result = mySQLConnector.reservationData
                .Include(b => b.PlayerData)
                .ThenInclude(d => d.ClanData)
                .Where(b => b.PlayerData.ClanData.ServerID == playerData.ClanData.ServerID)
                .Where(b => b.PlayerData.ClanData.ClanID == playerData.ClanData.ClanID)
                .Where(b => b.PlayerData.UserID == playerData.UserID)
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

            
            mySQLConnector.reservationData.Add(
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

            using var mySQLConnector = new MySQLConnector();
            var transaction = mySQLConnector.Database.BeginTransaction();

            var playerID = mySQLConnector.reservationData
                .Include(d => d.PlayerData)
                .ThenInclude(c => c.ClanData)
                .Select(d => d.PlayerID)
                .FirstOrDefault();

        }

        public void DeleteReservationData(List<ReservationData> reservationDatas)
        {

        }

        private void DeleteData(ReservationData reservationData)
        {

        }

    }
}
