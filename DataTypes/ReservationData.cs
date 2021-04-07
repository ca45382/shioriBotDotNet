using System;
using System.Collections.Generic;
using System.Text;

namespace PriconneBotConsoleApp.DataTypes
{
    class ReservationData
    {
        public string ServerID;
        public string ClanRoleID;
        public string UserID;
        public string CommentData;
        //public string GuildUserName;
        public int BossNumber;
        public int BattleLaps;
        public int AttackType;
        public bool Reply;
        public bool DataReady = false;
    }
}
