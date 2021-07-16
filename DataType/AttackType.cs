using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    public enum AttackType
    {
        Unknown = 0,

        [Description("物理")]
        Physics = 101,

        [Description("魔法")]
        Magic = 102,

        [Description("ニャル")]
        NewYearKyaru = 103,

        [Description("持ち越し")]
        CarryOver = 109,

        [Description("持越し")]
        CarryOver2 = 119,

        [Description("物")]
        PhysicsShort = 201,

        [Description("魔")]
        MagicShort = 202,

        [Description("ニ")]
        NewYearKyaruShort = 203,

        [Description("b")]
        PhysicsShortRoman = 301,

        [Description("m")]
        MagicShortRoman = 302,

        [Description("n")]
        NewYearKYaruShortRoman = 303,
    }
}
