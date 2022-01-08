namespace PriconneBotConsoleApp.DataType
{
    //チャンネル系 1xxx
    public enum ChannelFeatureType : uint
    {
        Unknown = 0,

        /// <summary>すべてのチャンネルで利用できるコマンドにつけられる.</summary>
        All = 9999,

        DeclareID = 1001,
        ReserveID = 1002,
        ReserveResultID = 1003,
        ProgressID = 1004,
        ReportID = 1005,
        CarryOverID = 1006,
        TaskKillID = 1007,
        TimeLineDisplayID = 1008,

        //凸宣言チャンネル 110x
        DeclareBoss1ID = 1101,
        DeclareBoss2ID = 1102,
        DeclareBoss3ID = 1103,
        DeclareBoss4ID = 1104,
        DeclareBoss5ID = 1105,

        //進行チャンネル 111x
        ProgressBoss1ID = 1111,
        ProgressBoss2ID = 1112,
        ProgressBoss3ID = 1113,
        ProgressBoss4ID = 1114,
        ProgressBoss5ID = 1115,
    }
}
