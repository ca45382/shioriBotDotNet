using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    public enum ErrorType
    {
        Unknown,

        //予約関連
        [Description("予約に失敗しました。")]
        FailedReservation,
        [Description("予約できません。予約可能時間は{0}～{1}です。")]
        OutOfReservationTime,
        [Description("コメントが長いので切り取られました。\n 問題がある場合は予約削除をして再度予約してください。")]
        TooLongComment,
        [Description("予約できません。予約が{0}周目まで可能です。")]
        OutOfReservationBossLaps,

        //凸報告関連
        [Description("報告件数が規定数を超えています。")]
        UpperLimitReport,
    }
}
