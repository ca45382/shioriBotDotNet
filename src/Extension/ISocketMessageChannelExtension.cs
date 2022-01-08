using System;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace ShioriBot.Extension
{
    public static class ISocketMessageChannelExtension
    {
        /// <summary>
        /// Sends a timed message to this message channel.
        /// </summary>
        /// <param name="channel">The WebSocket-based channel that can send and receive messages</param>
        /// <param name="text">The message to be sent.</param>
        /// <param name="isTTS">Determines whether the message should be read aloud by Discord or not.</param>
        /// <param name="embed">The Discord.EmbedType.Rich Discord.Embed to be sent.</param>
        /// <param name="options">The options to be used when sending the request.</param>
        /// <param name="allowedMentions"> Specifies if notifications are sent for mentioned users and roles in the message 
        /// text. If null, all mentioned roles and users will be notified.</param>
        /// <param name="messageReference">The message references to be included. Used to reply to specific messages.</param>
        /// <param name="component">The message components to be included with this message. Used for interactions.</param>
        /// <param name="displayTime">The time when the message is displayed.</param>
        /// <returns>A task that represents an asynchronous send operation for delivering the message.
        /// The task result contains the sent message.</returns>
        public static async Task<RestMessage> SendTimedMessageAsync(
            this ISocketMessageChannel channel,
            TimeSpan displayTime,
            string text = null,
            bool isTTS = false,
            Embed embed = null,
            RequestOptions options = null,
            AllowedMentions allowedMentions = null,
            MessageReference messageReference = null,
            MessageComponent component = null)
        {
            var messageData = await channel.SendMessageAsync(
                text, isTTS, embed, options, allowedMentions, messageReference, component);

            if (displayTime.TotalSeconds <= 0)
            {
                return messageData;
            }

            _ = DeleteMessageDelayAsync(messageData, displayTime);

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
