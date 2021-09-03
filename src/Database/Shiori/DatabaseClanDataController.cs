using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using ShioriBot.Net.Define;
using ShioriBot.Net.Model;

namespace ShioriBot.Net.Database
{
    public static class DatabaseClanDataController
    {
        public static List<ClanData> LoadClanData()
        {
            using var databaseConnector = new ShioriDBContext();
            return databaseConnector.ClanData.ToList();
        }

        public static IEnumerable<ClanData> LoadClanData(SocketGuild guild)
        {
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.ClanData
                .Include(x => x.MessageData).Include(x => x.MessageData).Include(x => x.RoleData)
                .Where(x => x.ServerID == guild.Id)
                .ToArray();
        }

        public static ClanData LoadClanData(SocketRole role)
        {
            using var databaseConnector = new ShioriDBContext();

            return databaseConnector.ClanData
                .Include(b => b.ServerData)
                .Include(b => b.MessageData)
                .Include(b => b.ChannelData)
                .Include(b => b.RoleData)
                .FirstOrDefault(b => b.ServerID == role.Guild.Id && b.ClanRoleID == role.Id);
        }

        public static bool UpdateClanData(ClanData clanData)
        {
            using var databaseConnector = new ShioriDBContext();
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
    }
}
