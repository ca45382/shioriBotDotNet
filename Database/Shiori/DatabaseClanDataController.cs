using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;
using PriconneBotConsoleApp.Define;

namespace PriconneBotConsoleApp.Database
{
    public static class DatabaseClanDataController
    {
        public static List<ClanData> LoadClanData()
        {
            using var databaseConnector = new DatabaseConnector();
            return databaseConnector.ClanData.ToList();
        }

        public static ClanData LoadClanData(SocketRole role)
        {
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.ClanData
                .Include(b => b.ServerData)
                .Include(b => b.MessageData)
                .Include(b => b.ChannelData)
                .Include(b => b.RoleData)
                .FirstOrDefault(b => b.ServerID == role.Guild.Id && b.ClanRoleID == role.Id);
        }

        public static bool UpdateClanData(ClanData clanData)
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
            for (var i = 0; i < CommonDefine.MaxBossNumber; i++)
            {
                databaseClanData.SetBossLap(i + 1, clanData.GetBossLap(i + 1));
            }

            //予約機能
            databaseClanData.ReservationLap = clanData.ReservationLap;
            databaseClanData.ReservationStartTime = clanData.ReservationStartTime;
            databaseClanData.ReservationEndTime = clanData.ReservationEndTime;

            databaseClanData.ClanName = clanData.ClanName;
            databaseConnector.SaveChanges();
            transaction.Commit();

            return true;
        }

        public static bool UpdateClanChannelData(ClanData clanData, ulong channelID, ChannelFeatureType channelType)
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

            var updateData = databaseConnector.ChannelData
                .FirstOrDefault(d => d.ClanID == clanID && d.FeatureID == (uint)channelType);

            if (updateData == null)
            {
                databaseConnector.ChannelData.Add(
                    new ChannelData()
                    {
                        ClanID = clanID,
                        FeatureID = (uint)channelType,
                        ChannelID = channelID,
                    });
            }
            else
            {
                updateData.ChannelID = channelID;
            }

            databaseConnector.SaveChanges();
            transaction.Commit();

            return true;
        }

        public static void UpdateClanRoleData(RoleData roleData)
        {

        }
    }
}
