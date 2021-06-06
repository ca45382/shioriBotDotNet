using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    public enum CampaignIconType
    {
        [Description("不明")]
        Unknown = 0,

        [Description("ドロップ")]
        Drop = 30,

        [Description("マナ")]
        Mana = 40,

        [Description("経験値")]
        Exp = 80,

        [Description("マスターコイン")]
        MasterCoin = 100,
    }
}
