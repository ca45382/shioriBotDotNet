﻿using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    public enum ErrorType
    {
        [Description("予約に失敗しました。")]
        FailedReservation = 1,
        [Description("予約できません。予約可能時間は{0}～{1}です。")]
        OutOfReservationTime = 2,
        [Description("コメントが長いので切り取られました。\n 問題がある場合は予約削除をして再度予約してください。")]
        TooLongComment = 3,
        [Description("予約できません。予約が可能なボスは{0}周目{1}ボスまでです。")]
        OutOfReservationBossLaps = 4,
        [Description("報告件数が規定数を超えています。")]
        UpperLimitReport = 5,
    }
}
