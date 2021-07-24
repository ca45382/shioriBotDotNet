using Discord.WebSocket;
using PriconneBotConsoleApp.Define;
using PriconneBotConsoleApp.Extension;
using System;
using System.Threading.Tasks;

namespace PriconneBotConsoleApp.Script
{
    public class Dice
    {
        private SocketMessage m_UserMessage;

        public Dice(SocketMessage message)
        {
            m_UserMessage = message;
        }

        public async Task Run()
        {
            var splitMessage = m_UserMessage.Content.ZenToHan().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var diceMax = UtilityDefine.DefaultMaxDiceNumber;

            if (splitMessage.Length == 0 || splitMessage[0] != "!dice")
            {
                return;
            }

            if (splitMessage.Length == 2 && int.TryParse(splitMessage[1], out var number))
            {
                diceMax = number;
            }

            var diceResult = new Random().Next(UtilityDefine.DefaultMinDiceNumber, diceMax);
            var sendMessage = $"{diceResult}";
            await m_UserMessage.Channel.SendMessageAsync(sendMessage);
        }
    }
}
