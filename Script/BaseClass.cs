using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;

namespace PriconneBotConsoleApp.Script
{
    class BaseClass
    {
        async protected Task<RestMessage> SendMessageToChannel(ISocketMessageChannel channel, string messageData)
        {
            var result = await channel.SendMessageAsync(messageData);
            return result;
        }

        async protected Task EditMessage(SocketUserMessage message, string messageData)
        {
            await message.ModifyAsync(msg => msg.Content = messageData);
            return;
        }

        protected string ZenToHan(string textData)
        {
            var convertText = textData;
            convertText = Regex.Replace(convertText, "　", p => ((char)(p.Value[0] - '　' + ' ')).ToString());
            convertText = Regex.Replace(convertText, "[０-９]", p => ((char)(p.Value[0] - '０' + '0')).ToString());
            convertText = Regex.Replace(convertText, "[ａ-ｚ]", p => ((char)(p.Value[0] - 'ａ' + 'a')).ToString());
            convertText = Regex.Replace(convertText, "[Ａ-Ｚ]", p => ((char)(p.Value[0] - 'Ａ' + 'A')).ToString());
            return convertText;
        }

        
    }
}
