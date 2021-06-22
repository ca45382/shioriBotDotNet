using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    public enum ReactionType
    {
        [Description("👌")]
        Success = 101,

        [Description("❌")]
        Failure = 102,

        [Description("🔄")]
        Reload = 103,

        [Description("⚔️")]
        StartBattle = 1001,

        [Description("✅")]
        FinishBattle = 1002,

        [Description("🏁")]
        SubdueBoss = 1003,

        [Description("❌")]
        CancelBattle = 1004,
    }
}
