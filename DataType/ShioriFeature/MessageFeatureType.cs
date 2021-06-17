using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    //メッセージ 2xxx
    public enum MessageFeatureType
    {
        [Description("")]
        Unknown = 0,

        [Description("DeclareID")]
        DeclareID = 2001,

        [Description("ReserveResultID")]
        ReserveResultID = 2002,

        [Description("ProgressID")]
        ProgressID = 2003,


        //凸宣言チャンネルメッセージ 210x
        [Description("DeclareBoss1ID")]
        DeclareBoss1ID = 2101,

        [Description("DeclareBoss2ID")]
        DeclareBoss2ID = 2102,

        [Description("DeclareBoss3ID")]
        DeclareBoss3ID = 2103,

        [Description("DeclareBoss4ID")]
        DeclareBoss4ID = 2104,

        [Description("DeclareBoss5ID")]
        DeclareBoss5ID = 2105,

        //進行チャンネルメッセージ 211x
        [Description("ProgressBoss1ID")]
        ProgressBoss1ID = 2111,

        [Description("ProgressBoss1ID")]
        ProgressBoss2ID = 2112,

        [Description("ProgressBoss1ID")]
        ProgressBoss3ID = 2113,

        [Description("ProgressBoss1ID")]
        ProgressBoss4ID = 2114,

        [Description("ProgressBoss1ID")]
        ProgressBoss5ID = 2115,
    }
}
