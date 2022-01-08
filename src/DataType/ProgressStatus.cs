using System.ComponentModel;

namespace ShioriBot.Net.DataType
{
    public enum ProgressStatus : byte
    {
        [Description("")]
        Unknown,

        [Description("✅")]
        AttackDone,

        [Description("⭕")]
        AttackReady,

        [Description("⏸️")]
        AttackReported,

        [Description("🚨")]
        SOS,

        [Description("🏃")]
        SubdueBoss,
    }
}
