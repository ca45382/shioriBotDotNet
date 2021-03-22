using System;
using System.Collections.Generic;
using System.Text;

namespace PriconneBotConsoleApp.Script
{
    public class DataSet
    {
        public class ClanBattleDate
        {
            
            private DateTime startBattle;
            private DateTime endBattle;

            public int ClanBattleID;
            public int Month;
            public string StartBattle
            {
                get { return startBattle.ToString();  }
                set { startBattle = DateTime.Parse(value); }

            }

            public string EndBattle
            {
                get { return endBattle.ToString(); }
                set { endBattle = DateTime.Parse(value); }

            }
        }
    }
}
