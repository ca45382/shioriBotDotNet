using PriconneBotConsoleApp.Attribute;

namespace PriconneBotConsoleApp.DataType
{
    public enum AttackType
    {
        Unknown,

        [MultiDescription("物理", "物", "b", "B")]
        Physics,

        [MultiDescription("魔法", "魔", "m", "M")]
        Magic,

        [MultiDescription("ニャル", "ニ", "n", "N")]
        NewYearKyaru,

        [MultiDescription("持ち越し", "持", "-", "持越し", "持越")]
        CarryOver,
    }
}
