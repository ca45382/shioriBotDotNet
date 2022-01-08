﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ShioriBot.Net.DataType
{
    public enum InformationType
    {
        Unknown,

        // 凸報告関連
        [Description("<@{0}>の凸報告を代理削除しました。\nこのメッセージは{1}秒後削除されます。")]
        DeleteInstead,

        //持ち越し関連
        [Description("持ち越しをすべて削除しました。\nこのメッセージは{0}秒後削除されます。")]
        DeleteAllCarryOverData,

        // 持ち越し秒数計算機関連
        [Description("{0}人目が{1}秒の持ち越しを持ちます。")]
        CarryOverTimeResult,
    }
}
