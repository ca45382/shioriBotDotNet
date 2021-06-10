using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    public enum CampaignSystemType
    {
        [Description("不明")]
        Unknown = 0,

        [Description("クエスト(N)")]
        NormalQuest = 101,

        [Description("クエスト(H)")]
        HardQuest = 102,

        [Description("探索")]
        Exploration = 103,

        [Description("ダンジョン")]
        Dungeon = 104,

        [Description("聖跡調査")]
        TempleSurvey = 109,

        [Description("クエスト(VH)")]
        VeryHardQuest = 111,

        [Description("神殿調査")]
        ShrineSurvey = 112,

        [Description("イベント(N)")]
        EventNormalQuest = 6004,

        [Description("イベント(H)")]
        EventHardQuest = 6005,
    }
}
