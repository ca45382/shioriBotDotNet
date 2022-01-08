using System.ComponentModel;

namespace ShioriBot.DataType
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
