using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriconneBotConsoleApp.DataType
{
    public enum ProgressStatus : byte
    {
        Unknown = 0,
        AttackConfirm = 1,
        TemporaryConfirm = 2,
        SaveOurSouls = 3,
        CarryOver = 4,
    }
}
