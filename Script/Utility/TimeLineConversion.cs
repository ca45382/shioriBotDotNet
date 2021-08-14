using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.Define;

namespace PriconneBotConsoleApp.Script
{
    public class TimeLineConversion
    {
        private CommandEventArgs m_CommandEventArgs;

        private class ConvertData
        {
            public int Time;
            public string MessageGuildID = "";
            public string MessageChannelID = "";
            public string MessageID = "";
            public IMessage Message = null;
        }

        public TimeLineConversion(CommandEventArgs commandEventArgs)
            => m_CommandEventArgs = commandEventArgs;

        public async Task RunByMessage()
        {
            var messageData = await LoadTimeLineMessage();

            if (messageData == null)
            {
                return;
            }

            var convertMessage = ConversionMessage(messageData.Message.Content, messageData.Time);
            await m_CommandEventArgs.Channel.SendMessageAsync(convertMessage);
        }

        /// <summary>
        /// 引用するタイムラインデータの確定.
        /// DiscordのメッセージからURLから引用する
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<ConvertData> LoadTimeLineMessage()
        {

            if (!int.TryParse(m_CommandEventArgs.Arguments[1], out int timeData) 
                || timeData < CommonDefine.MinBattleTime 
                || CommonDefine.MaxBattleTime < timeData  )
            {
                return null;
            }

            var convertData = new ConvertData
            {
                Time = timeData,
            };

            var uriData = new Uri(m_CommandEventArgs.Arguments[0]);
            var discordID = uriData.Segments;

            if (discordID.Length != 5)
            {
                return null;
            }

            convertData.MessageGuildID = discordID[2].Replace("/", "");
            convertData.MessageChannelID = discordID[3].Replace("/", "");
            convertData.MessageID = discordID[4];

            var timeLineChannelData = m_CommandEventArgs.Role.Guild.GetChannel(ulong.Parse(convertData.MessageChannelID)) as SocketTextChannel;
            convertData.Message = await timeLineChannelData.GetMessageAsync(ulong.Parse(convertData.MessageID));

            if (convertData.Message == null)
            {
                return null;
            }

            return convertData;
        }

        /// <summary>
        /// TLデータから秒数を変換する機能, 
        /// 0:00 や 00秒 を持ち越し秒数に変換する
        /// </summary>
        /// <param name="messageData"></param>
        /// <param name="timeData"></param>
        /// <returns></returns>
        private string ConversionMessage(string messageData, int timeData)
        {
            var scrapTime = 90 - timeData;
            var messageContent = messageData.Split("\n");
            var sendMessageContent = new StringBuilder();
            sendMessageContent.AppendLine("```Python");

            foreach (var lineMessageContent in messageContent)
            {
                if (Regex.IsMatch(lineMessageContent, $"```"))
                {
                    continue;
                }

                if (!Regex.IsMatch(lineMessageContent, @"(\d{1,2}:\d{2}|\d+[秒s])"))
                {
                    sendMessageContent.AppendLine(lineMessageContent);
                    continue;
                }

                var afterLineMessageContent = lineMessageContent;

                foreach (Match matchTimeData in Regex.Matches(lineMessageContent, @"\d{1,2}:\d{2}"))
                {
                    var timeDataContent = matchTimeData.Value.Split(":");
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
                        afterLineMessageContent = afterLineMessageContent.Replace(matchTimeData.Value, $"-:--");
                    }
                    else
                    {
                        afterLineMessageContent = afterLineMessageContent.Replace(matchTimeData.Value, $"{afterMinutes}:{afterSeconds:D2}");
                    }
                }

                foreach (Match matchTimeData in Regex.Matches(lineMessageContent, @"(\d{1,2})([秒s])"))
                {
                    var seconds = int.Parse(matchTimeData.Groups[1].Value);
                    var afterSeconds = seconds - scrapTime;

                    if (afterSeconds <= 0)
                    {
                        afterLineMessageContent = afterLineMessageContent.Replace(matchTimeData.Value, $"--{matchTimeData.Groups[2]}");
                    }
                    else
                    {
                        afterLineMessageContent = afterLineMessageContent.Replace(matchTimeData.Value, $"{afterSeconds:D2}{matchTimeData.Groups[2]}");
                    }

                    
                }

                sendMessageContent.AppendLine(afterLineMessageContent);
            }

            sendMessageContent.AppendLine("```");

            return sendMessageContent.ToString();
        }
    }
}
