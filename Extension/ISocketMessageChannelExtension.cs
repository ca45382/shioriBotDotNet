using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriconneBotConsoleApp.Extension
{
    public static class ISocketMessageChannelExtension
    {
       /// <summary>
       /// 一定時間後に削除する機能を追加したSendMessageAync
       /// </summary>
        public static async Task<RestMessage> SendTimedMessageAsync(this ISocketMessageChannel channel, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent component = null, int timeLimit = 0)
        {
            var delayTime = new TimeSpan(0, 0, timeLimit);
            var messageData = await channel.SendMessageAsync(
                text, isTTS, embed, options, allowedMentions, messageReference, component);
            if (timeLimit <= 0)
            {
                return messageData;
            }

            _ = DeleteMessageDelayAsync(messageData, delayTime);
            
            return messageData;
        }

        private static async Task DeleteMessageDelayAsync(RestUserMessage message, TimeSpan delayTime)
        {
            await Task.Delay(delayTime);

            try
            {
                await message.DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
