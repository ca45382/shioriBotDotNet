using System.Collections.Generic;
using Discord.WebSocket;

using System.Linq;
using Microsoft.EntityFrameworkCore;

using PriconneBotConsoleApp.DataTypes;

namespace PriconneBotConsoleApp.MySQL
{
    class MySQLClanDataController
    {
        public List<ClanData> LoadClanData()
        {
            var clanData = new List<ClanData>();

            using (var mySQLConnector= new MySQLConnector())
            {
                clanData = mySQLConnector.ClanData
                    .ToList();
            }

            return clanData;
        }

        public ClanData LoadClanData(SocketRole role)
        {
            var clanData = new ClanData();

            using(var mySQLConnector = new MySQLConnector())
            {
                var result = mySQLConnector.ClanData
                    .Include(b => b.BotDatabase)
                    .Include(b => b.MessageIDs)
                    .Include(b => b.ChannelIDs)
                    .Include(b => b.RoleIDs)
                    .Where(b => b.ServerID == role.Guild.Id.ToString())
                    .Where(b => b.ClanRoleID == role.Id.ToString())
                    .FirstOrDefault();

                clanData = result;
            }

            return clanData;
        }

        public bool UpdateClanData (ClanData clanData)
        {
            using (var mySQLConnector = new MySQLConnector())
            {
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
                mySQLData.BossRoleReady = clanData.BossRoleReady;
                mySQLData.ClanName = clanData.ClanName;
                mySQLConnector.SaveChanges();
                transaction.Commit();
            }

            return true;
        }
    }
}
