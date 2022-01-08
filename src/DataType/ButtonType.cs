using PriconneBotConsoleApp.Attribute;

namespace PriconneBotConsoleApp.DataType
{
    /// <summary> Long : 文字列, Short : 絵文字 </summary>
    public enum ButtonType
    {
        Unknown,

        [MultiDescription("開始", "⚔️")]
        StartBattle,

        [MultiDescription("完了", "✅")]
        FinishBattle,

        [MultiDescription("討伐", "🏁")]
        SubdueBoss,

        [MultiDescription("取消", "✖️")]
        CancelBattle,

        [MultiDescription("更新", "🔄")]
        Reload,
    }
}
