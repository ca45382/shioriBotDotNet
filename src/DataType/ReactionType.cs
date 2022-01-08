using System.ComponentModel;

namespace ShioriBot.Net.DataType
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
