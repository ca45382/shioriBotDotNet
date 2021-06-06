using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    public enum CampaignSystemType
    {
        [Description("不明")]
        Unknown = 0,

        [Description("ノーマルクエスト")]
        NormalQuest = 101,

        [Description("ハードクエスト")]
        HardQuest = 102,

        [Description("探索")]
        Exploration = 103,

        [Description("ダンジョン")]
        Dungeon = 104,

        [Description("聖跡調査")]
        TempleSurvey = 109,

        [Description("ベリーハードクエスト")]
        VeryHardQuest = 111,

        [Description("神殿調査")]
        ShrineSurvey = 112,
    }
}
