using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

using PriconneBotConsoleApp.DataTypes;

namespace PriconneBotConsoleApp.MySQL
{
    class MySQLPlayerDataController
    {
        public List<PlayerData> LoadPlayerData(string serverID)
        {
            var commandString =
                 "SELECT clan_info.server_id, clan_info.clan_role_id, player_data.user_id, player_data.name " +
                 "FROM player_data JOIN clan_info ON player_data.clan_id = clan_info.clan_id " +
                 "WHERE server_id = @serverID";

            var sqlParameter = new SqlParameter("serverID", serverID);
            var botDatabase = new List<BotDatabase>();

            using (var mySQLConnector = new MySQLConnector())
            {
                botDatabase = mySQLConnector.BotDatabase
                    .FromSqlRaw(commandString, sqlParameter)
                    .ToList();
            }

            if (botDatabase.Count() == 0) { return null; }
            if (botDatabase.First().ClanData.Count == 0) { return null; }

            return botDatabase.First().ClanData.First().PlayerData;

        } 
    }
}
