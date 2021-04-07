using System;
using System.Collections.Generic;
using System.Text;

namespace PriconneBotConsoleApp.DataTypes
{
    class ClanData
    {
        public string ServerID;
        public string ClanRoleID;
        public string ClanName;

        public ChannelIDs ChannelIDs;
        public MessageIDs MessageIDs;
        public RoleIDs RoleIDs;
        public Status Status;

        public ClanData()
        {
            ChannelIDs = new ChannelIDs();
            MessageIDs = new MessageIDs();
            RoleIDs = new RoleIDs();
            Status = new Status();
        }
    }

    public class ChannelIDs
    {
        public string ProgressiveChannelID;
        public string ReportChannelID;
        public string CarryOverChannelID;
        public string TaskKillChannelID;
        public string DeclarationChannelID;
        public string ReservationChannelID;
        public string TimeLineConversionChannelID;
    }

    public class MessageIDs
    {
        public string ProgressiveMessageID;
        public string DeclarationMessageID;
        public string ReservationMessageID;
    }

    public class RoleIDs
    {
        public string TaskKillRoleID;
        public string FirstBossID;
        public string SecondBossID;
        public string ThirdBossID;
        public string FourthBossID;
        public string FifthBossID;
    }

    public class Status
    {
        public int BattleLaps = 0;
        public int BossNumber = 0;
        public bool ProgressiveFlag = false;
        public bool BossRoleReady = false;
    }
}
