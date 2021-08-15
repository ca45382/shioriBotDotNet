namespace PriconneBotConsoleApp.Define
{
    public static class ClanBattleDefine
    {
        public const short MaxLapNumber = 250;

        public static bool IsValidLapNumber(int value)
            => 0 <= value && value <= MaxLapNumber;
    }
}
