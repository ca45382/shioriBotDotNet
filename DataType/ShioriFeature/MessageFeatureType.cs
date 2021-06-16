using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    public enum MessageFeatureType
    {
        [Description("")]
        Unknown = 0,

        [Description("ReserveID")]
        DeclareID = 2001,

        [Description("ReserveResultID")]
        ReserveResultID = 2002,

        [Description("ProgressID")]
        ProgressID = 2003,
    }
}
