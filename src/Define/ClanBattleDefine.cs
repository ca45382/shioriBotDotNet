namespace ShioriBot.Net.Define
{
    public static class ClanBattleDefine
    {
        public const ushort MaxLapNumber = 250;

        public static bool IsValidLapNumber(int value)
            => 0 <= value && value <= MaxLapNumber;
    }
}
