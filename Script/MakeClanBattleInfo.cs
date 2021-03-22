using System;
using System.Collections.Generic;
using System.Text;

namespace PriconneBotConsoleApp.Script
{
    public class MakeClanBattleInfo
    {
        public MakeClanBattleInfo()
        {

        }

        public void loadClanBattleScadule()
        {
            var conn = new LoadRediveSQLiteData();
            conn.open();

            var schedule = conn.loadClanBattleScadule();

            conn.close();

            return;
        }
    }
}
