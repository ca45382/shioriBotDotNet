using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataTypes;
using System.Collections.Generic;
using System.Linq;

namespace PriconneBotConsoleApp.MySQL
{
    class MySQLClanDataController
    {
        public List<ClanData> LoadClanData()
        {
            using var mySQLConnector = new MySQLConnector();
            return mySQLConnector.ClanData.ToList();
        }

        public ClanData LoadClanData(SocketRole role)
        {
            using var mySQLConnector = new MySQLConnector();

            return mySQLConnector.ClanData
                .Include(b => b.BotDatabase)
                .Include(b => b.MessageIDs)
                .Include(b => b.ChannelIDs)
                .Include(b => b.RoleIDs)
                .Where(b => b.ServerID == role.Guild.Id)
                .Where(b => b.ClanRoleID == role.Id)
                .FirstOrDefault();
        }

        public bool UpdateClanData (ClanData clanData)
        {
            using var mySQLConnector = new MySQLConnector();
            var transaction = mySQLConnector.Database.BeginTransaction();

            var mySQLData = mySQLConnector.ClanData
                .Include(b => b.BotDatabase)
                .Where(b => b.ClanID == clanData.ClanID)
                .FirstOrDefault();

            if (mySQLData == null)
            {
                transaction.Rollback();
                return false;
            }

            mySQLData.BattleLap = clanData.BattleLap;
            mySQLData.BossNumber = clanData.BossNumber;
            mySQLData.ProgressiveFlag = clanData.ProgressiveFlag;
            //mySQLData.BossRoleReady = clanData.BossRoleReady;
            mySQLData.ClanName = clanData.ClanName;
            mySQLConnector.SaveChanges();
            transaction.Commit();

            return true;
        }
    }
}
