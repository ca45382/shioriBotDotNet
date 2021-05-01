using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;


namespace PriconneBotConsoleApp.Script
{
    class TimeLineConversion : BaseClass
    {

        private IMessage m_userMessage;

        private class ConvertData
        {
            public int Time;
            public string MessageGuildID = "";
            public string MessageChannelID = "";
            public string MessageID = "";
            public IMessage Message = null;
        }

        public TimeLineConversion(IMessage message)
        {
            m_userMessage = message;

        }

        public async Task RunByMessage()
        {
            var userMessage = m_userMessage;
            if (userMessage == null)
            {
                return;
            }
            if (!userMessage.Content.StartsWith("!tl"))
            {
                return;
            }
            var messageData = await loadTimeLineMessage(userMessage);
            if (messageData == null)
            {
                return;
            }
            var convertMessage = ConversionMessage(
                messageData.Message.Content, messageData.Time
                );

            var userChannelData = m_userMessage.Channel as ISocketMessageChannel;
            await SendMessageToChannel(userChannelData, convertMessage);
            return;
        }

        /// <summary>
        /// 引用するタイムラインデータの確定
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<ConvertData> loadTimeLineMessage(IMessage message)
        {
            var splitMessageContent =
                message.Content.Split(new string[] { " ", "　" }, StringSplitOptions.RemoveEmptyEntries);

            if (splitMessageContent.Length != 3)
            {
                return null;
            }

            if (!(int.TryParse(splitMessageContent[2], out int timeData)
                && timeData >= 20 && timeData <= 90
                ))
            {
                return null;
            }

            var convertData = new ConvertData()
            {
                Time = timeData
            };

            Uri uriData = new Uri(splitMessageContent[1]);

            var discordID = uriData.Segments;

            if (discordID.Count() != 5)
            {
                return null;
            }
            convertData.MessageGuildID = discordID[2].Replace("/", "");
            convertData.MessageChannelID = discordID[3].Replace("/", "");
            convertData.MessageID = discordID[4];


            var userChannelData = message.Channel as SocketGuildChannel;
            var timeLineChannelData = userChannelData.Guild.GetChannel(
                ulong.Parse(convertData.MessageChannelID)) as SocketTextChannel;
            convertData.Message = await timeLineChannelData.GetMessageAsync(
                ulong.Parse(convertData.MessageID)
                );

            if (convertData.Message == null)
            {
                return null;
            }

            return convertData;
        }

        /// <summary>
        /// TLデータから秒数を変換する機能
        /// 0:00 や 00秒 を持ち越し秒数に変換する
        /// </summary>
        /// <param name="messageData"></param>
        /// <param name="timeData"></param>
        /// <returns></returns>
        private string ConversionMessage(string messageData, int timeData)
        {
            var scrapTime = 90 - timeData;
            var messageContent =
                messageData.Split(new string[] { "\n" }, StringSplitOptions.None);

            var sendMessageContent = new StringBuilder();

            sendMessageContent.AppendLine("```Python");

            foreach (var lineMessageContent in messageContent)
            {
                if (Regex.IsMatch(lineMessageContent, $"```"))
                {
                    continue;
                }
                if (!(Regex.IsMatch(lineMessageContent, @"\d:\d{2}")
                    || Regex.IsMatch(lineMessageContent, @"\d+秒")
                    ))
                {
                    sendMessageContent.AppendLine(lineMessageContent);
                    continue;
                }
                var matchData = Regex.Matches(lineMessageContent, @"\d:\d{2}");

                var afterLineMessageContent = lineMessageContent;

                foreach (var matchTimeData in matchData)
                {
                    var timeDataContent =
                        matchTimeData.ToString().Split(
                            new string[] { ":" }, StringSplitOptions.None);
                    var minutes = int.Parse(timeDataContent[0]);
                    var seconds = int.Parse(timeDataContent[1]);
                    var afterSeconds = minutes * 60 + seconds - scrapTime;
                    var afterMinutes = 0;
                    if (afterSeconds >= 60)
                    {
                        afterMinutes = 1;
                        afterSeconds -= 60;
                    }
                    if (afterSeconds <= 0 && afterSeconds <= 0)
                    {
                        afterSeconds = 0;
                        afterMinutes = 0;
                    }
                    afterLineMessageContent = afterLineMessageContent.Replace(
                        matchTimeData.ToString(), $"{afterMinutes}:{afterSeconds:D2}");

                }

                matchData = Regex.Matches(lineMessageContent, @"\d{2}秒");

                foreach (var matchTimeData in matchData)
                {
                    var data = Regex.Replace(matchTimeData.ToString(), @"[^0-9]", "");
                    var seconds =
                         int.Parse(data);
                    var afterSeconds = seconds - scrapTime;
                    if (afterSeconds <= 0)
                    {
                        afterSeconds = 0;
                    }
                    afterLineMessageContent = afterLineMessageContent.Replace(
                        matchTimeData.ToString(), $"{afterSeconds:D2}秒");
                }

                sendMessageContent.AppendLine(afterLineMessageContent);
            }

            sendMessageContent.AppendLine("```");

            Console.WriteLine(sendMessageContent.ToString());
            return sendMessageContent.ToString();
        }
    }
}
