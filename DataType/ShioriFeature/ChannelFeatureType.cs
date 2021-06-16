using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    public enum ChannelFeatureType
    {
        [Description("")]
        Unknown = 0,

        [Description("DeclareID")]
        DeclareID = 1001,

        [Description("ReserveID")]
        ReserveID = 1002,

        [Description("ReserveResultID")]
        ReserveResultID = 1003,

        [Description("ProgressID")]
        ProgressID = 1004,

        [Description("ReportID")]
        ReportID = 1005,

        [Description("CarryOverID")]
        CarryOverID = 1006,

        [Description("TaskKillID")]
        TaskKillID = 1007,

        [Description("TimeLineID")]
        TimeLineDisplayID = 1008,
    }
}
