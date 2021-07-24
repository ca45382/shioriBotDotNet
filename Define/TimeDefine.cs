using System;

namespace PriconneBotConsoleApp.Define
{
    public static class TimeDefine
    {
        public readonly static TimeSpan GameDateOffset = new(5, 0, 0);
        public readonly static TimeSpan DailyRefreshTime = new(5, 0, 30);
    }
}
