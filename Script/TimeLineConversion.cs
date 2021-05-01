using System;
using System.Collections.Generic;
using System.Text;

using Discord;

namespace PriconneBotConsoleApp.Script
{
    class TimeLineConversion
    {
        public TimeLineConversion(IMessage message, int timeSec)
        {

            if (message == null)
            {
                return;
            }

            var messageData = loadTimeLineMessage(message);

        }

        private string loadTimeLineMessage(IMessage message)
        {


            return null;
        }

        private string ConversionMessage(string messageData, int timeData)
        {
            var messageContent =
                messageData.Split(new string[] { "\n" },StringSplitOptions.None);

            return null;
        }
    }
}
