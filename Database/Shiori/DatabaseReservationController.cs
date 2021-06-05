﻿using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataTypes;
using System.Collections.Generic;
using System.Linq;

namespace PriconneBotConsoleApp.Database
{
    class DatabaseReservationController
    {
        public bool UpdateReservationMessageID(ClanData clanData, ulong messageID)
        {
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            var clanID = databaseConnector.ClanData
                .Include(d => d.ServerData)
                .Where(d => d.ServerID == clanData.ServerID)
                .Where(d => d.ClanRoleID == clanData.ClanRoleID)
                .Select(d => d.ClanID)
                .FirstOrDefault();

            if (clanID == 0) 
            {
                transaction.Rollback();
                return false;
            }

            var updateData = databaseConnector.MessageIDs
                .Include(d => d.ClanData)
                .Where(d => d.ClanID == clanID)
                .FirstOrDefault();

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
                .Where(b => b.PlayerData.ClanData.ServerID == clanData.ServerID)
                .Where(b => b.PlayerData.ClanData.ClanID == clanData.ClanID)
                .Where(b => b.DeleteFlag == false)
                .OrderBy(o => o.BattleLap)
                .ThenBy(d => d.BossNumber)
                .ThenBy(d => d.CreateDateTime)
                .ToList();
        }

        public List<ReservationData> LoadReservationData(PlayerData playerData)
        {
            if (playerData == null)
            {
                return null;
            }

            using var databaseConnector = new DatabaseConnector();

            var playerDataOnSQL = databaseConnector.PlayerData
                .Include(b => b.ClanData)
                .ThenInclude(b => b.ServerData)
                .Where(b => b.ClanData.ServerID == playerData.ClanData.ServerID
                    && b.ClanData.ClanRoleID == playerData.ClanData.ClanRoleID
                    && b.UserID == playerData.UserID);

            var playerID = playerDataOnSQL.FirstOrDefault()?.PlayerID;

            return databaseConnector.ReservationData
                .Include(b => b.PlayerData)
                .Where(b => b.PlayerID == playerID)
                .Where(b => b.DeleteFlag == false)
                .OrderBy(o => o.BattleLap)
                .ThenBy(d => d.BossNumber)
                .ToList();
        }

        public List<ReservationData> LoadBossLapReservationData(ClanData clanData)
        {
            using var databaseConnector = new DatabaseConnector();

            var clanID = databaseConnector.ClanData
                .Include(d => d.ServerData)
                .Where(d => d.ServerID == clanData.ServerID)
                .Where(d => d.ClanRoleID == clanData.ClanRoleID)
                .Select(d => d.ClanID)
                .FirstOrDefault();

            if (clanID == 0)
            {
                return null;
            }

            return databaseConnector.ReservationData
                .Include(b => b.PlayerData)
                .ThenInclude(d => d.ClanData)
                .Where(d => d.PlayerData.ClanID == clanID)
                .Where(b => b.DeleteFlag == false)
                .Where(b => b.BattleLap == clanData.BattleLap)
                .Where(b => b.BossNumber == clanData.BossNumber)
                .OrderBy(d => d.CreateDateTime)
                .ToList();
        }

        public void CreateReservationData(ReservationData reservationData)
        {
            var userData = reservationData.PlayerData;
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            var playerID = databaseConnector.PlayerData
                .Include(d => d.ClanData)
                .Where(d => d.ClanData.ServerID == userData.ClanData.ServerID)
                .Where(d => d.ClanData.ClanRoleID == userData.ClanData.ClanRoleID)
                .Where(d => d.UserID == userData.UserID)
                .Select(d => d.PlayerID)
                .FirstOrDefault();

            
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

        public void UpdateReservationData(ReservationData reservationData)
        {
            var userData = reservationData.PlayerData;
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            var playerID = databaseConnector.PlayerData
                .Include(d => d.ClanData)
                .Where(d => d.ClanData.ServerID == userData.ClanData.ServerID)
                .Where(d => d.ClanData.ClanRoleID == userData.ClanData.ClanRoleID)
                .Where(d => d.UserID == userData.UserID)
                .Select(d => d.PlayerID)
                .FirstOrDefault();

            if (playerID == 0)
            {
                return;
            }

            var updateData = databaseConnector.ReservationData
                .Include(d => d.PlayerData)
                .Where(d => d.PlayerID == playerID)
                .Where(d => d.BattleLap == reservationData.BattleLap)
                .Where(d => d.BossNumber == reservationData.BossNumber)
                .Where(b => b.DeleteFlag == false)
                .FirstOrDefault();

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

                var updateData = databaseConnector.ReservationData
                    .Include(d => d.PlayerData)
                    .Where(d => d.PlayerID == reservationData.PlayerID)
                    .Where(d => d.BossNumber == reservationData.BossNumber)
                    .Where(d => d.BattleLap == reservationData.BattleLap)
                    .Where(b => b.DeleteFlag == false)
                    .FirstOrDefault();

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
