using System;

namespace PriconneBotConsoleApp.Define
{
    public static class TimeDefine
    {
        public readonly static TimeSpan GameDateOffset = new(5, 0, 0);
        public readonly static TimeSpan DailyRefreshTime = new(5, 0, 30);

        public readonly static TimeSpan ErrorMessageDisplayTime = new(0, 0, 5);
        public readonly static TimeSpan WarningMessageDisplayTime = new(0, 0, 20);
        public readonly static TimeSpan SuccessMessageDisplayTime = new(0, 0, 30);
    }
}
