using PriconneBotConsoleApp.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace PriconneBotConsoleApp.Script
{
    public class ClanBattleInfoLoader
    {
        public ClanBattleInfoLoader()
        {

        }

        public void LoadClanBattleScadule()
        {
            List<ClanBattleDate> schedule;
            using (var rediveDatabaseConnection = new RediveDatabaseLoader())
            {
                schedule = rediveDatabaseConnection.LoadClanBattleSchedule();
            }

        }
    }
}
