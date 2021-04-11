using System;
using System.Collections.Generic;
using System.Text;

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
                    .FirstOrDefault();

                clanData = result;
            }

            return clanData;
        }
    }
}
