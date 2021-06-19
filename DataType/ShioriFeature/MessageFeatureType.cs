using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    //メッセージ 2xxx
    public enum MessageFeatureType
    {
        Unknown = 0,

        DeclareID = 2001,
        ReserveResultID = 2002,
        ProgressID = 2003,

        //凸宣言チャンネルメッセージ 210x
        DeclareBoss1ID = 2101,
        DeclareBoss2ID = 2102,
        DeclareBoss3ID = 2103,
        DeclareBoss4ID = 2104,
        DeclareBoss5ID = 2105,

        //進行チャンネルメッセージ 211x
        ProgressBoss1ID = 2111,
        ProgressBoss2ID = 2112,
        ProgressBoss3ID = 2113,
        ProgressBoss4ID = 2114,
        ProgressBoss5ID = 2115,
    }
}
