using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    public enum InfomationType
    {
        Unknown,

        // 凸報告関連
        [Description("<@{0}>の凸報告を代理削除しました。\nこのメッセージは{1}秒後削除されます。")]
        DeleteInsted,
    }
}
