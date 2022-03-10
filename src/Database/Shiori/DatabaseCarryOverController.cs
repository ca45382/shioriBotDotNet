using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShioriBot.Extension;
using ShioriBot.Model;

namespace ShioriBot.Database
{
    public static class DatabaseCarryOverController
    {
        public static IEnumerable<CarryOverData> GetCarryOverData()
        {
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.CarryOverData.AsQueryable()
                .Where(x => !x.DeleteFlag)
                .ToArray();
        }

        public static IEnumerable<CarryOverData> GetCarryOverData(PlayerData playerData)
        {
            if (playerData == null)
            {
                return null;
            }

            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.CarryOverData.AsQueryable()
                .Where(x => x.PlayerID == playerData.PlayerID && !x.DeleteFlag)
                .ToArray();
        }

        public static IEnumerable<CarryOverData> GetCarryOverData(ClanData clanData)
        {
            if (clanData == null)
            {
                return null;
            }

            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.CarryOverData.AsQueryable().Include(x => x.PlayerData)
                .Where(x => x.PlayerData.ClanID == clanData.ClanID && !x.DeleteFlag)
                .ToArray();
        }

        public static bool CreateCarryOverData(CarryOverData carryOverData)
        {
            if (carryOverData == null || carryOverData.PlayerID == 0)
            {
                return false;
            }

            carryOverData.PlayerData = null;

            using var databaseConnector = new ShioriDBContext();
            var transaction = databaseConnector.Database.BeginTransaction();
            try
            {
                databaseConnector.Add(carryOverData);
                databaseConnector.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (DbUpdateException e)
            {
                Console.WriteLine(e.Message);
                transaction.Rollback();
                return false;
            }
        }

        public static bool UpdateCarryOverData(CarryOverData carryOverData)
        {
            if (carryOverData.CarryOverID == 0)
            {
                return false;
            }

            carryOverData.PlayerData = null;

            using var databaseConnector = new ShioriDBContext();
            var transaction = databaseConnector.Database.BeginTransaction();

            var updateData = databaseConnector.CarryOverData.AsQueryable()
                .FirstOrDefault(x => x.CarryOverID == carryOverData.CarryOverID);

            if (updateData == null)
            {
                return false;
            }

            updateData.BattleLap = carryOverData.BattleLap;
            updateData.BossNumber = carryOverData.BossNumber;
            updateData.CommentData = carryOverData.CommentData;
            updateData.RemainTime = carryOverData.RemainTime;

            try
            {
                databaseConnector.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (DbUpdateException e)
            {
                Console.WriteLine(e.Message);
                transaction.Rollback();
                return false;
            }
        }

        public static bool DeleteCarryOverData(IEnumerable<CarryOverData> carryOverDataList)
        {
            using var databaseConnector = new ShioriDBContext();
            var transaction = databaseConnector.Database.BeginTransaction();

            var deleteDataList = databaseConnector.CarryOverData.AsQueryable()
               .Where(x => carryOverDataList.Select(y => y.CarryOverID).Any(y => y == x.CarryOverID))
               .AsEnumerable();
            deleteDataList.ForEach(x => x.DeleteFlag = true);

            try
            {
                databaseConnector.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (DbUpdateException e)
            {
                transaction.Rollback();
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static bool DeleteCarryOverData(CarryOverData carryOverData)
        {
            var carryOverList = new CarryOverData[]
            {
                carryOverData,
            };

            return DeleteCarryOverData(carryOverList);
        }
    }
}
