using System;
using System.Threading.Tasks;
using ShioriBot.Net.Model;
using ShioriBot.Net.Define;

namespace ShioriBot.Net.Script
{
    public class Dice
    {
        private CommandEventArgs m_CommandEventArgs;

        public Dice(CommandEventArgs commandEventArgs)
            => m_CommandEventArgs = commandEventArgs;

        public async Task Run()
        {
            var diceMax = UtilityDefine.DefaultMaxDiceNumber;

            if (m_CommandEventArgs.Arguments.Count == 1 && int.TryParse(m_CommandEventArgs.Arguments[0], out var number))
            {
                diceMax = number;
            }

            var diceResult = new Random().Next(UtilityDefine.DefaultMinDiceNumber, diceMax);
            var sendMessage = $"{diceResult}";
            await m_CommandEventArgs.Channel.SendMessageAsync(sendMessage);
        }
    }
}
