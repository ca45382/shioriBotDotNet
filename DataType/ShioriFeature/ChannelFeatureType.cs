using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    //チャンネル系 1xxx
    public enum ChannelFeatureType
    {
        [Description("")]
        Unknown = 0,

        [Description("DeclareID")]
        DeclareID = 1001,

        [Description("ReserveID")]
        ReserveID = 1002,

        [Description("ReserveResultID")]
        ReserveResultID = 1003,

        [Description("ProgressID")]
        ProgressID = 1004,

        [Description("ReportID")]
        ReportID = 1005,

        [Description("CarryOverID")]
        CarryOverID = 1006,

        [Description("TaskKillID")]
        TaskKillID = 1007,

        [Description("TimeLineID")]
        TimeLineDisplayID = 1008,

        //凸宣言チャンネル 110x
        [Description("DeclareBoss1ID")]
        DeclareBoss1ID = 1101,

        [Description("DeclareBoss2ID")]
        DeclareBoss2ID = 1102,

        [Description("DeclareBoss3ID")]
        DeclareBoss3ID = 1103,
        
        [Description("DeclareBoss4ID")]
        DeclareBoss4ID = 1104,
        
        [Description("DeclareBoss5ID")]
        DeclareBoss5ID = 1105,

        //進行チャンネル 111x
        [Description("ProgressBoss1ID")]
        ProgressBoss1ID = 1111,

        [Description("ProgressBoss2ID")]
        ProgressBoss2ID = 1112,

        [Description("ProgressBoss3ID")]
        ProgressBoss3ID = 1113,

        [Description("ProgressBoss4ID")]
        ProgressBoss4ID = 1114,

        [Description("ProgressBoss5ID")]
        ProgressBoss5ID = 1115,
    }
}
