using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    public enum ReactionType
    {
        Unknown,

        [Description("👌")]
        Success,

        [Description("❌")]
        Failure,
    }
}
