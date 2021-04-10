using System;
using System.Collections.Generic;
using System.Text;

namespace PriconneBotConsoleApp.DataTypes
{
    class ReservationData
    {
        public readonly string ServerID;
        public readonly string ClanRoleID;
        public readonly string UserID;
        public readonly string CommentData;
        //public string GuildUserName;
        public readonly int BossNumber;
        public readonly int BattleLaps;
        public readonly int AttackType;
        public readonly bool Reply;

        public ReservationData(
            string serverID, string clanRoleID, string userID, 
            int bossNumber, int battleLaps, string commentData,
            int attackType = -1, bool reply = true)
        {
            ServerID = serverID;
            ClanRoleID = clanRoleID;
            UserID = userID;
            CommentData = commentData;
            BossNumber = bossNumber;
            BattleLaps = battleLaps;
            AttackType = attackType;
            Reply = reply;
        }
    }
}
