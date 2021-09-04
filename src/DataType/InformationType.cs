using System.ComponentModel;

namespace ShioriBot.Net.DataType
{
    public enum InformationType
    {
        Unknown,

        // 凸宣言関連
        [Description("{0}さんが{1}周目、{2}ボスに凸宣言を行いました。")]
        Declaration,

        [Description("{0}さんが{1}周目、{2}ボスへの本戦を終了しました。")]
        CompleteAttack,

        [Description("{0}さんが{1}周目、{2}ボスへの本戦を取り消しました。")]
        CancelAttack,

        [Description("{0}さんが{1}ボスを{2}周目に進めました。")]
        SubDueBoss,

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
