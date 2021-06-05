using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataTypes;
using System.Collections.Generic;
using System.Linq;

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
                .Include(b => b.MessageIDs)
                .Include(b => b.ChannelIDs)
                .Include(b => b.RoleIDs)
                .Where(b => b.ServerID == role.Guild.Id)
                .Where(b => b.ClanRoleID == role.Id)
                .FirstOrDefault();
        }

        public bool UpdateClanData (ClanData clanData)
        {
            using var databaseConnector = new DatabaseConnector();
            var transaction = databaseConnector.Database.BeginTransaction();

            var databaseClanData = databaseConnector.ClanData
                .Include(b => b.ServerData)
                .Where(b => b.ClanID == clanData.ClanID)
                .FirstOrDefault();

            if (databaseClanData == null)
            {
                transaction.Rollback();
                return false;
            }

            databaseClanData.BattleLap = clanData.BattleLap;
            databaseClanData.BossNumber = clanData.BossNumber;
            databaseClanData.ProgressiveFlag = clanData.ProgressiveFlag;
            //databaseClanData.BossRoleReady = clanData.BossRoleReady;
            databaseClanData.ClanName = clanData.ClanName;
            databaseConnector.SaveChanges();
            transaction.Commit();

            return true;
        }
    }
}
