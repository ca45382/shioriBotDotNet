namespace ShioriBot.Net.Define
{
    public static class CommonDefine
    {
        public const int MinBossNumber = 1;
        public const int MaxBossNumber = 5;

        /// <summary> 最も周回数が小さいボスと殴ることの可能なボスの周回数の差分。</summary>
        public const int BattleLapRange = 1;

        public const int MinBattleTime = 20;
        public const int MaxBattleTime = 90;

        public const int MaxReportNumber = 3;
        public const int MaxCarryOverNumber = 3;
        public const int MaxClanPlayer = 30;

        /// <summary> 単位[万] 999999万 </summary>
        public const int MaxDamageValue = 999999;

        public const int DisplayDamageUnit = 10000;

        public static bool IsValidBossNumber(int value)
            => MinBossNumber <= value && value <= MaxBossNumber;

        public static bool IsValidBattleTime(int value)
            => MinBattleTime <= value && value <= MaxBattleTime;

        public static bool IsValidDamageValue(int value)
            => 0 <= value && value <= MaxDamageValue;

        public static bool IsValidBattleLap(int value)
            => 0 < value;
    }
}
