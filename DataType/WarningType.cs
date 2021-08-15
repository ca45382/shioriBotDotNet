using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriconneBotConsoleApp.DataType
{
    public enum WarningType
    {
        [Description("コメントが長いので切り取られました。\n 問題がある場合は予約削除をして再度予約してください。") ]
        TooLongComment,
    }
}
