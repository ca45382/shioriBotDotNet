using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    public enum ButtonType
    {
        Unknown,

        [Description("⚔️")]
        StartBattle,

        [Description("✅")]
        FinishBattle,

        [Description("🏁")]
        SubdueBoss,

        [Description("❌")]
        CancelBattle,
    }
}
