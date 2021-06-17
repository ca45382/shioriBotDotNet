using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataModel;

namespace PriconneBotConsoleApp.Database
{
    class DatabaseClanDataController
    {
        public List<ClanData> LoadClanData()
        {
            using var databaseConnector = new DatabaseConnector();
            return databaseConnector.ClanData.ToList();
        }

        public ClanData LoadClanData(SocketRole role)
        {
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.ClanData
                .Include(b => b.ServerData)
                .Include(b => b.MessageData)
                .Include(b => b.ChannelData)
                .Include(b => b.roleData)
                .FirstOrDefault(b => b.ServerID == role.Guild.Id && b.ClanRoleID == role.Id);
        }

        public bool UpdateClanData (ClanData clanData)
        {
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            var databaseClanData = databaseConnector.ClanData.AsQueryable()
                .FirstOrDefault(b => b.ClanID == clanData.ClanID);

            if (databaseClanData == null)
            {
                transaction.Rollback();
                return false;
            }

            // 周回数アップデート
            databaseClanData.Boss1Lap = clanData.Boss1Lap;
            databaseClanData.Boss2Lap = clanData.Boss2Lap;
            databaseClanData.Boss3Lap = clanData.Boss3Lap;
            databaseClanData.Boss4Lap = clanData.Boss4Lap;
            databaseClanData.Boss5Lap = clanData.Boss5Lap;

            //予約機能
            databaseClanData.ReservationLap = clanData.ReservationLap;
            databaseClanData.ReservationStartTime = clanData.ReservationStartTime;
            databaseClanData.ReservationEndTime = clanData.ReservationEndTime;

            databaseClanData.ClanName = clanData.ClanName;
            databaseConnector.SaveChanges();
            transaction.Commit();

            return true;
        }
    }
}
