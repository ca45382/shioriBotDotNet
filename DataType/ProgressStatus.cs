using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriconneBotConsoleApp.DataType
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
        Fin,
    }
}
