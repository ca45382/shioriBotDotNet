using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using PriconneBotConsoleApp.DataModel;

namespace PriconneBotConsoleApp.Script
{
    public class BaseClass
    {
        protected static async Task<RestUserMessage> SendMessageToChannel(ISocketMessageChannel channel, string messageData)
            => await channel.SendMessageAsync(messageData);

        protected static async Task EditMessage(SocketUserMessage message, string messageData)
            => await message.ModifyAsync(msg => msg.Content = messageData);

        /// <summary>
        /// 文字列の0-9,a-z,A-Zを全角から半角に強制的に変更する。
        /// </summary>
        /// <param name="textData"></param>
        /// <returns></returns>
        protected static string ZenToHan(string textData)
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
